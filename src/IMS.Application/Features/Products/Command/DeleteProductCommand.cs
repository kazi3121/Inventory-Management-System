using IMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<Unit>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IProductRepository productRepository, ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {request.Id} was not found.");

        await _productRepository.DeleteAsync(product, cancellationToken);
        _logger.LogInformation("Product ID {ProductId} deleted.", request.Id);

        return Unit.Value;
    }
}