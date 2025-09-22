using DTOs;

namespace Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserAync(CreateUserDto dto);

        Task UpdateUserAsync(int id, CreateUserDto dto);

        Task<bool> DeleteUserAsync(int id);
    }
}