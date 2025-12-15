namespace Models
{

    public class Comment
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public int UserId { get; set; }

        public int? AttachmentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RequestId { get; set; }
        public Request? Request { get; set; }
    }
}
