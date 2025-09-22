using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
using Microsoft.AspNetCore.Authorization;
using Data;
namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly AppDbContext _context;

        public UserController(IUserService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllUsersAsync());
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost]

        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var created = await _service.CreateUserAync(dto);

            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("{id}")]
        public async Task UpdateUser(int id, CreateUserDto dto)
        {
            await _service.UpdateUserAsync(id, dto);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _service.DeleteUserAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }



    }
}