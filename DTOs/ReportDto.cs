namespace DTOs
{
    public class ReportDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? User { get; set; }

        public int ReqCategoryId { get; set; }
        public ReqCategoryDto? ReqCategory { get; set; }

        public DateTime? FirstOperationDate { get; set; }
        public int? OperationTime { get; set; } // in hours or days

        public int? ExecutorId { get; set; }
        public UserDto? Executor { get; set; }

        public DateTime? CloseDate { get; set; }

        public int ReqStatusId { get; set; }
        public ReqStatusDto? ReqStatus { get; set; }

        public int? RequestId { get; set; }
        public bool isDeleted { get; set; }
    }

    public class CreateReportDto
    {

        public int UserId { get; set; }
        public int ReqCategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FirstOperationDate { get; set; }
        public int? OperationTime { get; set; }
        public int? ExecutorId { get; set; }
        public DateTime? CloseDate { get; set; }
        public int ReqStatusId { get; set; }
        public int? RequestId { get; set; }
    }

    public class OutReportDto
    {
        public int Id { get; set; }
        public string? Sender { get; set; } // Username + Usersurname combined
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FirstOperDate { get; set; }
        public int? OperationTime { get; set; }
        public string? Executor { get; set; } // Executor name
        public DateTime? CloseDate { get; set; }
        public string? StatusName { get; set; }
    }
}