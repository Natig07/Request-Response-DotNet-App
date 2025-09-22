using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ResponseController : ControllerBase
    {
        private readonly IResponseService _service;

        public ResponseController(IResponseService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet]

        public async Task<IActionResult> GetAllResponses()
        {
            return Ok(await _service.GetAllResponsesAsync());
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _service.GetByIdAsync(id);

            if (res == null) return NotFound();

            return Ok(res);
        }
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateResponse([FromForm] CreateResponseDto resdto)
        {
            var created = await _service.CreateResAync(resdto);
            if (created.File != null)
            {
                created.File.Url = $"{Request.Scheme}://{Request.Host}/api/files/{created.File.Id}";
            }

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task UpdateReq(int id, [FromForm] CreateResponseDto dto)
        {
            await _service.UpdateAsync(id, dto);
        }

        [Authorize]
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteRes(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpPut("responses/{responseId}/status/{newStatusId}")]
        public async Task ChangeReqStat(int responseId, int newStatusId)
        {
            await _service.ChangeResStat(responseId, newStatusId);
        }

    }
}