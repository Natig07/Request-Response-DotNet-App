
using DTOs;

namespace Services
{
    public interface IResponseService
    {
        Task<IEnumerable<OutResponseDto>> GetAllResponsesAsync();

        Task<ResponseDto> CreateResAync(CreateResponseDto dto);

        Task<OutResponseDto> GetByIdAsync(int id);

        Task UpdateAsync(int id, CreateResponseDto dto);

        Task<bool> DeleteAsync(int id);
        Task ChangeResStat(int id, int newStatusId);
    }
}
