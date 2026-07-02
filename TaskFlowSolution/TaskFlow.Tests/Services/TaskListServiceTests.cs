using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.API.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.Tests.Services
{
    public class TaskListServiceTests
    {
        private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;

        private readonly TaskListService _taskListService;

        public TaskListServiceTests()
        {
            _taskListRepositoryMock = new Mock<ITaskListRepository>();
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();

            _taskListService = new TaskListService(
                _taskListRepositoryMock.Object,
                _taskItemRepositoryMock.Object);
        }

        private static TaskList CreateTaskList(
            Guid? userId = null,
            string name = "My Task List")
        {
            return new TaskList
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Name = name
            };
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTaskList_WhenTaskListExistsAndBelongsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskList = CreateTaskList(userId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act
            var result = await _taskListService.GetByIdAsync(userId, taskList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskList, result);

            _taskListRepositoryMock.Verify(
                r => r.GetByIdAsync(taskList.Id),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTaskListDoesNotExist()
        {
            // Arrange
            var taskListId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskListId))
                .ReturnsAsync((TaskList?)null);

            // Act
            var result = await _taskListService.GetByIdAsync(Guid.NewGuid(), taskListId);

            // Assert
            Assert.Null(result);

            _taskListRepositoryMock.Verify(
                r => r.GetByIdAsync(taskListId),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowUnauthorizedAccessException_WhenTaskListBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var taskList = CreateTaskList(ownerId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskListService.GetByIdAsync(anotherUserId, taskList.Id));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUserTaskLists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskLists = new List<TaskList>
            {
                CreateTaskList(userId, "Home"),
                CreateTaskList(userId, "Work"),
                CreateTaskList(userId, "Shopping")
            };

            _taskListRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(userId))
                .ReturnsAsync(taskLists);

            // Act
            var result = await _taskListService.GetAllAsync(userId);

            // Assert
            Assert.Equal(taskLists, result);

            _taskListRepositoryMock.Verify(
                r => r.GetAllByUserIdAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenUserHasNoTaskLists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetAllByUserIdAsync(userId))
                .ReturnsAsync(new List<TaskList>());

            // Act
            var result = await _taskListService.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _taskListRepositoryMock.Verify(
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
                _taskListService.CreateAsync(userId, name));

            _taskListRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<TaskList>()),
                Times.Never);

            _taskListRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTaskList_WhenNameIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string name = "My Task List";

            // Act
            var result = await _taskListService.CreateAsync(userId, name);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(name, result.Name);

            _taskListRepositoryMock.Verify(
                r => r.AddAsync(It.Is<TaskList>(t =>
                    t.Id != Guid.Empty &&
                    t.UserId == userId &&
                    t.Name == name)),
                Times.Once);

            _taskListRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _taskListService.CreateAsync(userId, "   My List   ");

            // Assert
            Assert.Equal("My List", result.Name);

            _taskListRepositoryMock.Verify(
                r => r.AddAsync(It.Is<TaskList>(t =>
                    t.Name == "My List")),
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
            var taskListId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskListService.UpdateAsync(userId, taskListId, name));

            _taskListRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);

            _taskListRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<TaskList>()),
                Times.Never);

            _taskListRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId, "Old");

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act
            await _taskListService.UpdateAsync(
                userId,
                taskList.Id,
                "   New Name   ");

            // Assert
            Assert.Equal("New Name", taskList.Name);

            _taskListRepositoryMock.Verify(
                r => r.UpdateAsync(taskList),
                Times.Once);

            _taskListRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenTaskListDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskListId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskListId))
                .ReturnsAsync((TaskList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskListService.UpdateAsync(userId, taskListId, "New"));

            _taskListRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<TaskList>()),
                Times.Never);

            _taskListRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenTaskListBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var taskList = CreateTaskList(ownerId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskListService.UpdateAsync(currentUserId, taskList.Id, "Updated"));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenTaskListDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskListId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskListId))
                .ReturnsAsync((TaskList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskListService.DeleteAsync(userId, taskListId));

            _taskItemRepositoryMock.Verify(
                r => r.AnyInTaskListAsync(It.IsAny<Guid>()),
                Times.Never);

            _taskListRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<TaskList>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenTaskListBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var taskList = CreateTaskList(ownerId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskListService.DeleteAsync(currentUserId, taskList.Id));
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenTaskListContainsTasks()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            _taskItemRepositoryMock
                .Setup(r => r.AnyInTaskListAsync(taskList.Id))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskListService.DeleteAsync(userId, taskList.Id));

            _taskListRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<TaskList>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTaskList_WhenItContainsNoTasks()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            _taskItemRepositoryMock
                .Setup(r => r.AnyInTaskListAsync(taskList.Id))
                .ReturnsAsync(false);

            // Act
            await _taskListService.DeleteAsync(userId, taskList.Id);

            // Assert
            _taskListRepositoryMock.Verify(
                r => r.DeleteAsync(taskList),
                Times.Once);

            _taskListRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ClearAsync_ShouldThrowInvalidOperationException_WhenTaskListDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskListId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskListId))
                .ReturnsAsync((TaskList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskListService.ClearAsync(userId, taskListId));

            _taskItemRepositoryMock.Verify(
                r => r.DeleteAllByTaskListIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task ClearAsync_ShouldThrowUnauthorizedAccessException_WhenTaskListBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var taskList = CreateTaskList(ownerId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskListService.ClearAsync(currentUserId, taskList.Id));
        }

        [Fact]
        public async Task ClearAsync_ShouldRemoveAllTasksFromTaskList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act
            await _taskListService.ClearAsync(userId, taskList.Id);

            // Assert
            _taskItemRepositoryMock.Verify(
                r => r.DeleteAllByTaskListIdAsync(taskList.Id),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }
    }
}
