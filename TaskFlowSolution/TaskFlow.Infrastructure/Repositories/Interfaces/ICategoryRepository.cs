using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id);

        Task<List<Category>> GetAllByUserIdAsync(Guid userId);

        Task<Category?> GetWithTasksAsync(Guid id);

        Task AddAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(Category category);
    }
}
