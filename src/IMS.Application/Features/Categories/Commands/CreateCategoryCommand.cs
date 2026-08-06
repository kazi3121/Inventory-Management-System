using AutoMapper;
using IMS.Application.Common.Interfaces;
using IMS.Application.DTOs;
using IMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IMS.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, string Description) : IRequest<CategoryDto>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new category with name: {CategoryName}", request.Name);

        bool exists = await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken : cancellationToken);
        if (exists)
        {
            _logger.LogWarning("Category creation failed. Name '{CategoryName}' already exists.", request.Name);
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists.");
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
            // Id and CreatedAt are handled automatically by BaseEntity / DbContext!
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}