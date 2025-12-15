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
        private readonly IUserService _UserService;
        private readonly AppDbContext _context;

        public UserController(IUserService service, AppDbContext context)
        {
            _UserService = service;
            _context = context;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            return Ok(await _UserService.GetAllUsersAsync());
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _UserService.GetUserByIdAsync(id);

            if (res == null) return NotFound();

            return Ok(res);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost]

        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var created = await _UserService.CreateUserAync(dto);

            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto dto)
        {
            var response = await _UserService.UpdateUserAsync(id, dto);
            return Ok(response);

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _UserService.DeleteUserAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }



    }
}