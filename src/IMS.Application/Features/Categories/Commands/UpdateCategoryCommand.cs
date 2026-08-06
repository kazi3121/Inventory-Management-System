using AutoMapper;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Categories.Commands;

public record UpdateCategoryCommand(int Id, string Name, string Description) : IRequest<CategoryDto>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {request.Id} was not found.");

        bool nameExists = await _categoryRepository.ExistsByNameAsync(request.Name, request.Id, cancellationToken);
        if (nameExists)
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists.");
        }

        category.Name = request.Name;
        category.Description = request.Description;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        _logger.LogInformation("Category ID {CategoryId} updated successfully.", request.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}