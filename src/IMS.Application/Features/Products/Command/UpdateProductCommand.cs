using AutoMapper;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Products.Commands;

public record UpdateProductCommand(
    int Id,
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId
) : IRequest<ProductDto>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {request.Id} was not found.");

        if (await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken) == null)
            throw new KeyNotFoundException($"Category with ID {request.CategoryId} does not exist.");

        if (await _productRepository.ExistsBySkuAsync(request.SKU, request.Id, cancellationToken))
            throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists.");

        product.Name = request.Name;
        product.SKU = request.SKU;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;

        await _productRepository.UpdateAsync(product, cancellationToken);
        _logger.LogInformation("Product ID {ProductId} updated successfully.", request.Id);

        var updatedProduct = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        return _mapper.Map<ProductDto>(updatedProduct);
    }
}