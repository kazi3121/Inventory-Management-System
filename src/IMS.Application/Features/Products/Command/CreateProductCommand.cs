using AutoMapper;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId
) : IRequest<ProductDto>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with SKU: {SKU}", request.SKU);

        // Verify Category Exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {request.CategoryId} does not exist.");

        // Unique SKU check
        if (await _productRepository.ExistsBySkuAsync(request.SKU, null, cancellationToken))
            throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists.");

        var product = new Product
        {
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId
        };

        await _productRepository.AddAsync(product, cancellationToken);
        
        // Reload to include Category navigation property for mapper
        var createdProduct = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        return _mapper.Map<ProductDto>(createdProduct);
    }
}