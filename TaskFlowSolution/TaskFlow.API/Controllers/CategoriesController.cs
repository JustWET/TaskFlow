using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Abstractions;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services.Interfaces;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : MyControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var categories = await _categoryService.GetAllAsync(GetUserId());

            var response = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{categoryId:guid}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid categoryId)
        {
            var category = await _categoryService.GetByIdAsync(GetUserId(), categoryId);

            if (category == null)
                return NotFound();

            var response = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CreateOrUpdateCategoryDto request)
        {
            var category = await _categoryService.CreateAsync(GetUserId(), request.Name);

            var response = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return CreatedAtAction(nameof(GetById), new { categoryId = response.Id }, response);
        }

        [HttpPut("{categoryId:guid}")]
        public async Task<IActionResult> Update(Guid categoryId, CreateOrUpdateCategoryDto request)
        {
            await _categoryService.UpdateAsync(GetUserId(), categoryId, request.Name);

            return NoContent();
        }

        [HttpDelete("{categoryId:guid}")]
        public async Task<IActionResult> Delete(Guid categoryId)
        {
            await _categoryService.DeleteAsync(GetUserId(), categoryId);

            return NoContent();
        }
    }
}
