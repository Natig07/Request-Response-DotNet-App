using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _fileService.GetFileAsync(id);

            if (res == null) return NotFound();


            return Ok(res);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var fileStream = await _fileService.DownloadAsync(id);
            if (fileStream == null) return NotFound();

            var file = await _fileService.GetFileAsync(id);
            return File(fileStream, "application/octet-stream", file?.FileName);
        }
    }
}