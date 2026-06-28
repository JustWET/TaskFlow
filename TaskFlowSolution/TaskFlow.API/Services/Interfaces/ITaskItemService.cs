using TaskFlow.API.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Services.Interfaces
{
    public interface ITaskItemService
    {
        Task<TaskItem?> GetByIdAsync(Guid currentUserId, Guid taskId);

        Task<List<TaskItem>> GetAllAsync(Guid currentUserId, Guid taskListId);

        Task<TaskItem> CreateAsync(
            Guid currentUserId,
            Guid taskListId,
            CreateOrUpdateTaskDto taskDto);

        Task UpdateAsync(
            Guid currentUserId,
            Guid taskId,
            CreateOrUpdateTaskDto taskDto);

        Task DeleteAsync(Guid currentUserId, Guid taskId);

        Task CompleteAsync(Guid currentUserId, Guid taskId);

        Task UncompleteAsync(Guid currentUserId, Guid taskId);

        Task RenameAsync(Guid currentUserId, Guid taskId, string name);

        Task ChangePriorityAsync(Guid currentUserId, Guid taskId, Priority priority);

        Task ChangeDueDateAsync(Guid currentUserId, Guid taskId, DateTime? dueDate);
    }
}
