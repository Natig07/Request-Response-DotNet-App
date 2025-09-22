using Data;
using DTOs;
using Models;
using Exceptions;
using Services;

public class FileService : IFileService
{
    private readonly FileDbContext _context;
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    private readonly ILogger<FileService> _logger;

    public FileService(FileDbContext context, ILogger<FileService> logger)
    {
        _context = context;
        _logger = logger;

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created uploads directory at {Path}", _storagePath);
        }
    }

    public async Task<int> UploadAsync(IFormFile file)
    {
        try
        {
            _logger.LogInformation("File upload attempt: {FileName}, ContentType: {ContentType}, Size: {Size} bytes",
                file.FileName, file.ContentType, file.Length);

            var allowedContentTypes = new[]
            {
                "application/pdf",
                "image/jpeg",
                "image/png"
            };

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("Upload failed. Invalid content type: {ContentType}", file.ContentType);
                throw new ValidationExceptionC("Only PDF or image files are allowed.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Upload failed. Invalid file extension: {Extension}", extension);
                throw new ValidationExceptionC("Invalid file extension. Only PDF or image files are allowed.");
            }

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(_storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var entity = new FileEntity
            {
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                Size = file.Length

            };

            entity.isDeleted = false;
            _context.Files.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File uploaded successfully: {FileName} (Id: {FileId})", entity.FileName, entity.Id);

            return entity.Id;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "I/O error occurred during file upload: {FileName}", file.FileName);
            throw new ServiceUnavailableException("File storage is unavailable at the moment.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload: {FileName}", file.FileName);
            throw new InternalServerException("Unexpected error occurred while uploading the file.");
        }
    }

    public async Task<FileEntityDto?> GetFileAsync(int id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null || file.isDeleted == true)
        {
            _logger.LogWarning("File not found (Id: {FileId})", id);
            throw new NotFoundException($"File with ID {id} was not found.");
        }

        _logger.LogInformation("Retrieved file metadata for Id: {FileId}", id);

        return new FileEntityDto
        {
            Id = file.Id,
            FileName = file.FileName,
            Url = $"/api/files/{file.Id}"
        };
    }

    public async Task<Stream?> DownloadAsync(int id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null || file.isDeleted == true)
        {
            _logger.LogWarning("Download failed. File not found (Id: {FileId})", id);
            throw new NotFoundException($"File with ID {id} was not found.");
        }

        try
        {
            _logger.LogInformation("File download started: {FileName} (Id: {FileId})", file.FileName, id);
            return new FileStream(file.FilePath, FileMode.Open, FileAccess.Read);
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "I/O error while accessing file {FilePath}", file.FilePath);
            throw new ServiceUnavailableException("File could not be accessed at the moment.");
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null)
        {
            _logger.LogWarning("Delete failed. File not found (Id: {FileId})", id);
            throw new NotFoundException($"File with ID {id} was not found.");
        }

        try
        {
            if (File.Exists(file.FilePath))
            {
                File.Delete(file.FilePath);
                _logger.LogInformation("Deleted file from storage: {FilePath}", file.FilePath);
            }

            file.isDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted file record from DB (Id: {FileId})", id);

            return true;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "I/O error while deleting file {FilePath}", file.FilePath);
            throw new ServiceUnavailableException("File storage is unavailable for deletion.");
        }
    }
}
