namespace Models
{
    public class Report
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int ReqCategoryId { get; set; }
        public ReqCategory? ReqCategory { get; set; }

        public int ReqPriorityId { get; set; }
        public ReqPriority? ReqPriority { get; set; }

        public int ReqTypeId { get; set; }
        public ReqType? ReqType { get; set; }

        public int ReqStatusId { get; set; }
        public ReqStatus? ReqStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? FirstOperationDate { get; set; }
        public int? OperationTime { get; set; }
        public int? PlannedOperationTime { get; set; }
        public string? Result { get; set; }
        public string? Solution { get; set; }

        public int? ExecutorId { get; set; }
        public User? Executor { get; set; }

        public DateTime? CloseDate { get; set; }

        public int? RequestId { get; set; }
        public Request? Request { get; set; }

        // NEW FIELDS
        public string Type { get; set; } = "ApplicationMaintenance";
        public string? RequestSender { get; set; }
        public int? SolmanReqNumber { get; set; }
        public string Communication { get; set; } = "Email";
        public bool IsRoutine { get; set; }
        public string? Code { get; set; }
        public string? RootCause { get; set; }

        public bool isDeleted { get; set; }
    }
}