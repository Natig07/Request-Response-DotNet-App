namespace Models;

public class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Username { get; set; }
    public string? Position { get; set; }
    public bool isDeleted { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public ICollection<Response>? Responses { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();


    // public ICollection<Request>? Requests { get; set; }
}
