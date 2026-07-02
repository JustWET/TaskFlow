using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Models;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.Tests.Services
{
    public class TaskItemServiceTests
    {
        private readonly Mock<ITaskItemRepository> _taskItemRepositoryMock;
        private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;

        private readonly TaskItemService _taskItemService;

        public TaskItemServiceTests()
        {
            _taskItemRepositoryMock = new Mock<ITaskItemRepository>();
            _taskListRepositoryMock = new Mock<ITaskListRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            _taskItemService = new TaskItemService(
                _taskItemRepositoryMock.Object,
                _taskListRepositoryMock.Object,
                _categoryRepositoryMock.Object);
        }

        private static TaskItem CreateTaskItem(
            Guid? userId = null,
            Guid? taskListId = null,
            Guid? categoryId = null,
            string name = "Task",
            bool isCompleted = false,
            Priority priority = Priority.Low,
            DateTime? dueDate = null,
            string? description = null)
        {
            var list = CreateTaskList(userId, taskListId);

            return new TaskItem
            {
                Id = Guid.NewGuid(),
                TaskListId = list.Id,
                TaskList = list,
                CategoryId = categoryId,
                Name = name,
                IsCompleted = isCompleted,
                Priority = priority,
                DueDate = dueDate,
                Description = description
            };
        }

        private static TaskList CreateTaskList(
            Guid? userId = null,
            Guid? taskListId = null,
            string name = "My Task List")
        {
            return new TaskList
            {
                Id = taskListId ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Name = name
            };
        }

        private static Category CreateCategory(
            Guid? userId = null,
            string name = "Work")
        {
            return new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Name = name
            };
        }

        private static CreateOrUpdateTaskDto CreateTaskDto(
            string name = "Buy milk",
            Guid? categoryId = null,
            bool isCompleted = false,
            Priority priority = Priority.Low,
            DateTime? dueDate = null,
            string? description = "Description")
        {
            return new CreateOrUpdateTaskDto
            {
                Name = name,
                CategoryId = categoryId,
                IsCompleted = isCompleted,
                Priority = priority,
                DueDate = dueDate,
                Description = description
            };
        }

        private static TaskQuery CreateTaskQuery(int page = 1, int pageSize = 15)
        {
            return new TaskQuery
            {
                Page = page,
                PageSize = pageSize
            };
        }

        private static PagedResult<TaskItem> CreatePagedResult(params TaskItem[] tasks)
        {
            return new PagedResult<TaskItem>
            {
                Items = tasks.ToList(),
                Page = 1,
                PageSize = 15,
                TotalCount = tasks.Length
            };
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTask_WhenTaskExistsAndBelongsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act
            var result = await _taskItemService.GetByIdAsync(userId, task.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(task, result);

            _taskItemRepositoryMock.Verify(
                r => r.GetByIdAsync(task.Id),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _taskItemService.GetByIdAsync(Guid.NewGuid(), taskId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var task = CreateTaskItem(ownerId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.GetByIdAsync(currentUserId, task.Id));
        }

        [Fact]
        public async Task GetAllAsync_ShouldThrowInvalidOperationException_WhenTaskListDoesNotExist()
        {
            // Arrange
            var taskListId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskListId))
                .ReturnsAsync((TaskList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.GetAllAsync(
                    Guid.NewGuid(),
                    taskListId,
                    new TaskQuery()));

            _taskItemRepositoryMock.Verify(
                r => r.GetAllByTaskListIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<TaskQuery>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_ShouldThrowUnauthorizedAccessException_WhenTaskListBelongsToAnotherUser()
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
                _taskItemService.GetAllAsync(
                    currentUserId,
                    taskList.Id,
                    new TaskQuery()));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnPagedResult()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            var pagedResult = CreatePagedResult(
                CreateTaskItem(userId), 
                CreateTaskItem(userId));

            var query = new TaskQuery();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            _taskItemRepositoryMock
                .Setup(r => r.GetAllByTaskListIdAsync(taskList.Id, query))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _taskItemService.GetAllAsync(
                userId,
                taskList.Id,
                query);

            // Assert
            Assert.Equal(pagedResult, result);

            _taskItemRepositoryMock.Verify(
                r => r.GetAllByTaskListIdAsync(taskList.Id, query),
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
            var dto = CreateTaskDto();
            dto.Name = name;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskItemService.CreateAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    dto));

            _taskListRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            var dto = CreateTaskDto(name: "   Buy milk   ");

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act
            var result = await _taskItemService.CreateAsync(
                userId,
                taskList.Id,
                dto);

            // Assert
            Assert.Equal("Buy milk", result.Name);

            _taskItemRepositoryMock.Verify(
                r => r.AddAsync(It.Is<TaskItem>(t =>
                    t.Name == "Buy milk")),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenPriorityIsInvalid()
        {
            // Arrange
            var dto = CreateTaskDto();
            dto.Priority = (Priority)100;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskItemService.CreateAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    dto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenTaskListDoesNotExist()
        {
            // Arrange
            var taskListId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskListId))
                .ReturnsAsync((TaskList?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.CreateAsync(
                    Guid.NewGuid(),
                    taskListId,
                    CreateTaskDto()));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenTaskListBelongsToAnotherUser()
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
                _taskItemService.CreateAsync(
                    currentUserId,
                    taskList.Id,
                    CreateTaskDto()));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            var dto = CreateTaskDto();
            dto.CategoryId = Guid.NewGuid();

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.CategoryId.Value))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.CreateAsync(
                    userId,
                    taskList.Id,
                    dto));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowUnauthorizedAccessException_WhenCategoryBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            var dto = CreateTaskDto();
            dto.CategoryId = Guid.NewGuid();

            var category = CreateCategory(Guid.NewGuid());

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.CategoryId.Value))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.CreateAsync(
                    userId,
                    taskList.Id,
                    dto));
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTask()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var taskList = CreateTaskList(userId);

            var dto = CreateTaskDto();
            dto.Name = "   Buy milk   ";

            _taskListRepositoryMock
                .Setup(r => r.GetByIdAsync(taskList.Id))
                .ReturnsAsync(taskList);

            // Act
            var result = await _taskItemService.CreateAsync(
                userId,
                taskList.Id,
                dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Buy milk", result.Name);

            _taskItemRepositoryMock.Verify(
                r => r.AddAsync(It.Is<TaskItem>(t =>
                    t.Name == "Buy milk" &&
                    t.TaskListId == taskList.Id)),
                Times.Once);

            _taskItemRepositoryMock.Verify(
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
            var dto = CreateTaskDto(name: name);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskItemService.UpdateAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    dto));

            _taskItemRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            var dto = CreateTaskDto(name: "   New task name   ");

            // Act
            await _taskItemService.UpdateAsync(
                userId,
                task.Id,
                dto);

            // Assert
            Assert.Equal("New task name", task.Name);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenPriorityIsInvalid()
        {
            // Arrange
            var dto = CreateTaskDto();
            dto.Priority = (Priority)100;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskItemService.UpdateAsync(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    dto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.UpdateAsync(
                    Guid.NewGuid(),
                    taskId,
                    CreateTaskDto()));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var task = CreateTaskItem(ownerId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.UpdateAsync(
                    currentUserId,
                    task.Id,
                    CreateTaskDto()));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            var dto = CreateTaskDto(categoryId: Guid.NewGuid());

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.CategoryId!.Value))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.UpdateAsync(userId, task.Id, dto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenCategoryBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            var category = CreateCategory();

            var dto = CreateTaskDto(categoryId: category.Id);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.UpdateAsync(userId, task.Id, dto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTask()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var category = CreateCategory(userId);

            var task = CreateTaskItem(userId);

            var dto = CreateTaskDto(
                name: "   New Task   ",
                categoryId: category.Id,
                isCompleted: true,
                priority: Priority.High,
                dueDate: DateTime.Today,
                description: "Updated");

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            _categoryRepositoryMock
                .Setup(r => r.GetByIdAsync(category.Id))
                .ReturnsAsync(category);

            // Act
            await _taskItemService.UpdateAsync(userId, task.Id, dto);

            // Assert
            Assert.Equal("New Task", task.Name);
            Assert.Equal(category.Id, task.CategoryId);
            Assert.True(task.IsCompleted);
            Assert.Equal(Priority.High, task.Priority);
            Assert.Equal(DateTime.Today, task.DueDate);
            Assert.Equal("Updated", task.Description);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.DeleteAsync(Guid.NewGuid(), taskId));

            _taskItemRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<TaskItem>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var task = CreateTaskItem(ownerId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.DeleteAsync(currentUserId, task.Id));
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteTask()
        {
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await _taskItemService.DeleteAsync(userId, task.Id);

            _taskItemRepositoryMock.Verify(
                r => r.DeleteAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CompleteAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.CompleteAsync(Guid.NewGuid(), taskId));
        }

        [Fact]
        public async Task CompleteAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            var task = CreateTaskItem(Guid.NewGuid());

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.CompleteAsync(Guid.NewGuid(), task.Id));
        }

        [Fact]
        public async Task CompleteAsync_ShouldMarkTaskAsCompleted()
        {
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await _taskItemService.CompleteAsync(userId, task.Id);

            Assert.True(task.IsCompleted);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UncompleteAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.UncompleteAsync(Guid.NewGuid(), taskId));
        }

        [Fact]
        public async Task UncompleteAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            var task = CreateTaskItem(Guid.NewGuid());

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.UncompleteAsync(Guid.NewGuid(), task.Id));
        }

        [Fact]
        public async Task UncompleteAsync_ShouldMarkTaskAsNotCompleted()
        {
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId, isCompleted: true);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await _taskItemService.UncompleteAsync(userId, task.Id);

            Assert.False(task.IsCompleted);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task RenameAsync_ShouldThrowArgumentException_WhenNameIsInvalid(string name)
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskItemService.RenameAsync(Guid.NewGuid(), taskId, name));

            _taskItemRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task RenameAsync_ShouldTrimName_WhenNameContainsSpaces()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act
            await _taskItemService.RenameAsync(
                userId,
                task.Id,
                "   Renamed task   ");

            // Assert
            Assert.Equal("Renamed task", task.Name);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task RenameAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.RenameAsync(Guid.NewGuid(), taskId, "Task"));
        }

        [Fact]
        public async Task RenameAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var task = CreateTaskItem(ownerId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.RenameAsync(currentUserId, task.Id, "New Name"));
        }

        [Fact]
        public async Task RenameAsync_ShouldTrimName()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act
            await _taskItemService.RenameAsync(userId, task.Id, "   New Task   ");

            // Assert
            Assert.Equal("New Task", task.Name);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ChangePriorityAsync_ShouldThrowArgumentException_WhenPriorityIsInvalid()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _taskItemService.ChangePriorityAsync(
                    Guid.NewGuid(),
                    taskId,
                    (Priority)100));
        }

        [Fact]
        public async Task ChangePriorityAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.ChangePriorityAsync(
                    Guid.NewGuid(),
                    taskId,
                    Priority.High));
        }

        [Fact]
        public async Task ChangePriorityAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            var task = CreateTaskItem(Guid.NewGuid());

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.ChangePriorityAsync(
                    Guid.NewGuid(),
                    task.Id,
                    Priority.High));
        }

        [Fact]
        public async Task ChangePriorityAsync_ShouldUpdatePriority()
        {
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await _taskItemService.ChangePriorityAsync(
                userId,
                task.Id,
                Priority.High);

            Assert.Equal(Priority.High, task.Priority);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ChangeDueDateAsync_ShouldThrowInvalidOperationException_WhenTaskDoesNotExist()
        {
            var taskId = Guid.NewGuid();

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((TaskItem?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _taskItemService.ChangeDueDateAsync(
                    Guid.NewGuid(),
                    taskId,
                    DateTime.Today));
        }

        [Fact]
        public async Task ChangeDueDateAsync_ShouldThrowUnauthorizedAccessException_WhenTaskBelongsToAnotherUser()
        {
            var task = CreateTaskItem(Guid.NewGuid());

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _taskItemService.ChangeDueDateAsync(
                    Guid.NewGuid(),
                    task.Id,
                    DateTime.Today));
        }

        [Fact]
        public async Task ChangeDueDateAsync_ShouldUpdateDueDate()
        {
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(userId);

            var dueDate = new DateTime(2026, 7, 1);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            await _taskItemService.ChangeDueDateAsync(
                userId,
                task.Id,
                dueDate);

            Assert.Equal(dueDate, task.DueDate);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task ChangeDueDateAsync_ShouldClearDueDate()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var task = CreateTaskItem(
                userId,
                dueDate: DateTime.Today);

            _taskItemRepositoryMock
                .Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);

            // Act
            await _taskItemService.ChangeDueDateAsync(
                userId,
                task.Id,
                null);

            // Assert
            Assert.Null(task.DueDate);

            _taskItemRepositoryMock.Verify(
                r => r.UpdateAsync(task),
                Times.Once);

            _taskItemRepositoryMock.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }
    }
}
