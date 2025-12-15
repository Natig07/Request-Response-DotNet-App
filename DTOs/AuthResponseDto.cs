namespace DTOs
{
    public class AuthResponseDto
    {
        public int UserID { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public string? Department { get; set; }
        public string? MobTelNumber { get; set; }
        public string? OfficeTelNumber { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool AllowNotification { get; set; }
    }
}