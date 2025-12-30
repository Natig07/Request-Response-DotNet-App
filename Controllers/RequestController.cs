using DTOs;
using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(IRequestService requestService, ILogger<RequestsController> logger)
        {
            _requestService = requestService;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            return Ok(await _requestService.GetAllRequestsAsync());
        }
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _requestService.GetByIdAsync(id);

            if (res == null) return NotFound();

            return Ok(res);
        }
        [Authorize]
        [HttpGet("category/{categoryId}")]

        public async Task<IActionResult> GetByCat(int categoryId)
        {
            var res = await _requestService.GetByCategoryAsync(categoryId);

            if (res == null || !res.Any()) return NotFound();
            return Ok(res);
        }
        [Authorize]
        [HttpGet("reqres/{id}")]

        public async Task<IActionResult> GetReqRes(int id)
        {
            var res = await _requestService.GetReqResByIdAsync(id);

            if (res == null) return NotFound();
            return Ok(res);
        }
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateReq([FromForm] CreateRequestDto dto)
        {
            var created = await _requestService.CreateReqAsync(dto);

            if (created.File != null)
            {
                created.File.Url = $"{Request.Scheme}://{Request.Host}/api/files/{created.File.Id}";
            }

            return Ok(created);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task UpdateReq(int id, [FromForm] CreateRequestDto dto)
        {
            await _requestService.UpdateAsync(id, dto);
        }

        [Authorize]
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteReq(int id)
        {
            var deleted = await _requestService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
        [Authorize]
        [HttpPut("{requestId}/status/{newStatusId}/{UserId}")]
        public async Task ChangeReqStat(int requestId, int newStatusId, int UserId)
        {
            await _requestService.ChangeReqStat(requestId, newStatusId, UserId);
        }
        [Authorize]
        [HttpPut("take/{executorId}/{reqId}")]
        public async Task<IActionResult> TakeRequest(int executorId, int reqId)
        {
            await _requestService.TakeRequestAsync(reqId, executorId);

            return Ok(new { message = "Request successfully taken" });
        }
        [Authorize]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterRequests(
            [FromQuery] int? categoryId,
            [FromQuery] int? statusId,
            [FromQuery] int? priorityId,
            [FromQuery] int? executorId,
            [FromQuery] string? search,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortDirection = null
        )
        {
            try
            {
                var filter = new RequestFilterDto
                {
                    CategoryId = categoryId,
                    StatusId = statusId,
                    PriorityId = priorityId,
                    ExecutorId = executorId,
                    Search = search,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Page = page,
                    PageSize = pageSize,
                    SortField = sortField,
                    SortDirection = sortDirection
                };

                var result = await _requestService.GetFilteredAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered requests");
                return StatusCode(500, new { message = "Error fetching filtered requests", error = ex.Message });
            }
        }

        // Add to your RequestsController

        [HttpGet("{id}/section/{section}")]
        public async Task<ActionResult<RequestDto>> GetRequestBySection(int id, string section)
        {
            try
            {
                var request = await _requestService.GetRequestWithDetails(id, section);
                return Ok(request);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching request {RequestId} with section {Section}", id, section);
                return StatusCode(500, new { message = "Error fetching request" });
            }
        }

        [HttpGet("{id}/comment-count")]
        public async Task<ActionResult<int>> GetCommentCount(int id)
        {
            try
            {
                var count = await _requestService.GetCommentCountAsync(id);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comment count for request {RequestId}", id);
                return StatusCode(500, new { message = "Error fetching comment count" });
            }
        }




    }

}