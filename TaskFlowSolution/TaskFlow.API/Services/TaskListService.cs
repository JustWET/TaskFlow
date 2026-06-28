using TaskFlow.API.Services.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.API.Services
{
    public class TaskListService : ITaskListService
    {
        private readonly ITaskListRepository _taskListRepository;
        private readonly ITaskItemRepository _taskItemRepository;

        public TaskListService(
            ITaskListRepository taskListRepository,
            ITaskItemRepository taskItemRepository)
        {
            _taskListRepository = taskListRepository;
            _taskItemRepository = taskItemRepository;
        }

        public async Task<TaskList?> GetByIdAsync(Guid currentUserId, Guid taskListId)
        {
            var taskList = await _taskListRepository.GetByIdAsync(taskListId);

            if (taskList == null)
                return null;

            if (taskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            return taskList;
        }

        public async Task<List<TaskList>> GetAllAsync(Guid currentUserId)
        {
            return await _taskListRepository.GetAllByUserIdAsync(currentUserId);
        }

        public async Task<TaskList> CreateAsync(Guid currentUserId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Task list name cannot be empty.", nameof(name));

            var taskList = new TaskList
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                UserId = currentUserId
            };

            await _taskListRepository.AddAsync(taskList);
            await _taskListRepository.SaveChangesAsync();

            return taskList;
        }

        public async Task UpdateAsync(Guid currentUserId, Guid taskListId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Task list name cannot be empty.", nameof(name));

            var taskList = await _taskListRepository.GetByIdAsync(taskListId);

            if (taskList == null)
                throw new InvalidOperationException("Task list not found.");

            if (taskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            taskList.Name = name.Trim();

            await _taskListRepository.UpdateAsync(taskList);
            await _taskListRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid currentUserId, Guid taskListId)
        {
            var taskList = await _taskListRepository.GetByIdAsync(taskListId);

            if (taskList == null)
                throw new InvalidOperationException("Task list not found.");

            if (taskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            if (await _taskItemRepository.AnyInTaskListAsync(taskListId))
                throw new InvalidOperationException("Cannot delete a task list that contains tasks.");

            await _taskListRepository.DeleteAsync(taskList);
            await _taskListRepository.SaveChangesAsync();
        }

        public async Task ClearAsync(Guid currentUserId, Guid taskListId)
        {
            var taskList = await _taskListRepository.GetByIdAsync(taskListId);

            if (taskList == null)
                throw new InvalidOperationException("Task list not found.");

            if (taskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            await _taskItemRepository.DeleteAllByTaskListIdAsync(taskListId);

            await _taskItemRepository.SaveChangesAsync();
        }
    }
}
