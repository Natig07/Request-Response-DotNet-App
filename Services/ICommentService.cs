using DTOs;

namespace Services;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentsByRequestIdAsync(int requestId);
    Task<CommentDto> AddCommentAsync(CreateCommentDto dto);
}
