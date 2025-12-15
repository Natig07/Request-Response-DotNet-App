using Models;

namespace DTOs;


public class CommentDto
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? AttachmentId { get; set; }
    public FileEntityDto? Attachment { get; set; }
    public int UserId { get; set; }
    public OutUserDto? User { get; set; }
}
public class CreateCommentDto
{
    public string? Text { get; set; }
    public int RequestId { get; set; }
    public int UserId { get; set; }
    public IFormFile? File { get; set; }
}


