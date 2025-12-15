namespace Models
{
    public class Report
    {
        public int Id { get; set; }

        // Sender information
        public int UserId { get; set; }
        public User? User { get; set; }

        // Category
        public int ReqCategoryId { get; set; }
        public ReqCategory? ReqCategory { get; set; }

        // Date (Created Date)
        public DateTime CreatedAt { get; set; }

        // First Operation Date (İlk icra tarixi)
        public DateTime? FirstOperationDate { get; set; }

        // Operation Time/Duration (İcra müddəti) - in days or hours
        public int? OperationTime { get; set; }

        // Executor (İcra edən)
        public int? ExecutorId { get; set; }
        public User? Executor { get; set; }

        // Close Date (Bağlanma tarixi)
        public DateTime? CloseDate { get; set; }

        // Status
        public int ReqStatusId { get; set; }
        public ReqStatus? ReqStatus { get; set; }

        // Optional: Reference to original request if needed
        public int? RequestId { get; set; }
        public Request? Request { get; set; }

        public bool isDeleted { get; set; }
    }
}