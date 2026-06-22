using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories.Interfaces
{
    public interface ITaskItemRepository
    {
        Task<TaskItem?> GetByIdAsync(Guid id);

        Task<List<TaskItem>> GetAllByTaskListIdAsync(Guid taskListId);

        Task<List<TaskItem>> GetAllByCategoryIdAsync(Guid categoryId);

        Task<List<TaskItem>> GetCompletedAsync(Guid taskListId);

        Task<List<TaskItem>> GetActiveAsync(Guid taskListId);

        Task AddAsync(TaskItem task);

        Task UpdateAsync(TaskItem task);

        Task DeleteAsync(TaskItem task);
    }
}
