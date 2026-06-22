using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Contexts;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.Infrastructure.Repositories
{
    public class TaskListRepository : ITaskListRepository
    {
        private readonly TodoDbContext _context;

        public TaskListRepository(TodoDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<TaskList?> GetByIdAsync(Guid id)
        {
            return await _context.TaskLists
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TaskList>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.TaskLists
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(TaskList taskList)
        {
            await _context.TaskLists.AddAsync(taskList);
        }

        public Task UpdateAsync(TaskList taskList)
        {
            _context.TaskLists.Update(taskList);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(TaskList taskList)
        {
            _context.TaskLists.Remove(taskList);

            return Task.CompletedTask;
        }
    }
}
