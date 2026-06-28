using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category?> GetByIdAsync(Guid currentUserId, Guid categoryId);

        Task<List<Category>> GetAllAsync(Guid currentUserId);

        Task<Category> CreateAsync(Guid currentUserId, string name);

        Task UpdateAsync(Guid currentUserId, Guid categoryId, string name);

        Task DeleteAsync(Guid currentUserId, Guid categoryId);
    }
}
