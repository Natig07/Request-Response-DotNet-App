
namespace DTOs
{
    public class RequestDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string? Header { get; set; }

        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? User { get; set; }

        public int? ExecutorId { get; set; }
        public UserDto? Executor { get; set; }


        public FileEntityDto? File { get; set; }

        public int ReqCategoryId { get; set; }
        public ReqCategoryDto? ReqCategory { get; set; }

        public int ReqTypeId { get; set; }
        public ReqTypeDto? ReqType { get; set; }


        public int ReqPriorityId { get; set; }
        public ReqPriorityDto? ReqPriority { get; set; }

        public int ReqStatusId { get; set; }
        public ReqStatusDto? ReqStatus { get; set; }

        public bool isDeleted { get; set; }

        public ResponseDto? Response { get; set; }

        public List<CommentDto> Comments { get; set; } = new();

    }

    public class CreateRequestDto
    {
        public int Id { get; set; }

        public string? Text { get; set; }
        public int UserId { get; set; }
        public int ReqCategoryId { get; set; }
        public int ReqPriorityId { get; set; }
        public int ReqTypeId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? Header { get; set; }
        // public int ReqStatusId { get; set; }
        public IFormFile? File { get; set; }
    }

    public class OutRequestDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string? Header { get; set; }
        public string? Username { get; set; }
        public string? Usersurname { get; set; }
        public string? CategoryName { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ExecutorName { get; set; }
        public string? ExecutorSurname { get; set; }


        public string? PriorityName { get; set; }
        public string? RequestTypeName { get; set; }
        public int? FileId { get; set; }

        public List<CommentDto> Comments { get; set; } = new();



    }


}