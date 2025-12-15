using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }



        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var res = await _commentService.GetCommentsByRequestIdAsync(id);

            if (res == null) return NotFound();

            return Ok(res);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentDto dto)
        {
            var created = await _commentService.AddCommentAsync(dto);
            return Ok(created);
        }


    }
}