namespace DTOs
{
    public class RegisterDto
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Username { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public string? MobTelNumber { get; set; }
        public string? OfficeTelNumber { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public string? Password { get; set; }
        public bool AllowNotification { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }


}
