using DTOs;

namespace Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto> CreateReportAsync(CreateReportDto dto);
        Task<IEnumerable<OutReportDto>> GetAllReportsAsync();
        Task<OutReportDto?> GetReportByIdAsync(int id);
    }
}