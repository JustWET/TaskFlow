using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Services.Interfaces
{
    public interface ITaskListService
    {
        Task<TaskList?> GetByIdAsync(Guid currentUserId, Guid taskListId);

        Task<List<TaskList>> GetAllAsync(Guid currentUserId);

        Task<TaskList> CreateAsync(Guid currentUserId, string name);

        Task UpdateAsync(Guid currentUserId, Guid taskListId, string name);

        Task DeleteAsync(Guid currentUserId, Guid taskListId);

        Task ClearAsync(Guid currentUserId, Guid taskListId);
    }
}
