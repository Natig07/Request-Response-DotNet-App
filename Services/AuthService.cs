using Data;
using DTOs;
using Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IFileService _fileService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, IConfiguration config, IEmailService emailService, IFileService fileService, ILogger<AuthService> logger)
    {
        _context = context;
        _config = config;
        _fileService = fileService;
        _emailService = emailService;
        _logger = logger;
    }


    public async Task<string?> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Register attempt for email: {Email}", dto.Email);

        var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
        {
            _logger.LogWarning("Registration failed. Email {Email} already in use.", dto.Email);
            throw new ConflictException($"Email '{dto.Email}' is already in use.");
        }


        int? fileId = null;
        if (dto.ProfilePhoto != null)
        {
            var ext = Path.GetExtension(dto.ProfilePhoto.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                throw new BadRequestException("Only JPG, JPEG, and PNG files are allowed for profile photos.");
            }

            _logger.LogInformation("Uploading profile photo for user {Email}", dto.Email);
            fileId = await _fileService.UploadAsync(dto.ProfilePhoto);
        }

        var user = new User
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Username = dto.Username,
            Position = dto.Position,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            isDeleted = false,
            UserRoles = new List<UserRole>(),
            ProfilePhotoId = fileId,
            Department = dto.Department,
            MobTelNumber = dto.MobTelNumber,
            OfficeTelNumber = dto.OfficeTelNumber,
            AllowNotification = dto.AllowNotification
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = dto.RoleId
        });

        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(user.Email!,
            "Welcome to ReqRes App",
            $"<h3>Hello {user.Name},</h3><p>Your account has been created successfully.</p>"
        );



        _logger.LogInformation("User {UserId} registered successfully with email {Email}", user.Id, user.Email);
        return null;
    }


    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Login attempt for username: {Username}", dto.Username);

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == dto.Username && !u.isDeleted);

        if (user == null)
        {
            _logger.LogWarning("Login failed. Username {Username} not found.", dto.Username);
            throw new NotFoundException($"User with username '{dto.Username}' not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed. Invalid password for username {Username}", dto.Username);
            throw new UnauthorizedException("Invalid username or password!");
        }

        var accessToken = GenerateAccessToken(user);

        var refreshToken = GenerateRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return new AuthResponseDto
        {
            UserID = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            Expires = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpireMinutes")),
            FirstName = user.Name!,
            LastName = user.Surname!,
            Department = user.Department,
            MobTelNumber = user.MobTelNumber,
            OfficeTelNumber = user.OfficeTelNumber,
            AllowNotification = user.AllowNotification

        };
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Refresh token attempt for token: {Token}", refreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || storedToken.IsExpired || storedToken.Revoked != null)
        {
            _logger.LogWarning("Refresh token failed. Token is invalid or expired.");

            throw new UnauthorizedException("Refresh token is invalid or expired.");
        }

        storedToken.Revoked = DateTime.UtcNow;

        var user = storedToken.User!;
        var accessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken(user.Id);

        storedToken.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Update(storedToken);
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token rotated successfully for user {UserId}", user.Id);


        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newRefreshToken.Token,
            Expires = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:RefreshTokenValidity"))
        };
    }

    private string GenerateAccessToken(User user)
    {
        _logger.LogDebug("Generating access token for user {UserId}", user.Id);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name ?? ""),
            new Claim("Username", user.Username ?? "")
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role!.Name!));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpireMinutes")),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        _logger.LogDebug("Access token generated for user {UserId}", user.Id);

        return tokenHandler.WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(int userId)
    {
        _logger.LogDebug("Generating refresh token for user {UserId}", userId);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:RefreshTokenValidity")),
            Created = DateTime.UtcNow,
            UserId = userId
        };
    }

    public async Task<bool> RenewPasswordAsync(int userId, RenewPasswordDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found.");

        // Verify old password
        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
            throw new Exception("Əvvəlki şifrə yalnışdır!");


        // Check password length
        if (dto.NewPassword.Length < 8)
            throw new Exception("Şifrə ən azı 8 simvol uzunluğunda olmalıdır!");

        // Check if it's only digits
        if (dto.NewPassword.All(char.IsDigit))
            throw new Exception("Şifrə yalnız rəqəmlərdən ibarət ola bilməz!");

        //Check if it contains any letter
        if (!dto.NewPassword.Any(char.IsLetter))
            throw new Exception("Şifrədə ən azı bir hərf olmalıdır!");

        // Check if password was used before
        foreach (var oldHash in user.OldPasswordHashes)
        {
            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, oldHash))
                throw new Exception("Əvvəl istifadə edilmiş şifrəni təkrar istifadə etmək olmaz.");
        }



        // Confirm new passwords match
        if (dto.NewPassword != dto.RepeatNewPassword)
            throw new Exception("Yeni şifrələr uyğun gəlmir.");


        // Store current password into history
        user.OldPasswordHashes.Add(user.PasswordHash!);

        // Hash and save the new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }


}
