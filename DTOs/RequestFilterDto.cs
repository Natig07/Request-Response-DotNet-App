namespace DTOs;

public class RequestFilterDto
{
    public int? CategoryId { get; set; }
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public int? ExecutorId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string? Search { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
}
