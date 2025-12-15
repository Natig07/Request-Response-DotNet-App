namespace DTOs
{
    public class RequestHistoryDto
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public UserDto? User { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OutRequestHistoryDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? UserSurname { get; set; }
        public string? UserPosition { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}