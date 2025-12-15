using Exceptions;
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
        private readonly ILogger<FileController> _logger;


        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> PostFile(IFormFile file)
        {

            var uploadedFile = await _fileService.UploadAsync(file);
            return Ok(uploadedFile);

        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file == null)
                return NotFound();

            var path = file.FilePath;
            Console.WriteLine($"{path}");

            if (!System.IO.File.Exists(path))
                return NotFound("File not found.");

            var contentType = file.ContentType ?? "application/octet-stream";
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return File(fileStream, contentType);
        }

        [Authorize]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file == null) return NotFound();

            var contentType = GetContentType(file.FileName);


            var fileStream = await _fileService.DownloadAsync(id);
            return File(fileStream!, contentType, file?.FileName);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var deleted = await _fileService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        private string GetContentType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octat-stream";
            }

            return contentType;
        }
    }
}