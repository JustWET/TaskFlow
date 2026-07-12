using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Models;
using TaskFlow.Infrastructure.Contexts;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.Infrastructure.Repositories
{
    public class TaskItemRepository : ITaskItemRepository
    {
        private readonly TodoDbContext _context;

        public TaskItemRepository(TodoDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks
                .Include(t => t.TaskList)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PagedResult<TaskItem>> GetAllByTaskListIdAsync(Guid taskListId, TaskQuery taskQuery)
        {
            var query = _context.Tasks
                .Where(t => t.TaskListId == taskListId);

            if (!string.IsNullOrWhiteSpace(taskQuery.Search))
            {
                query = query.Where(t =>
                    t.Name.Contains(taskQuery.Search));
            }

            if (taskQuery.CategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == taskQuery.CategoryId.Value);
            }

            switch (taskQuery.SortBy)
            {
                case TaskSortBy.Name:
                    query = taskQuery.Descending
                        ? query.OrderByDescending(t => t.Name)
                        : query.OrderBy(t => t.Name);
                    break;

                case TaskSortBy.DueDate:
                    query = taskQuery.Descending
                        ? query.OrderByDescending(t => t.DueDate)
                        : query.OrderBy(t => t.DueDate);
                    break;

                case TaskSortBy.Priority:
                    query = taskQuery.Descending
                        ? query.OrderByDescending(t => t.Priority)
                        : query.OrderBy(t => t.Priority);
                    break;
                default:
                    query = query.OrderBy(t => t.Name);
                    break;
            }

            var totalCount = await query.CountAsync();

            var page = Math.Max(taskQuery.Page, 1);
            var pageSize = Math.Clamp(taskQuery.PageSize, 1, 100);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<TaskItem>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<TaskItem>> GetAllByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Tasks
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetCompletedAsync(Guid taskListId)
        {
            return await _context.Tasks
                .Where(t => t.TaskListId == taskListId && t.IsCompleted)
                .ToListAsync();
        }

        public async Task<List<TaskItem>> GetActiveAsync(Guid taskListId)
        {
            return await _context.Tasks
                .Where(t => t.TaskListId == taskListId && !t.IsCompleted)
                .ToListAsync();
        }

        public async Task AddAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);
        }

        public Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);

            return Task.CompletedTask;
        }

        public async Task<bool> AnyInTaskListAsync(Guid taskListId)
        {
            return await _context.Tasks
                .AnyAsync(t => t.TaskListId == taskListId);
        }

        public async Task DeleteAllByTaskListIdAsync(Guid taskListId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.TaskListId == taskListId)
                .ToListAsync();

            _context.Tasks.RemoveRange(tasks);
        }

        public async Task ClearCategoryAsync(Guid categoryId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.CategoryId = null;
            }
        }
    }
}
