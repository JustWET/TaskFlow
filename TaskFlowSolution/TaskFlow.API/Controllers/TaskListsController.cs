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
    public class TaskListsController : MyControllerBase
    {
        private readonly ITaskListService _taskListService;

        public TaskListsController(ITaskListService taskListService)
        {
            _taskListService = taskListService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskListDto>>> GetAll()
        {
            var taskLists = await _taskListService.GetAllAsync(GetUserId());

            var response = taskLists.Select(t => new TaskListDto()
            {
                Name = t.Name,
                Id = t.Id,
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{taskListId:guid}")]
        public async Task<ActionResult<TaskListDto>> GetById(Guid taskListId)
        {
            var taskList = await _taskListService
                .GetByIdAsync(GetUserId(), taskListId);

            if (taskList == null)
                return NotFound();

            var response = new TaskListDto
            {
                Name = taskList.Name,
                Id = taskList.Id
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<TaskListDto>> Create(string name)
        {
            var taskList = await _taskListService.CreateAsync(GetUserId(), name);

            var response = new TaskListDto
            {
                Name = name,
                Id = taskList.Id
            };

            return CreatedAtAction(nameof(GetById), new { taskListId = taskList.Id }, response);
        }

        [HttpPut("{taskListId:guid}")]
        public async Task<IActionResult> Update(Guid taskListId, string name)
        {
            await _taskListService.UpdateAsync(GetUserId(), taskListId, name);

            return NoContent();
        }

        [HttpDelete("{taskListId:guid}")]
        public async Task<IActionResult> Delete(Guid taskListId)
        {
            await _taskListService.DeleteAsync(GetUserId(), taskListId);

            return NoContent();
        }
    }
}
