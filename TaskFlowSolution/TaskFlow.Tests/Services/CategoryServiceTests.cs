using Moq;
using TaskFlow.API.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;

        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();

            _categoryService = new CategoryService(
                _categoryRepositoryMock.Object,
                _taskItemRepositoryMock.Object);
        }

        private static Category CreateCategory(Guid? userId = null, string name = "Work")
        {
            return new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Name = name
            };
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExistsAndBelongsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = CreateCategory(userId);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetByIdAsync(userId, category.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result);

            _categoryRepositoryMock.Verify(
                r => r.GetByIdAsync(category.Id),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetByIdAsync(Guid.NewGuid(), categoryId);

            // Assert
            Assert.Null(result);

            _categoryRepositoryMock.Verify(
                r => r.GetByIdAsync(categoryId),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowUnauthorizedAccessException_WhenCategoryBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var category = CreateCategory(ownerId);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _categoryService.GetByIdAsync(anotherUserId, category.Id));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUserCategories()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var categories = new List<Category>
            {
                CreateCategory(userId, "Work"),
                CreateCategory(userId, "Home"),
                CreateCategory(userId, "Shopping")
            };

            _categoryRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(userId))
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllAsync(userId);

            // Assert
            Assert.Equal(categories, result);

            _categoryRepositoryMock.Verify(
                r => r.GetAllByUserIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenUserHasNoCategories()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(userId))
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _categoryService.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _categoryRepositoryMock.Verify(
                r => r.GetAllByUserIdAsync(userId),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenNameIsInvalid(string name)
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.CreateAsync(userId, name));

            _categoryRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Category>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _categoryService.CreateAsync(
                userId,
                "   Work   ");

            // Assert
            Assert.Equal("Work", result.Name);

            _categoryRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Category>(c =>
                    c.Name == "Work")),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateCategory_WhenNameIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string name = "Work";

            // Act
            var result = await _categoryService.CreateAsync(userId, name);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(name, result.Name);
            Assert.Equal(userId, result.UserId);
            Assert.NotEqual(Guid.Empty, result.Id);

            _categoryRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Category>(c =>
                    c.Name == name &&
                    c.UserId == userId &&
                    c.Id != Guid.Empty)),
                Times.Once);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenNameIsInvalid(string name)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.UpdateAsync(userId, categoryId, name));

            _categoryRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Category>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var category = CreateCategory(userId, "Old");

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act
            await _categoryService.UpdateAsync(
                userId,
                category.Id,
                "   New Category   ");

            // Assert
            Assert.Equal("New Category", category.Name);

            _categoryRepositoryMock.Verify(
                r => r.UpdateAsync(category),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _categoryService.UpdateAsync(userId, categoryId, "Work"));

            _categoryRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Category>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenCategoryBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var category = CreateCategory(ownerId);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _categoryService.UpdateAsync(currentUserId, category.Id, "Updated"));

            _categoryRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Category>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenDataIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var category = CreateCategory(userId, "Old");

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act
            await _categoryService.UpdateAsync(userId, category.Id, "New");

            // Assert
            Assert.Equal("New", category.Name);

            _categoryRepositoryMock.Verify(
                r => r.UpdateAsync(category),
                Times.Once);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _categoryService.DeleteAsync(userId, categoryId));

            _taskItemRepositoryMock.Verify(
                r => r.ClearCategoryAsync(It.IsAny<Guid>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<Category>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenCategoryBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var category = CreateCategory(ownerId);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _categoryService.DeleteAsync(currentUserId, category.Id));

            _taskItemRepositoryMock.Verify(
                r => r.ClearCategoryAsync(It.IsAny<Guid>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<Category>()),
                Times.Never);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCategory_WhenCategoryExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var category = CreateCategory(userId);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act
            await _categoryService.DeleteAsync(userId, category.Id);

            // Assert
            _taskItemRepositoryMock.Verify(
                r => r.ClearCategoryAsync(category.Id),
                Times.Once);

            _categoryRepositoryMock.Verify(
                r => r.DeleteAsync(category),
                Times.Once);

            _categoryRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }
    }
}
