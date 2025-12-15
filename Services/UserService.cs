using Data;
using AutoMapper;
using DTOs;
using Services;
using Models;
using Microsoft.EntityFrameworkCore;
using Exceptions;
using Microsoft.AspNetCore.Mvc;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly FileDbContext _fileDbcontext;
    private readonly IFileService _fileService;

    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, FileDbContext fileDbContext, IFileService fileService, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _fileService = fileService;
        _fileDbcontext = fileDbContext;
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAync(CreateUserDto dto)
    {
        _logger.LogInformation("Creating new user {UserName} {UserSurname} with RoleId={RoleId}", dto.Name, dto.Surname, dto.RoleId);

        var user = _mapper.Map<User>(dto);
        user.isDeleted = false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} saved to database", user.Id);

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = dto.RoleId
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation("Role {RoleId} assigned to User {UserId}", dto.RoleId, user.Id);

            var savedUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (savedUser == null)
            {
                _logger.LogError("User {UserId} could not be retrieved after creation", user.Id);
                throw new InternalServerException("User could not be retrieved after creation.");
            }
            var dtoResult = _mapper.Map<UserDto>(savedUser);
            if (user.ProfilePhotoId.HasValue)
            {
                var file = await _fileService.GetFileAsync(user.ProfilePhotoId.Value);
                dtoResult.ProfilePhoto = file == null ? null : new FileEntityDto { Id = file.Id, FileName = file.FileName };
            }

            _logger.LogInformation("User {UserId} created successfully", user.Id);
            return _mapper.Map<UserDto>(savedUser);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while creating user {UserName}", dto.Name);
            throw new InternalServerException("Could not create user due to database error.");
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);

        var user = await _context.Users.FindAsync(id);
        if (user == null || user.isDeleted)
        {
            _logger.LogWarning("User {UserId} not found or already deleted", id);
            throw new NotFoundException($"User with ID {id} not found.");
        }

        try
        {
            user.isDeleted = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} deleted successfully", id);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while deleting user {UserId}", id);
            throw new InternalServerException("Could not delete user due to database error.");
        }
    }

    public async Task<IEnumerable<OutUserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Fetching all active users");

        try
        {
            var users = await _context.Users
                .Where(u => !u.isDeleted)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new OutUserDto
                {
                    Name = u.Name,
                    Username = u.Username,
                    Surname = u.Surname,
                    Position = u.Position,
                    ProfilePhotoId = u.ProfilePhotoId,
                    Email = u.Email,
                    Department = u.Department,
                    MobTelNumber = u.MobTelNumber,
                    OfficeTelNumber = u.OfficeTelNumber,
                    AllowNotification = u.AllowNotification

                })
                .ToListAsync();


            _logger.LogInformation("Fetched {Count} users", users.Count);
            return _mapper.Map<List<OutUserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching users");
            throw new InternalServerException("Could not fetch users.");
        }
    }

    public async Task<OutUserDto> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Fetching User {UserId}", id);

        var user = await _context.Users
            .Where(u => u.Id == id && !u.isDeleted)   // <--- filter by ID
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Select(u => new OutUserDto
            {
                Name = u.Name,
                Username = u.Username,
                Surname = u.Surname,
                Position = u.Position,
                ProfilePhotoId = u.ProfilePhotoId,
                Email = u.Email,
                Department = u.Department,
                MobTelNumber = u.MobTelNumber,
                OfficeTelNumber = u.OfficeTelNumber,
                AllowNotification = u.AllowNotification
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException($"User with ID {id} not found.");
        }

        _logger.LogInformation("User {UserId} retrieved successfully", id);
        return user;   // no need to map again, already an OutUserDto
    }


    public async Task<UpdateUserDto> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        _logger.LogInformation("Updating user {UserId}", id);

        var existingUser = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (existingUser == null || existingUser.isDeleted)
        {
            _logger.LogWarning("User {UserId} not found or deleted", id);
            throw new NotFoundException($"User with ID {id} not found.");
        }

        try
        {
            bool isModified = false;

            // Update only if values are provided
            if (!string.IsNullOrWhiteSpace(dto.Name) && existingUser.Name != dto.Name)
            {
                existingUser.Name = dto.Name;
                isModified = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Surname) && existingUser.Surname != dto.Surname)
            {
                existingUser.Surname = dto.Surname;
                isModified = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username) && existingUser.Username != dto.Username)
            {
                existingUser.Username = dto.Username;
                isModified = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Position) && existingUser.Position != dto.Position)
            {
                existingUser.Position = dto.Position;
                isModified = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.Department) && existingUser.Department != dto.Department)
            {
                existingUser.Department = dto.Department;
                isModified = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.MobTelNumber) && existingUser.MobTelNumber != dto.MobTelNumber)
            {
                existingUser.MobTelNumber = dto.MobTelNumber;
                isModified = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.OfficeTelNumber) && existingUser.OfficeTelNumber != dto.OfficeTelNumber)
            {
                existingUser.OfficeTelNumber = dto.OfficeTelNumber;
                isModified = true;
            }

            if (dto.AllowNotification.HasValue)
            {
                existingUser.AllowNotification = dto.AllowNotification.Value;
                isModified = true;
            }



            if (!string.IsNullOrWhiteSpace(dto.Email) && existingUser.Email != dto.Email)
            {
                existingUser.Email = dto.Email;
                isModified = true;
            }

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                isModified = true;
            }

            // Update role only if provided
            if (dto.RoleId > 0 &&
                !(existingUser.UserRoles?.Any(ur => ur.RoleId == dto.RoleId) ?? false))
            {
                existingUser.UserRoles?.Clear();
                existingUser.UserRoles = new List<UserRole>
            {
                new UserRole { UserId = existingUser.Id, RoleId = dto.RoleId}
            };
                isModified = true;
            }

            // Update profile photo only if new one is uploaded
            if (dto.ProfilePhoto != null)
            {
                _logger.LogInformation("Updating profile photo for User {UserId}", id);
                if (existingUser.ProfilePhotoId.HasValue)
                {
                    await _fileService.DeleteAsync(existingUser.ProfilePhotoId.Value);
                }

                existingUser.ProfilePhotoId = await _fileService.UploadAsync(dto.ProfilePhoto);
                isModified = true;
            }

            if (isModified)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} updated successfully", id);
            }
            else
            {
                _logger.LogInformation("No changes detected for User {UserId}", id);
            }
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating user {UserId}", id);
            throw new InternalServerException("Could not update user due to database error.");
        }
        return _mapper.Map<UpdateUserDto>(existingUser);
    }

}
