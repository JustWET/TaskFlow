using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Abstractions;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Models;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : MyControllerBase
    {
        private readonly ITaskItemService _taskItemService;

        public TasksController(ITaskItemService taskItemService)
        {
            _taskItemService = taskItemService;
        }

        [HttpGet("{taskId:guid}")]
        public async Task<ActionResult<ResponseTaskDto>> GetById(Guid taskId)
        {
            var task = await _taskItemService.GetByIdAsync(GetUserId(), taskId);

            if (task == null)
                return NotFound();

            return Ok(ToDto(task));
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ResponseTaskDto>>> GetAll(Guid taskListId, [FromQuery] TaskQuery taskQuery)
        {
            var result = await _taskItemService.GetAllAsync(GetUserId(), taskListId, taskQuery);

            return Ok(new PagedResult<ResponseTaskDto>
            {
                Items = result.Items.Select(ToDto).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost]
        public async Task<ActionResult<ResponseTaskDto>> Create(Guid taskListId, CreateOrUpdateTaskDto request)
        {
            var task = await _taskItemService.CreateAsync(GetUserId(), taskListId, request);

            return CreatedAtAction(nameof(GetById), new { taskId = task.Id }, ToDto(task));
        }

        [HttpPut("{taskId:guid}")]
        public async Task<IActionResult> Update(Guid taskId, CreateOrUpdateTaskDto request)
        {
            await _taskItemService.UpdateAsync(GetUserId(), taskId, request);

            return NoContent();
        }

        [HttpDelete("{taskId:guid}")]
        public async Task<IActionResult> Delete(Guid taskId)
        {
            await _taskItemService.DeleteAsync(GetUserId(), taskId);

            return NoContent();
        }

        [HttpPatch("{taskId:guid}/complete")]
        public async Task<IActionResult> Complete(Guid taskId)
        {
            await _taskItemService.CompleteAsync(GetUserId(), taskId);

            return NoContent();
        }

        [HttpPatch("{taskId:guid}/uncomplete")]
        public async Task<IActionResult> Uncomplete(Guid taskId)
        {
            await _taskItemService.UncompleteAsync(GetUserId(), taskId);

            return NoContent();
        }

        [HttpPatch("{taskId:guid}/name")]
        public async Task<IActionResult> Rename(Guid taskId, RenameTaskDto dto)
        {
            await _taskItemService.RenameAsync(GetUserId(), taskId, dto.Name);

            return NoContent();
        }

        [HttpPatch("{taskId:guid}/priority")]
        public async Task<IActionResult> ChangePriority(Guid taskId, ChangePriorityDto dto)
        {
            await _taskItemService.ChangePriorityAsync(GetUserId(), taskId, dto.Priority);

            return NoContent();
        }

        [HttpPatch("{taskId:guid}/due-date")]
        public async Task<IActionResult> ChangeDueDate(Guid taskId, ChangeDueDateDto dto)
        {
            await _taskItemService.ChangeDueDateAsync(GetUserId(), taskId, dto.DueDate);

            return NoContent();
        }

        private static ResponseTaskDto ToDto(TaskItem task)
        {
            return new ResponseTaskDto
            {
                Id = task.Id,
                TaskListId = task.TaskListId,
                CategoryId = task.CategoryId,
                Name = task.Name,
                IsCompleted = task.IsCompleted,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Description = task.Description
            };
        }
    }
}
