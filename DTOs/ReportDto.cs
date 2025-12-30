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
        public int ReqPriorityId { get; set; }
        public int ReqTypeId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? FirstOperationDate { get; set; }

        public int? OperationTime { get; set; }
        public int? PlannedOperationTime { get; set; }

        public string? Result { get; set; }
        public string? Solution { get; set; }

        public int ReqStatusId { get; set; }

        public int? ExecutorId { get; set; }
        public DateTime? CloseDate { get; set; }

        public int? RequestId { get; set; }

        // Additional fields from your form
        public string Type { get; set; } = "ApplicationMaintenance"; // "ApplicationMaintenance" or "ApplicationDevelopment"
        // public string? RequestSender { get; set; }
        public int? SolmanReqNumber { get; set; }
        public string Communication { get; set; } = "Email"; // "Email", "Phone", "SOLMAN", "REQUEST"
        public bool IsRoutine { get; set; }
        public string? Code { get; set; }
        public string? RootCause { get; set; }
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

    public class ReportFilterDto
    {
        public int? CategoryId { get; set; }
        public int? ExecutorId { get; set; }
        public int? StatusId { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? SortField { get; set; }
        public string? SortDirection { get; set; }
    }

    public class PagedReportResult
    {
        public IEnumerable<OutReportDto> Items { get; set; } = new List<OutReportDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}