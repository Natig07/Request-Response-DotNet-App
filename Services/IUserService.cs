using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Services
{
    public interface IUserService
    {
        Task<IEnumerable<OutUserDto>> GetAllUsersAsync();
        Task<OutUserDto> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAync(CreateUserDto dto);

        Task<UpdateUserDto> UpdateUserAsync(int id, UpdateUserDto dto);

        Task<bool> DeleteUserAsync(int id);
    }
}