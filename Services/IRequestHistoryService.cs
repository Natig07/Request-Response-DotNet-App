using DTOs;

namespace Services
{
    public interface IRequestHistoryService
    {
        Task AddHistoryAsync(int requestId, int userId, string action, string description);
        Task<IEnumerable<OutRequestHistoryDto>> GetRequestHistoryAsync(int requestId);
    }
}