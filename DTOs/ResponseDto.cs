
namespace DTOs
{
    public class ResponseDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }

        public int RequestId { get; set; }
        // public RequestDto? Request { get; set; }

        public int ResStatusId { get; set; }
        public ResStatusDto? ResStatus { get; set; }

        public FileEntityDto? File { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public UserDto? User { get; set; }

        public bool isDeleted { get; set; }



    }

    public class CreateResponseDto
    {
        public string? Text { get; set; }

        public int RequestId { get; set; }

        public int ResStatusId { get; set; }

        public int UserId { get; set; }

        public IFormFile? File { get; set; }
        public DateTime CreatedAt { get; set; }

    }

    public class OutResponseDto
    {
        public string? Text { get; set; }

        public int RequestId { get; set; }
        public string? RequestText { get; set; }
        public string? ResStatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Username { get; set; }
        public string? Usersurname { get; set; }
        public int? FileId { get; set; }
    }
}