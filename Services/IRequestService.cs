using DTOs;

namespace Services
{
    public interface IRequestService
    {
        Task<IEnumerable<OutRequestDto>> GetAllRequestsAsync();
        Task<RequestDto> CreateReqAsync(CreateRequestDto dto);
        Task<OutRequestDto> GetByIdAsync(int Id);
        Task<IEnumerable<OutRequestDto>> GetByCategoryAsync(int Id);

        Task<RequestDto> GetReqResByIdAsync(int id);

        Task UpdateAsync(int id, CreateRequestDto dto);

        Task<bool> DeleteAsync(int id);
        Task ChangeReqStat(int Requestid, int UserId, int newStatusId);

        Task TakeRequestAsync(int requestId, int executorId);
    }
}