using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DTOs;
using Services;
using Models;
using Data;
using Exceptions;

public class ResponseService : IResponseService
{
    private readonly AppDbContext _context;
    private readonly FileDbContext _fileDbContext;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;
    private readonly ILogger<ResponseService> _logger;

    public ResponseService(
        AppDbContext context,
        IMapper mapper,
        IFileService fileService,
        ILogger<ResponseService> logger,
        FileDbContext fileDbContext)
    {
        _context = context;
        _mapper = mapper;
        _fileService = fileService;
        _logger = logger;
        _fileDbContext = fileDbContext;
    }

    public async Task ChangeResStat(int id, int newStatusId)
    {
        _logger.LogInformation("Changing response status for ResponseId={ResponseId} to StatusId={NewStatusId}", id, newStatusId);

        var existingResponse = await _context.Responses.FindAsync(id);
        if (existingResponse == null || existingResponse.isDeleted)
        {
            _logger.LogWarning("Response {ResponseId} not found or deleted", id);
            throw new NotFoundException($"Response with ID {id} not found.");
        }

        bool statusExists = await _context.Statuses.AnyAsync(s => s.Id == newStatusId);
        if (!statusExists)
        {
            _logger.LogWarning("Status {NewStatusId} does not exist", newStatusId);
            throw new BadRequestException($"Status with ID {newStatusId} does not exist.");
        }

        existingResponse.ResStatusId = newStatusId;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Response {ResponseId} status updated successfully", id);

        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating status for ResponseId={ResponseId}", id);
            throw new InternalServerException("Could not update response status due to database error.");
        }
    }

    public async Task<ResponseDto> CreateResAync(CreateResponseDto dto)
    {
        _logger.LogInformation("Creating response for RequestId={RequestId} by UserId={UserId}", dto.RequestId, dto.UserId);

        var existingRequest = await _context.Requests
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId && !r.isDeleted);

        if (existingRequest == null)
        {
            _logger.LogWarning("Request {RequestId} not found", dto.RequestId);
            throw new NotFoundException($"Request with ID {dto.RequestId} not found.");
        }

        var response = _mapper.Map<Response>(dto);
        response.isDeleted = false;

        if (dto.File != null)
        {
            _logger.LogInformation("Uploading file for response (RequestId={RequestId})", dto.RequestId);
            response.FileId = await _fileService.UploadAsync(dto.File);
        }

        try
        {
            _context.Responses.Add(response);

            if (existingRequest.ReqStatusId == (int)RequestStatus.New)
            {
                existingRequest.ReqStatusId = (int)RequestStatus.Completed;
                _logger.LogInformation("Request {RequestId} moved to Completed due to new response", dto.RequestId);
            }
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while creating response for RequestId={RequestId}", dto.RequestId);
            throw new InternalServerException("Could not create response due to database error.");
        }

        var savedResponse = await _context.Responses
            .Include(r => r.ResStatus)
            .Include(r => r.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(r => r.Id == response.Id);

        if (savedResponse == null)
        {
            _logger.LogError("Response {ResponseId} could not be retrieved after save", response.Id);
            throw new InternalServerException("Response could not be retrieved after save.");
        }

        var dtoResult = _mapper.Map<ResponseDto>(savedResponse);

        if (response.FileId.HasValue)
        {
            var file = await _fileService.GetFileAsync(response.FileId.Value);
            dtoResult.File = file == null ? null : new FileEntityDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Url = $"/uploads/{Path.GetFileName(file.Url)}"
            };
        }

        _logger.LogInformation("Response {ResponseId} created successfully", response.Id);
        return dtoResult;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting response {ResponseId}", id);

        var response = await _context.Responses.FindAsync(id);
        if (response == null || response.isDeleted)
        {
            _logger.LogWarning("Response {ResponseId} not found or already deleted", id);
            throw new NotFoundException($"Response with ID {id} not found.");
        }

        try
        {
            if (response.FileId.HasValue)
            {
                var file = await _fileDbContext.Files.FindAsync(response.FileId.Value);
                if (file != null)
                {
                    if (File.Exists(file.FilePath))
                        File.Delete(file.FilePath);

                    file.isDeleted = true;
                    _fileDbContext.Files.Update(file);
                    await _fileDbContext.SaveChangesAsync();
                    _logger.LogInformation("File {FileId} deleted successfully", file.Id);
                }
            }

            response.isDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Response {ResponseId} deleted successfully", id);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while deleting response {ResponseId}", id);
            throw new InternalServerException("Could not delete response due to database error.");
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "File system error while deleting file for response {ResponseId}", id);
            throw new InternalServerException("Could not delete response file due to file system error.");
        }
    }

    public async Task<IEnumerable<OutResponseDto>> GetAllResponsesAsync()
    {
        _logger.LogInformation("Fetching all active responses");
        try
        {
            var responses = await _context.Responses
                .Where(r => !r.isDeleted)
                .Include(r => r.User)
                .Include(r => r.ResStatus)
                .Select(r => new OutResponseDto
                {
                    Text = r.Text,
                    RequestId = r.RequestId,
                    RequestText = r.Request != null ? r.Request.Text : null,
                    Username = r.User != null ? r.User.Name : null,
                    Usersurname = r.User != null ? r.User.Surname : null,
                    ResStatusName = r.ResStatus != null ? r.ResStatus.Name : null,
                    FileId = r.FileId
                })
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} responses", responses.Count);
            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching responses");
            throw new InternalServerException("Could not fetch responses.");
        }
    }

    public async Task<OutResponseDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching response {ResponseId}", id);

        var response = await _context.Responses
            .Include(r => r.User)
            .Include(r => r.ResStatus)
            .Where(r => r.Id == id && !r.isDeleted)
            .Select(r => new OutResponseDto
            {
                Text = r.Text,
                RequestId = r.RequestId,
                RequestText = r.Request != null ? r.Request.Text : null,
                Username = r.User != null ? r.User.Name : null,
                Usersurname = r.User != null ? r.User.Surname : null,
                ResStatusName = r.ResStatus != null ? r.ResStatus.Name : null,
                FileId = r.FileId
            })
            .FirstOrDefaultAsync();

        if (response == null)
        {
            _logger.LogWarning("Response {ResponseId} not found", id);
            throw new NotFoundException($"Response with ID {id} not found.");
        }

        _logger.LogInformation("Response {ResponseId} retrieved successfully", id);
        return response;
    }

    public async Task UpdateAsync(int id, CreateResponseDto dto)
    {
        _logger.LogInformation("Updating response {ResponseId}", id);

        var existingResponse = await _context.Responses
            .Include(r => r.User)
            .ThenInclude(ur => ur!.UserRoles)
            .ThenInclude(ro => ro.Role)
            .Include(r => r.ResStatus)
            .Include(r => r.Request)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (existingResponse == null || existingResponse.isDeleted)
        {
            _logger.LogWarning("Response {ResponseId} not found or deleted", id);
            throw new NotFoundException($"Response with ID {id} not found.");
        }

        bool isModified =
            existingResponse.UserId != dto.UserId || existingResponse.RequestId != dto.RequestId || existingResponse.ResStatusId != dto.ResStatusId ||
            existingResponse.Text != dto.Text || dto.File != null;

        if (!isModified)
        {
            _logger.LogInformation("No changes detected for Response {ResponseId}", id);
        }
        try
        {
            existingResponse.UserId = dto.UserId;
            existingResponse.RequestId = dto.RequestId;
            existingResponse.ResStatusId = dto.ResStatusId;
            existingResponse.Text = dto.Text;

            if (dto.File != null)
            {
                _logger.LogInformation("Updating file for request {RequestId}", id);

                if (existingResponse.FileId.HasValue)
                {
                    await _fileService.DeleteAsync(existingResponse.FileId.Value);
                }

                existingResponse.FileId = await _fileService.UploadAsync(dto.File);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Response {ResponseId} updated successfully", id);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating response {ResponseId}", id);

            throw new InternalServerException("Could not update response due to database error.");
        }
    }
}
