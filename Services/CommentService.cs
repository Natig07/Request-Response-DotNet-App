
using DTOs;
using Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Models;
using Exceptions;
namespace Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    private readonly IFileService _fileService;
    private readonly ILogger<CommentService> _logger;

    public CommentService(AppDbContext context, IMapper mapper, ILogger<CommentService> logger, IFileService fileService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _fileService = fileService;
    }

    public async Task<List<CommentDto>> GetCommentsByRequestIdAsync(int requestId)
    {
        var comments = await _context.Comments
            .Where(c => c.RequestId == requestId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var userIds = comments.Select(c => c.UserId).Distinct().ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var userDict = users.ToDictionary(x => x.Id, x => x);

        var commentDtos = comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Text = c.Text,
            CreatedAt = c.CreatedAt,
            AttachmentId = c.AttachmentId,
            UserId = c.UserId,
            User = new OutUserDto
            {
                Name = userDict[c.UserId].Name,
                Username = userDict[c.UserId].Username,
                Surname = userDict[c.UserId].Surname,
                Position = userDict[c.UserId].Position,
                ProfilePhotoId = userDict[c.UserId].ProfilePhotoId,
                Department = userDict[c.UserId].Department,
                MobTelNumber = userDict[c.UserId].MobTelNumber,
                OfficeTelNumber = userDict[c.UserId].OfficeTelNumber,
                AllowNotification = userDict[c.UserId].AllowNotification,
                Email = userDict[c.UserId].Email,

            }
        })
        .ToList();


        return commentDtos;
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentDto dto)
    {
        var comment = _mapper.Map<Comment>(dto);
        comment.CreatedAt = DateTime.UtcNow;
        if (dto.File != null)
        {
            _logger.LogInformation("Uploading file for request...");
            comment.AttachmentId = await _fileService.UploadAsync(dto.File);
        }

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var savedComment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

        if (savedComment == null)
        {
            _logger.LogError("Comment {CommentId} could not be retrieved after creation", comment.Id);
            throw new InternalServerException("Comment could not be retrieved after creation.");
        }
        var dtoResult = _mapper.Map<CommentDto>(savedComment);
        if (comment.AttachmentId.HasValue)
        {
            var file = await _fileService.GetFileAsync(comment.AttachmentId.Value);
            dtoResult.Attachment = file == null ? null : new FileEntityDto { Id = file.Id, FileName = file.FileName };
        }

        return _mapper.Map<CommentDto>(comment);
    }
}
