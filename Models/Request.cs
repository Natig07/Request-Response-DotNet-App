namespace Models
{
    public class Request
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string? Header { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int? ExecutorId { get; set; }
        public User? Executor { get; set; }


        public int? FileId { get; set; }
        // public FileEntity? File { get; set; }

        public int ReqCategoryId { get; set; }
        public ReqCategory? ReqCategory { get; set; }

        public int ReqPriorityId { get; set; }
        public ReqPriority? ReqPriority { get; set; }

        public int ReqStatusId { get; set; }
        public ReqStatus? ReqStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ReqTypeId { get; set; }
        public ReqType? ReqType { get; set; }

        public bool isDeleted { get; set; }

        public DateTime? FirstOperationDate { get; set; }


        public Response? Response { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();


    }


}