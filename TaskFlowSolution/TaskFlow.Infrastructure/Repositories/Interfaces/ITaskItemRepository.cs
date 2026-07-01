using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Models;

namespace TaskFlow.Infrastructure.Repositories.Interfaces
{
    public interface ITaskItemRepository
    {
        Task SaveChangesAsync();

        Task<TaskItem?> GetByIdAsync(Guid id);

        Task<PagedResult<TaskItem>> GetAllByTaskListIdAsync(Guid taskListId, TaskQuery taskQuery);

        Task<List<TaskItem>> GetAllByCategoryIdAsync(Guid categoryId);

        Task<List<TaskItem>> GetCompletedAsync(Guid taskListId);

        Task<List<TaskItem>> GetActiveAsync(Guid taskListId);

        Task AddAsync(TaskItem task);

        Task UpdateAsync(TaskItem task);

        Task DeleteAsync(TaskItem task);

        Task<bool> AnyInTaskListAsync(Guid taskListId);

        Task DeleteAllByTaskListIdAsync(Guid taskListId);

        Task ClearCategoryAsync(Guid categoryId);
    }
}
