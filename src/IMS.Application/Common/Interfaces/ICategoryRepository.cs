using IMS.Domain.Entities;

namespace IMS.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<(IReadOnlyList<Category> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken = default);
    
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    
    Task<bool> HasProductsAsync(int categoryId, CancellationToken cancellationToken = default);
}