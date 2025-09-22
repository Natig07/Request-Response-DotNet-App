namespace DTOs
{

    public class ResStatusDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
    public enum ResponseStatus
    {
        Accepted = 1,
        Denied = 2
    }
}
