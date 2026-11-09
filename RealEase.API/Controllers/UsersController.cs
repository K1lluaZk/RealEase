using Microsoft.AspNetCore.Mvc;
using RealEase.Application.Dtos.User;
using RealEase.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealEase.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // El parámetro de ruta automáticamente parsea el string a Guid
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound($"Usuario con ID {id} no encontrado.");

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] UserDto dto)
        {
            var userId = await _userService.AddUserAsync(dto);
            if (userId == Guid.Empty) return BadRequest("No se pudo crear el usuario.");

            return CreatedAtAction(nameof(GetById), new { id = userId }, userId);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserDto dto)
        {
            var result = await _userService.UpdateUserAsync(dto);
            if (!result) return NotFound($"No se pudo actualizar. Usuario no encontrado.");

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound($"No se pudo eliminar. Usuario no encontrado.");

            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<UserDto>>> GetFiltered(
            [FromQuery] string? name,
            [FromQuery] string? role,
            [FromQuery] bool? isActive)
        {
            var users = await _userService.GetFilteredUsersAsync(name, role, isActive);
            return Ok(users);
        }
    }
}
