using Models;

namespace DTOs
{
    public class ReqTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        // public ICollection<Request>? Requests { get; set; }
    }
    public enum RequestType
    {
        AppChange = 1,
        BugFix = 2,
        NewFeature = 3,
        AccessRequest = 4
    }
}