using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CategoryController : ControllerBase
    {
        private readonly IReqCategoryService _service;

        public CategoryController(IReqCategoryService service)
        {
            _service = service;
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllCategoriesAsync());
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost]

        public async Task<IActionResult> CreateReqCategory(CreateCategoryDto dto)
        {
            var created = await _service.CreateCategoryAync(dto);

            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);

        }
        [Authorize(Policy = "RequireAdminRole")]

        [HttpPut("{id}")]
        public async Task UpdateReqCategory(int id, CreateCategoryDto dto)
        {
            await _service.UpdateCategoryAsync(id, dto);
        }
        [Authorize(Policy = "RequireAdminRole")]

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteReqCategory(int id)
        {
            var deleted = await _service.DeleteCategoryAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}