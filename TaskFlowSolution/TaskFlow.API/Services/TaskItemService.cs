using TaskFlow.API.DTOs;
using TaskFlow.API.Services.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Models;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.API.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemRepository _taskItemRepository;
        private readonly ITaskListRepository _taskListRepository;
        private readonly ICategoryRepository _categoryRepository;

        public TaskItemService(
            ITaskItemRepository taskItemRepository,
            ITaskListRepository taskListRepository,
            ICategoryRepository categoryRepository)
        {
            _taskItemRepository = taskItemRepository;
            _taskListRepository = taskListRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<TaskItem?> GetByIdAsync(Guid currentUserId, Guid taskId)
        {
            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                return null;

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            return task;
        }

        public async Task<PagedResult<TaskItem>> GetAllAsync(Guid currentUserId, Guid taskListId, TaskQuery taskQuery)
        {
            var taskList = await _taskListRepository.GetByIdAsync(taskListId);

            if (taskList == null)
                throw new InvalidOperationException("Task list not found.");

            if (taskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            return await _taskItemRepository.GetAllByTaskListIdAsync(taskListId, taskQuery);
        }

        public async Task<TaskItem> CreateAsync(Guid currentUserId, Guid taskListId, CreateOrUpdateTaskDto taskDto)
        {
            ValidateTaskDto(taskDto.Name, taskDto.Priority);

            var taskList = await _taskListRepository.GetByIdAsync(taskListId);

            if (taskList == null)
                throw new InvalidOperationException("Task list not found.");

            if (taskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            await ValidateCategoryAsync(currentUserId, taskDto.CategoryId);

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                TaskListId = taskListId,
                CategoryId = taskDto.CategoryId,
                Name = taskDto.Name.Trim(),
                IsCompleted = taskDto.IsCompleted,
                DueDate = taskDto.DueDate,
                Priority = taskDto.Priority,
                Description = taskDto.Description
            };

            await _taskItemRepository.AddAsync(task);
            await _taskItemRepository.SaveChangesAsync();

            return task;
        }

        public async Task UpdateAsync(Guid currentUserId, Guid taskId, CreateOrUpdateTaskDto taskDto)
        {
            ValidateTaskDto(taskDto.Name, taskDto.Priority);

            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                throw new InvalidOperationException("Task not found.");

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            await ValidateCategoryAsync(currentUserId, taskDto.CategoryId);

            task.Name = taskDto.Name.Trim();
            task.CategoryId = taskDto.CategoryId;
            task.IsCompleted = taskDto.IsCompleted;
            task.Priority = taskDto.Priority;
            task.DueDate = taskDto.DueDate;
            task.Description = taskDto.Description;

            await _taskItemRepository.UpdateAsync(task);
            await _taskItemRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid currentUserId, Guid taskId)
        {
            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                throw new InvalidOperationException("Task not found.");

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            await _taskItemRepository.DeleteAsync(task);
            await _taskItemRepository.SaveChangesAsync();
        }

        public Task CompleteAsync(Guid currentUserId, Guid taskId)
        {
            return SetCompletedAsync(currentUserId, taskId, true);
        }

        public Task UncompleteAsync(Guid currentUserId, Guid taskId)
        {
            return SetCompletedAsync(currentUserId, taskId, false);
        }

        public async Task RenameAsync(Guid currentUserId, Guid taskId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Task name cannot be empty.", nameof(name));

            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                throw new InvalidOperationException("Task not found.");

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            task.Name = name.Trim();

            await _taskItemRepository.UpdateAsync(task);
            await _taskItemRepository.SaveChangesAsync();
        }

        public async Task ChangePriorityAsync(Guid currentUserId, Guid taskId, Priority priority)
        {
            if (!Enum.IsDefined(priority))
                throw new ArgumentException("Invalid priority.", nameof(priority));

            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                throw new InvalidOperationException("Task not found.");

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            task.Priority = priority;

            await _taskItemRepository.UpdateAsync(task);
            await _taskItemRepository.SaveChangesAsync();
        }

        private async Task SetCompletedAsync(Guid currentUserId, Guid taskId, bool completed)
        {
            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                throw new InvalidOperationException("Task not found.");

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            task.IsCompleted = completed;

            await _taskItemRepository.UpdateAsync(task);
            await _taskItemRepository.SaveChangesAsync();
        }

        public async Task ChangeDueDateAsync(Guid currentUserId, Guid taskId, DateTime? dueDate)
        {
            var task = await _taskItemRepository.GetByIdAsync(taskId);

            if (task == null)
                throw new InvalidOperationException("Task not found.");

            if (task.TaskList.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            task.DueDate = dueDate;

            await _taskItemRepository.UpdateAsync(task);
            await _taskItemRepository.SaveChangesAsync();
        }

        private static void ValidateTaskDto(string name, Priority priority)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Task name cannot be empty.", nameof(name));

            if (!Enum.IsDefined(priority))
                throw new ArgumentException("Invalid task priority.", nameof(priority));
        }

        private async Task ValidateCategoryAsync(Guid currentUserId, Guid? categoryId)
        {
            if (categoryId == null)
                return;

            var category = await _categoryRepository.GetByIdAsync(categoryId.Value);

            if (category == null)
                throw new InvalidOperationException("Category not found.");

            if (category.UserId != currentUserId)
                throw new UnauthorizedAccessException();
        }
    }
}
