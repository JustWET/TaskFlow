using Moq;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;
using TaskFlow.API.Services.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;

        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();

            _userService = new UserService(
                _userRepositoryMock.Object, 
                _tokenServiceMock.Object);
        }

        private static User CreateUser(string username = "John")
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = [1, 2, 3],
                PasswordSalt = [4, 5, 6],
            };
        }

        private static User CreateUserWithPassword(
            string username = "John",
            string password = "12345678")
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();

            return new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(password))
            };
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user, result);

            _userRepositoryMock.Verify(
                r => r.GetByIdAsync(user.Id),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByIdAsync(id);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                r => r.GetByIdAsync(id),
                Times.Once);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(user.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetByUsernameAsync(user.Username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user, result);

            _userRepositoryMock.Verify(
                r => r.GetByUsernameAsync(user.Username),
                Times.Once);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            const string username = "John";

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByUsernameAsync(username);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                r => r.GetByUsernameAsync(username),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("    ")]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenUsernameIsInvalid(string username)
        {
            // Arrange
            const string password = "123456";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.RegisterAsync(username, password));

            _userRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345")]
        public async Task RegisterAsync_ShouldThrowArgumentOutOfRangeException_WhenPasswordIsInvalid(string password)
        {
            // Arrange
            const string username = "John";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _userService.RegisterAsync(username, password));

            _userRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldTrimUsername_WhenUsernameContainsSpaces()
        {
            // Arrange
            const string username = "   John   ";
            const string password = "12345678";
            const string token = "jwt-token";

            User? createdUser = null;

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("John"))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => createdUser = u)
                .Returns(Task.CompletedTask);

            _tokenServiceMock
                .Setup(t => t.GenerateToken(It.IsAny<Guid>(), "John"))
                .Returns(token);

            // Act
            await _userService.RegisterAsync(username, password);

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal("John", createdUser!.Username);

            _userRepositoryMock.Verify(
                r => r.GetByUsernameAsync("John"),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenUsernameAlreadyExists()
        {
            // Arrange
            var existingUser = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(existingUser.Username))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.RegisterAsync(existingUser.Username, "12345678"));

            _userRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUserAndReturnJwtToken_WhenDataIsValid()
        {
            // Arrange
            const string username = "John";
            const string password = "12345678";
            const string jwt = "jwt-token";

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _tokenServiceMock
                .Setup(t => t.GenerateToken(It.IsAny<Guid>(), username))
                .Returns(jwt);

            // Act
            var result = await _userService.RegisterAsync(username, password);

            // Assert
            Assert.Equal(jwt, result);

            _userRepositoryMock.Verify(
                r => r.AddAsync(It.Is<User>(u =>
                    u.Id != Guid.Empty &&
                    u.Username == username &&
                    u.PasswordHash.Length > 0 &&
                    u.PasswordSalt.Length > 0)),
                Times.Once);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), username),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task LoginAsync_ShouldThrowArgumentException_WhenUsernameIsInvalid(string username)
        {
            // Arrange
            const string password = "12345678";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.LoginAsync(username, password));

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task LoginAsync_ShouldThrowArgumentException_WhenPasswordIsInvalid(string password)
        {
            // Arrange
            const string username = "John";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.LoginAsync(username, password));

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldTrimUsername_WhenUsernameContainsSpaces()
        {
            // Arrange
            const string username = "   John   ";
            const string password = "12345678";
            const string token = "jwt-token";

            var user = CreateUserWithPassword("John", password);

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("John"))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(t => t.GenerateToken(user.Id, user.Username))
                .Returns(token);

            // Act
            var result = await _userService.LoginAsync(username, password);

            // Assert
            Assert.Equal(token, result);

            _userRepositoryMock.Verify(
                r => r.GetByUsernameAsync("John"),
                Times.Once);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(user.Id, user.Username),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            const string username = "John";
            const string password = "12345678";

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LoginAsync(username, password);

            // Assert
            Assert.Null(result);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var user = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(user.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.LoginAsync(user.Username, "WrongPassword");

            // Assert
            Assert.Null(result);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnJwt_WhenCredentialsAreValid()
        {
            // Arrange
            const string username = "John";
            const string password = "12345678";
            const string jwt = "jwt-token";

            var user = CreateUserWithPassword(username, password);

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(t => t.GenerateToken(user.Id, user.Username))
                .Returns(jwt);

            // Act
            var result = await _userService.LoginAsync(username, password);

            // Assert
            Assert.Equal(jwt, result);

            _tokenServiceMock.Verify(
                t => t.GenerateToken(user.Id, user.Username),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenUsernameIsInvalid(string username)
        {
            // Arrange
            var dto = new UserDto
            {
                Username = username
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.UpdateAsync(Guid.NewGuid(), dto));

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldTrimUsername_WhenUsernameContainsSpaces()
        {
            // Arrange
            var user = CreateUser("John");

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.UsernameExistsAsync("John Smith"))
                .ReturnsAsync(false);

            var dto = new UserDto
            {
                Username = "   John Smith   "
            };

            // Act
            await _userService.UpdateAsync(user.Id, dto);

            // Assert
            Assert.Equal("John Smith", user.Username);

            _userRepositoryMock.Verify(
                r => r.UsernameExistsAsync("John Smith"),
                Times.Once);

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var dto = new UserDto
            {
                Username = "NewUsername"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.UpdateAsync(id, dto));

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenUsernameAlreadyExists()
        {
            // Arrange
            var user = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.UsernameExistsAsync("Kate"))
                .ReturnsAsync(true);

            var dto = new UserDto
            {
                Username = "Kate"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.UpdateAsync(user.Id, dto));

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotCheckUsernameUniqueness_WhenUsernameIsNotChanged()
        {
            // Arrange
            var user = CreateUser("John");

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            var dto = new UserDto
            {
                Username = "John"
            };

            // Act
            await _userService.UpdateAsync(user.Id, dto);

            // Assert
            Assert.Equal("John", user.Username);

            _userRepositoryMock.Verify(
                r => r.UsernameExistsAsync(It.IsAny<string>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(user),
                Times.Once);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUsername_WhenDataIsValid()
        {
            // Arrange
            var user = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.UsernameExistsAsync("Kate"))
                .ReturnsAsync(false);

            var dto = new UserDto
            {
                Username = "Kate"
            };

            // Act
            await _userService.UpdateAsync(user.Id, dto);

            // Assert
            Assert.Equal("Kate", user.Username);

            _userRepositoryMock.Verify(
                r => r.UpdateAsync(user),
                Times.Once);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.DeleteAsync(id));

            _userRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<User>()),
                Times.Never);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var user = CreateUser();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            await _userService.DeleteAsync(user.Id);

            // Assert
            _userRepositoryMock.Verify(
                r => r.DeleteAsync(user),
                Times.Once);

            _userRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }
    }
}
