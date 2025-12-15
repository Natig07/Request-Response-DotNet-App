namespace Models
{
    public class RequestHistory
    {
        public int Id { get; set; }

        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string Action { get; set; } = string.Empty; // e.g., "Status changed", "Comment added", "Priority changed"
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}