namespace DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }

    public Dictionary<string, int> StatusCounts { get; set; } = new();
}
