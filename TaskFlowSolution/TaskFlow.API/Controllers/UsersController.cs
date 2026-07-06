using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Abstractions;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services.Interfaces;

namespace TaskFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : MyControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(UserAuthDto request)
        {
            var token = await _userService.RegisterAsync(
                request.Username,
                request.Password);

            var response = new AuthResponseDto
            {
                Token = token
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserAuthDto request)
        {
            var token = await _userService.LoginAsync(
                request.Username,
                request.Password);

            if (token == null)
                return Unauthorized("Invalid username or password.");

            var response = new AuthResponseDto
            {
                Token = token
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userService.GetByIdAsync(GetUserId());

            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                Username = user.Username
            });
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(UserDto userDto)
        {
            await _userService.UpdateAsync(GetUserId(), userDto);

            return NoContent();
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await _userService.DeleteAsync(GetUserId());

            return NoContent();
        }
    }
}
