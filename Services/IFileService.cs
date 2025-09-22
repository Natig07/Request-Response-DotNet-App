using DTOs;

namespace Services
{
    public interface IFileService
    {
        Task<int> UploadAsync(IFormFile file);
        Task<FileEntityDto?> GetFileAsync(int id);
        Task<Stream?> DownloadAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
