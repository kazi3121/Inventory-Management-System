using AutoMapper;
using FluentAssertions;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Application.Features.Categories.Commands;
using IMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;


namespace IMS.UnitTests.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateCategoryCommandHandler>> _loggerMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateCategoryCommandHandler>>();

        _handler = new CreateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateCategoryAndReturnDto_WhenNameIsUnique()
    {
        // Arrange
        var command = new CreateCategoryCommand("Electronics", "Gadgets and devices");
        var categoryDto = new CategoryDto(1, command.Name, command.Description, DateTime.UtcNow);

        // 1. ExistsByNameAsync returns false
        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // 2. Simulate database assigning Id on AddAsync
        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((cat, _) => cat.Id = 1)
            .ReturnsAsync((Category cat, CancellationToken _) => cat);

        // 3. Map Category entity to CategoryDto
        _mapperMock
            .Setup(x => x.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(categoryDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(command.Name);

        _categoryRepositoryMock.Verify(x => x.AddAsync(It.Is<Category>(c =>
            c.Name == command.Name &&
            c.Description == command.Description
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenCategoryNameAlreadyExists()
    {
        // Arrange
        var command = new CreateCategoryCommand("Electronics", "Duplicate category");

        _categoryRepositoryMock
            .Setup(x => x.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Category with name '{command.Name}' already exists.");

        _categoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(x => x.Map<CategoryDto>(It.IsAny<Category>()), Times.Never);
    }
}