using DTOs;

namespace Services
{
    public interface IReqCategoryService
    {
        Task<IEnumerable<ReqCategoryDto>> GetAllCategoriesAsync();
        Task<ReqCategoryDto> CreateCategoryAync(CreateCategoryDto dto);

        Task UpdateCategoryAsync(int id, CreateCategoryDto dto);

        Task<bool> DeleteCategoryAsync(int id);
    }
}