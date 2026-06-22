using TaskFlow.API.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByUsernameAsync(string username);

        Task<string> RegisterAsync(string username, string password);

        Task<string?> LoginAsync(string username, string password);

        Task UpdateAsync(Guid currentUserId, UserDto userDto);

        Task DeleteAsync(Guid currentUserId);
    }
}
