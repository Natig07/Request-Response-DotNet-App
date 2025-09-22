using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DTOs;
using Services;
using Models;
using Data;
using Exceptions;

public class RequestService : IRequestService
{
    private readonly AppDbContext _context;
    private readonly FileDbContext _fileDbcontext;
    private readonly IMapper _mapper;
    private readonly ILogger<RequestService> _logger;
    private readonly IFileService _fileService;

    public RequestService(
        AppDbContext context,
        FileDbContext fileDbContext,
        IMapper mapper,
        IFileService fileService,
        ILogger<RequestService> logger)
    {
        _context = context;
        _mapper = mapper;
        _fileService = fileService;
        _logger = logger;
        _fileDbcontext = fileDbContext;
    }

    public async Task<RequestDto> CreateReqAsync(CreateRequestDto dto)
    {
        _logger.LogInformation("Creating new request for user {UserId}", dto.UserId);

        var request = _mapper.Map<Request>(dto);
        request.ReqStatusId = (int)RequestStatus.New;
        request.isDeleted = false; if (dto.File != null)
        {
            _logger.LogInformation("Uploading file for request...");
            request.FileId = await _fileService.UploadAsync(dto.File);
        }
        try
        {
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while creating request for user {UserId}", dto.UserId);
            throw new InternalServerException("Could not create request due to database error.");
        }
        var savedRequest = await _context.Requests
            .Include(r => r.User)
            .Include(r => r.ReqCategory)
            .Include(r => r.ReqPriority)
            .Include(r => r.ReqStatus)
            .Include(r => r.Response)
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        if (savedRequest == null)
        {
            _logger.LogError("Request {RequestId} could not be retrieved after save", request.Id);
            throw new InternalServerException("Request could not be retrieved after save.");
        }
        var dtoResult = _mapper.Map<RequestDto>(savedRequest);
        if (request.FileId.HasValue)
        {
            var file = await _fileService.GetFileAsync(request.FileId.Value);
            dtoResult.File = file == null ? null : new FileEntityDto { Id = file.Id, FileName = file.FileName };
        }
        _logger.LogInformation("Request {RequestId} created successfully", request.Id);
        return dtoResult;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting request {RequestId}", id);

        var request = await _context.Requests
            .Include(r => r.Response)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null || request.isDeleted)
        {
            _logger.LogWarning("Delete failed. Request {RequestId} not found or already deleted", id);
            throw new NotFoundException($"Request with ID {id} not found.");
        }

        try
        {
            if (request.Response != null)
                request.Response.isDeleted = true;

            if (request.FileId.HasValue)
            {
                var file = await _fileDbcontext.Files.FindAsync(request.FileId.Value);
                if (file != null)
                {
                    file.isDeleted = true;
                    _fileDbcontext.Files.Update(file);
                    await _fileDbcontext.SaveChangesAsync();
                    _logger.LogInformation("File {FileId} marked as deleted", file.Id);
                }
            }

            request.isDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Request {RequestId} deleted successfully", id);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while deleting request {RequestId}", id);
            throw new InternalServerException("Could not delete request due to database error.");
        }
    }

    public async Task<IEnumerable<OutRequestDto>> GetAllRequestsAsync()
    {
        _logger.LogInformation("Fetching all active requests");

        try
        {
            var requests = await _context.Requests
                .Where(r => !r.isDeleted)
                .Include(r => r.User)
                .Include(r => r.ReqCategory)
                .Include(r => r.ReqPriority)
                .Include(r => r.ReqStatus)
                .Select(r => new OutRequestDto
                {
                    Text = r.Text,
                    Username = r.User != null ? r.User.Name : null,
                    Usersurname = r.User != null ? r.User.Surname : null,
                    CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                    StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null,
                    PriorityName = r.ReqPriority != null ? r.ReqPriority.Level : null,
                    FileId = r.FileId
                })
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} requests", requests.Count);
            return requests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all requests");
            throw new InternalServerException("Could not fetch requests.");
        }
    }

    public async Task<OutRequestDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching request {RequestId}", id);

        var request = await _context.Requests
            .Include(r => r.User)
            .Include(r => r.ReqCategory)
            .Include(r => r.ReqPriority)
            .Include(r => r.ReqStatus)
            .Where(r => r.Id == id && !r.isDeleted)
            .Select(r => new OutRequestDto
            {
                Text = r.Text,
                Username = r.User != null ? r.User.Name : null,
                Usersurname = r.User != null ? r.User.Surname : null,
                CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null,
                PriorityName = r.ReqPriority != null ? r.ReqPriority.Level : null,
                FileId = r.FileId
            })
            .FirstOrDefaultAsync();

        if (request == null)
        {
            _logger.LogWarning("Request {RequestId} not found", id);
            throw new NotFoundException($"Request with ID {id} not found.");
        }

        _logger.LogInformation("Request {RequestId} retrieved successfully", id);
        return request;
    }

    public async Task UpdateAsync(int id, CreateRequestDto dto)
    {
        _logger.LogInformation("Updating request {RequestId}", id);
        var existingRequest = await _context.Requests
        .Include(r => r.User)
        .Include(r => r.ReqCategory)
        .Include(r => r.ReqPriority)
        .Include(r => r.ReqStatus)
        .Include(r => r.Response)
        .FirstOrDefaultAsync(r => r.Id == id);
        if (existingRequest == null || existingRequest.isDeleted)
        {
            _logger.LogWarning("Update failed. Request {RequestId} not found", id);

            throw new NotFoundException($"Request with ID {id} not found.");
        }
        bool isModified = existingRequest.UserId != dto.UserId
        || existingRequest.ReqCategoryId != dto.ReqCategoryId
        || existingRequest.ReqPriorityId != dto.ReqPriorityId
        || existingRequest.Text != dto.Text
        || dto.File != null;
        if (!isModified)
        {
            _logger.LogInformation("No changes detected for request {RequestId}", id);
            return;
        }
        try
        {
            existingRequest.UserId = dto.UserId;
            existingRequest.ReqCategoryId = dto.ReqCategoryId;
            existingRequest.ReqPriorityId = dto.ReqPriorityId;
            existingRequest.Text = dto.Text;
            if (dto.File != null)
            {
                _logger.LogInformation("Updating file for request {RequestId}", id);
                if (existingRequest.FileId.HasValue)
                {
                    await _fileService.DeleteAsync(existingRequest.FileId.Value);
                }

                existingRequest.FileId = await _fileService.UploadAsync(dto.File);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Request {RequestId} updated successfully", id);

        }

        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating request {RequestId}", id);

            throw new InternalServerException("Could not update request due to database error.");
        }
    }



    public async Task ChangeReqStat(int requestId, int newStatusId)
    {
        _logger.LogInformation("Changing status of request {RequestId} to {NewStatusId}", requestId, newStatusId);

        var existingRequest = await _context.Requests.FindAsync(requestId);
        if (existingRequest == null || existingRequest.isDeleted)
        {
            _logger.LogWarning("Request {RequestId} not found", requestId);
            throw new NotFoundException($"Request with ID {requestId} not found.");
        }

        bool statusExists = await _context.Statuses.AnyAsync(s => s.Id == newStatusId);
        if (!statusExists)
        {
            throw new BadRequestException($"Status with ID {newStatusId} does not exist.");
        }

        existingRequest.ReqStatusId = newStatusId;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Request {RequestId} status updated to {NewStatusId}", requestId, newStatusId);

        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating status for request {RequestId}", requestId);
            throw new InternalServerException("Could not update request status due to database error.");
        }
    }

    public async Task<IEnumerable<OutRequestDto>> GetByCategoryAsync(int Id)
    {
        var requests = await _context.Requests
        .Include(r => r.User)
        .Include(r => r.ReqCategory)
        .Include(r => r.ReqPriority)
        .Include(r => r.ReqStatus)
        .Where(r => r.ReqCategoryId == Id && !r.isDeleted)
        .Select(r => new OutRequestDto
        {
            Text = r.Text,
            Username = r.User != null ? r.User.Name : null,
            Usersurname = r.User != null ? r.User.Surname : null,
            CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
            StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null,
            PriorityName = r.ReqPriority != null ? r.ReqPriority.Level : null,
            FileId = r.FileId
        })
        .ToListAsync();

        return _mapper.Map<IEnumerable<OutRequestDto>>(requests);
    }

    public async Task<RequestDto> GetReqResByIdAsync(int id)
    {
        _logger.LogInformation("Fetching response for request {RequestId}", id);
        var request = await _context.Requests
        .Include(r => r.User)
        .ThenInclude(u => u!.UserRoles)
        .ThenInclude(ur => ur.Role)
        .Include(r => r.ReqCategory)
        .Include(r => r.ReqPriority)
        .Include(r => r.ReqStatus)
        .Include(r => r.Response)
        .ThenInclude(resp => resp!.ResStatus)
        .Include(r => r.Response)
        .ThenInclude(resp => resp!.User)
        .ThenInclude(u => u!.UserRoles)
        .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(r => r.Id == id && !r.isDeleted);
        if (request == null)
        {
            _logger.LogWarning("Request {RequestId} not found", id);
            throw new NotFoundException("Request not found");
        }
        if (request.Response != null && request.Response.isDeleted)
            request.Response = null;

        _logger.LogInformation("Response for Request {RequestId} retrieved successfully", id);


        var dtoResult = _mapper.Map<RequestDto>(request);

        if (request!.FileId.HasValue)
        {
            var file = await _fileService.GetFileAsync(request.FileId.Value);
            dtoResult.File = file == null ? null : new FileEntityDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Url = $"/api/files/{file.Id}"
            };
        }

        if (request.Response!.FileId.HasValue)
        {
            var file = await _fileService.GetFileAsync(request.Response.FileId.Value);
            dtoResult.Response!.File = file == null ? null : new FileEntityDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Url = $"/api/files/{file.Id}"
            };
        }


        return dtoResult;
    }


}
