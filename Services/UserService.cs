using Data;
using AutoMapper;
using DTOs;
using Services;
using Models;
using Microsoft.EntityFrameworkCore;
using Exceptions;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
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

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Fetching all active users");

        try
        {
            var users = await _context.Users
                .Where(u => !u.isDeleted)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} users", users.Count);
            return _mapper.Map<List<UserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching users");
            throw new InternalServerException("Could not fetch users.");
        }
    }

    public async Task UpdateUserAsync(int id, CreateUserDto dto)
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

        bool isModified =
            existingUser.Name != dto.Name ||
            existingUser.Surname != dto.Surname ||
            !(existingUser.UserRoles?.Any(ur => ur.RoleId == dto.RoleId) ?? false);

        if (!isModified)
        {
            _logger.LogInformation("No changes detected for User {UserId}", id);
        }

        try
        {
            _logger.LogInformation("Detected changes for User {UserId}. Applying updates...", id);

            existingUser.Name = dto.Name;
            existingUser.Surname = dto.Surname;
            existingUser.Position = dto.Position;

            existingUser.UserRoles?.Clear();
            existingUser.UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = existingUser.Id,
                    RoleId = dto.RoleId
                }
            };

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} updated successfully", id);

        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating user {UserId}", id);
            throw new InternalServerException("Could not update user due to database error.");
        }
    }
}
