using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task SaveChangesAsync();

        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByUsernameAsync(string username);

        Task<bool> UsernameExistsAsync(string username);

        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);
    }
}
