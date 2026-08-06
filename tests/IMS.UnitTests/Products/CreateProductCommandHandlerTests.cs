using AutoMapper;
using FluentAssertions;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Application.Features.Products.Commands;
using IMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;


namespace IMS.UnitTests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateProductCommandHandler>>();

        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateProductAndReturnDto_WhenCategoryExistsAndSkuIsUnique()
    {
        // Arrange
        var command = new CreateProductCommand("Mechanical Keyboard", "SKU-KB-001", "RGB Keyboard", 99.99m, 15, 1);
        var existingCategory = new Category { Id = 1, Name = "Electronics", Description = "Tech gadgets" };

        var reloadedProduct = new Product
        {
            Id = 10,
            Name = command.Name,
            SKU = command.SKU,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            CategoryId = command.CategoryId,
            Category = existingCategory
        };

        var expectedDto = new ProductDto(
            10, command.Name, command.SKU, command.Description,
            command.Price, command.StockQuantity, command.CategoryId,
            existingCategory.Name, DateTime.UtcNow
        );

        // 1. Mock Category lookup
        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        // 2. Mock Unique SKU check
        _productRepositoryMock
            .Setup(x => x.ExistsBySkuAsync(command.SKU, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // 3. Mock AddAsync setting product ID
        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => product.Id = 10)
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // 4. Mock GetByIdAsync call (reloading product with Category included)
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloadedProduct);

        // 5. Mock AutoMapper
        _mapperMock
            .Setup(x => x.Map<ProductDto>(reloadedProduct))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.Name.Should().Be(command.Name);
        result.SKU.Should().Be(command.SKU);
        result.CategoryName.Should().Be("Electronics");

        _productRepositoryMock.Verify(x => x.AddAsync(It.Is<Product>(p =>
            p.Name == command.Name &&
            p.SKU == command.SKU &&
            p.Price == command.Price &&
            p.CategoryId == command.CategoryId
        ), It.IsAny<CancellationToken>()), Times.Once);

        _productRepositoryMock.Verify(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowKeyNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateProductCommand("Mouse", "SKU-MOU-001", "Wireless Mouse", 29.99m, 10, 999);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with ID {command.CategoryId} does not exist.");

        _productRepositoryMock.Verify(x => x.ExistsBySkuAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenSkuAlreadyExists()
    {
        // Arrange
        var command = new CreateProductCommand("Mouse", "SKU-DUP-001", "Wireless Mouse", 29.99m, 10, 1);
        var existingCategory = new Category { Id = 1, Name = "Electronics" };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _productRepositoryMock
            .Setup(x => x.ExistsBySkuAsync(command.SKU, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Product with SKU '{command.SKU}' already exists.");

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}