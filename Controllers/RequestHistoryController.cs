using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class RequestsHistoryController : ControllerBase
    {

        private readonly IRequestHistoryService _requestHistoryService;
        private readonly ILogger<RequestsHistoryController> _logger;

        public RequestsHistoryController(IRequestHistoryService requestHistoryService, ILogger<RequestsHistoryController> logger)
        {
            _requestHistoryService = requestHistoryService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetRequestHistory(int id)
        {
            var history = await _requestHistoryService.GetRequestHistoryAsync(id);
            return Ok(history);
        }



    }

}