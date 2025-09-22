using Models;

namespace DTOs
{
    public class ReqPriorityDto
    {
        public int Id { get; set; }
        public string? Level { get; set; }
        // public ICollection<Request>? Requests { get; set; }
    }
    public enum RequestPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Urgent = 4
    }
}