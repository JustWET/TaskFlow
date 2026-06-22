using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories.Interfaces
{
    public interface ITaskListRepository
    {
        Task<TaskList?> GetByIdAsync(Guid id);

        Task<List<TaskList>> GetAllByUserIdAsync(Guid userId);

        Task AddAsync(TaskList taskList);

        Task UpdateAsync(TaskList taskList);

        Task DeleteAsync(TaskList taskList);
    }
}
