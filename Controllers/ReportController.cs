using DTOs;
using Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _reportService.GetAllReportsAsync();
            return Ok(reports);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _reportService.GetReportByIdAsync(id);

            if (res == null) return NotFound();

            return Ok(res);
        }

        [Authorize]
        [HttpGet("by-request/{requestId}")]
        public async Task<ActionResult<OutReportDto>> GetRepByReqId(int requestId)
        {
            try
            {
                var report = await _reportService.GetReportByRequestId(requestId);

                if (report == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "No report found for this request"
                    });
                }

                return Ok(report);
            }
            catch (InternalServerException ex)
            {
                _logger.LogError(ex, "Internal error fetching report for request {RequestId}", requestId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error fetching report",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching report for request {RequestId}", requestId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred"
                });
            }
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto dto)
        {
            var created = await _reportService.CreateReportAsync(dto);
            return Ok(created);
        }
        [Authorize]
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilteredReports(
            [FromQuery] int? categoryId,
            [FromQuery] int? executorId,
            [FromQuery] int? statusId,
            [FromQuery] string? search,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? sortField,
            [FromQuery] string? sortDirection,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            try
            {
                var filter = new ReportFilterDto
                {
                    CategoryId = categoryId,
                    ExecutorId = executorId,
                    StatusId = statusId,
                    Search = search,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SortField = sortField,
                    SortDirection = sortDirection,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _reportService.GetFilteredReportsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered reports");
                return StatusCode(500, new { message = "Error fetching filtered reports", error = ex.Message });
            }
        }



    }
}