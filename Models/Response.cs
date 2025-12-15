namespace Models
{
    public class Response
    {
        public int Id { get; set; }
        public string? Text { get; set; }

        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public int? FileId { get; set; }

        public int ResStatusId { get; set; }
        public ResStatus? ResStatus { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
        public bool isDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }

}