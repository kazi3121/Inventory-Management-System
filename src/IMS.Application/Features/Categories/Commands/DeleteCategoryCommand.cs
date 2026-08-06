using IMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Categories.Commands;

public record DeleteCategoryCommand(int Id) : IRequest<Unit>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {request.Id} was not found.");

        bool hasProducts = await _categoryRepository.HasProductsAsync(request.Id, cancellationToken);
        if (hasProducts)
        {
            throw new InvalidOperationException($"Cannot delete category '{category.Name}' because it contains associated products. Reassign or remove products first.");
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        _logger.LogInformation("Category ID {CategoryId} deleted successfully.", request.Id);

        return Unit.Value;
    }
}