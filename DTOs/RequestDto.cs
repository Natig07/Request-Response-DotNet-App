
namespace DTOs
{
    public class RequestDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }

        public int UserId { get; set; }
        public UserDto? User { get; set; }

        public FileEntityDto? File { get; set; }

        public int ReqCategoryId { get; set; }
        public ReqCategoryDto? ReqCategory { get; set; }

        public int ReqPriorityId { get; set; }
        public ReqPriorityDto? ReqPriority { get; set; }

        public int ReqStatusId { get; set; }
        public ReqStatusDto? ReqStatus { get; set; }

        public bool isDeleted { get; set; }

        public ResponseDto? Response { get; set; }
    }

    public class CreateRequestDto
    {
        public string? Text { get; set; }
        public int UserId { get; set; }
        public int ReqCategoryId { get; set; }
        public int ReqPriorityId { get; set; }
        // public int ReqStatusId { get; set; }
        public IFormFile? File { get; set; }
    }

    public class OutRequestDto
    {
        public string? Text { get; set; }
        public string? Username { get; set; }
        public string? Usersurname { get; set; }
        public string? CategoryName { get; set; }
        public string? StatusName { get; set; }
        public string? PriorityName { get; set; }
        public int? FileId { get; set; }


    }


}