using TaskFlow.API.DTOs;
using TaskFlow.API.Services.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<string> RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));

            username = username.Trim();

            var existing = await _userRepository.GetByUsernameAsync(username);

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ArgumentOutOfRangeException(
                    nameof(password),
                    "Password must be at least 6 characters long.");

            if (existing != null)
                throw new InvalidOperationException("Username already exists.");

            CreatePasswordHash(password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return _tokenService.GenerateToken(user.Id, user.Username);
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            username = username.Trim();

            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return _tokenService.GenerateToken(user.Id, user.Username);
        }

        public async Task UpdateAsync(Guid currentUserId, UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username))
                throw new InvalidOperationException("Username cannot be empty.");

            userDto.Username = userDto.Username.Trim();

            var user = await _userRepository.GetByIdAsync(currentUserId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (user.Username != userDto.Username && 
                await _userRepository.UsernameExistsAsync(userDto.Username))
                throw new InvalidOperationException("Username already exists.");
            
            user.Username = userDto.Username;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid currentUserId)
        {
            var user = await _userRepository.GetByIdAsync(currentUserId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
        }


        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(salt);
            var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(hash);
        }
    }
}
