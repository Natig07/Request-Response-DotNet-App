namespace Models
{
    public class Request
    {
        public int Id { get; set; }
        public string? Text { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int? FileId { get; set; }
        // public FileEntity? File { get; set; }

        public int ReqCategoryId { get; set; }
        public ReqCategory? ReqCategory { get; set; }

        public int ReqPriorityId { get; set; }
        public ReqPriority? ReqPriority { get; set; }

        public int ReqStatusId { get; set; }
        public ReqStatus? ReqStatus { get; set; }

        public bool isDeleted { get; set; }

        public Response? Response { get; set; }

    }


}