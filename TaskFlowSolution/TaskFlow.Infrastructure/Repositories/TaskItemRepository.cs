using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
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
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TaskItem>> GetAllByTaskListIdAsync(Guid taskListId)
        {
            return await _context.Tasks
                .Where(t => t.TaskListId == taskListId)
                .ToListAsync();
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
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
