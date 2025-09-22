using Models;

namespace DTOs
{
    public class ReqStatusDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // public ICollection<Request>? Requests { get; set; }
    }
    public enum RequestStatus
    {
        New = 1,
        InProgress = 2,
        Completed = 3,
        Denied = 4
    }



}