using TaskFlow.API.Services.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories.Interfaces;

namespace TaskFlow.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITaskItemRepository _taskItemRepository;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ITaskItemRepository taskItemRepository)
        {
            _categoryRepository = categoryRepository;
            _taskItemRepository = taskItemRepository;
        }

        public async Task<Category?> GetByIdAsync(Guid currentUserId, Guid categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null)
                return null;

            if (category.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            return category;
        }

        public async Task<List<Category>> GetAllAsync(Guid currentUserId)
        {
            return await _categoryRepository.GetAllByUserIdAsync(currentUserId);
        }

        public async Task<Category> CreateAsync(Guid currentUserId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty.", nameof(name));

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = currentUserId
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return category;
        }

        public async Task UpdateAsync(Guid currentUserId, Guid categoryId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty.", nameof(name));

            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null)
                throw new InvalidOperationException("Category not found.");

            if (category.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            category.Name = name;

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid currentUserId, Guid categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null)
                throw new InvalidOperationException("Category not found.");

            if (category.UserId != currentUserId)
                throw new UnauthorizedAccessException();

            await _taskItemRepository.ClearCategoryAsync(categoryId);
            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }
    }
}
