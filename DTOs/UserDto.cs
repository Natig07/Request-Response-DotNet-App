

namespace DTOs
{

    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Surname { get; set; }
        public string? Position { get; set; }
        public string? Email { get; set; }


        // public ICollection<OutResponseDto> Responses { get; set; } = new List<OutResponseDto>();s
        public ICollection<UserRoleDto>? UserRoles { get; set; }
        // public ICollection<RequestDto>? Requests { get; set; }
    }

    public class CreateUserDto
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Username { get; set; }
        public string? Position { get; set; }
        public int RoleId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }


}
