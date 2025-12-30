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

    private IRequestHistoryService _historyService;

    public RequestService(
        AppDbContext context,
        FileDbContext fileDbContext,
        IMapper mapper,
        IFileService fileService,
        ILogger<RequestService> logger,
        IRequestHistoryService requestHistoryService
        )
    {
        _context = context;
        _mapper = mapper;
        _fileService = fileService;
        _logger = logger;
        _fileDbcontext = fileDbContext;
        _historyService = requestHistoryService;
    }

    public async Task<RequestDto> CreateReqAsync(CreateRequestDto dto)
    {
        _logger.LogInformation("Creating new request for user {UserId}", dto.UserId);
        _logger.LogInformation("User dto before saving:{@Dto}", dto);

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
            .Include(r => r.ReqType)
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

        await _historyService.AddHistoryAsync(
                request.Id,
                savedRequest.User!.Id,
                "Status dəyişdirildi",
                "Yeni sorğu yaratdı"
            );
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
            {
                request.Response.isDeleted = true;
            }

            // Mark request file as deleted
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

            // Mark request as deleted
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
                .Include(r => r.Executor)
                .Select(r => new OutRequestDto
                {
                    Id = r.Id,
                    Text = r.Text,
                    Header = r.Header,
                    Username = r.User != null ? r.User.Name : null,
                    Usersurname = r.User != null ? r.User.Surname : null,
                    RequestTypeName = r.ReqType != null ? r.ReqType.Name : null,
                    CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                    StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null,
                    CreatedAt = r.CreatedAt,
                    PriorityName = r.ReqPriority != null ? r.ReqPriority.Level : null,
                    FileId = r.FileId,
                    ExecutorName = r.Executor != null ? r.Executor.Name : null,
                    ExecutorSurname = r.Executor != null ? r.Executor.Surname : null,

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
                Id = r.Id,
                Text = r.Text,
                Header = r.Header,
                Username = r.User != null ? r.User.Name : null,
                Usersurname = r.User != null ? r.User.Surname : null,
                CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null,
                RequestTypeName = r.ReqType != null ? r.ReqType.Name : null,
                CreatedAt = r.CreatedAt,
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
        || existingRequest.Header != dto.Header || existingRequest.ReqTypeId != dto.ReqTypeId
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
            existingRequest.Header = dto.Header;
            existingRequest.ReqTypeId = dto.ReqTypeId;
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



    public async Task ChangeReqStat(int requestId, int newStatusId, int UserId)
    {
        _logger.LogInformation("Changing status for request {RequestId} to {NewStatusId}", requestId, newStatusId);

        var existingRequest = await _context.Requests
            .Include(r => r.ReqStatus)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (existingRequest == null || existingRequest.isDeleted)
        {
            throw new NotFoundException($"Request with ID {requestId} not found");
        }

        var oldStatusId = existingRequest.ReqStatusId;

        existingRequest.ReqStatusId = newStatusId;

        try
        {
            await _context.SaveChangesAsync();

            var action = string.Empty;

            if (newStatusId == 2 && oldStatusId == 1)
            {
                action = "Sorğunu öz üzərinə götürdü";
            }
            else if (newStatusId == 6)
            {
                var report = await _context.Reports.FirstOrDefaultAsync(r => r.RequestId == requestId && !r.isDeleted);

                if (report != null)
                {
                    report.ReqStatusId = newStatusId;
                    report.CloseDate = DateTime.UtcNow;
                }
                action = "Sorğunu bağladı";
            }
            else if (newStatusId == 5)
            {
                action = "Sorğunu gözləməyə aldı";
            }
            else if (newStatusId == 4)
            {
                action = "Sorğudan imtina etdi";
            }
            else if (newStatusId == 1)
            {
                action = "Sorğunu açdı";
            }
            else
            {
                action = "Sorğunu icraya aldı";
            }


            await _historyService.AddHistoryAsync(
                requestId,
                UserId,
                "Status dəyişdirildi",
                action
            );

            _logger.LogInformation("Status changed successfully for request {RequestId}", requestId);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while changing status for request {RequestId}", requestId);
            throw new InternalServerException("Could not change request status due to database error.");
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
            Id = r.Id,
            Text = r.Text,
            Header = r.Header,
            Username = r.User != null ? r.User.Name : null,
            Usersurname = r.User != null ? r.User.Surname : null,
            CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
            StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null,
            PriorityName = r.ReqPriority != null ? r.ReqPriority.Level : null,
            RequestTypeName = r.ReqType != null ? r.ReqType.Name : null,
            CreatedAt = r.CreatedAt,
            FileId = r.FileId
        })
        .ToListAsync();

        return _mapper.Map<IEnumerable<OutRequestDto>>(requests);
    }

    public async Task<RequestDto> GetReqResByIdAsync(int id)
    {
        _logger.LogInformation("Fetching request {RequestId} with all responses", id);

        var request = await _context.Requests
            .Include(r => r.User)
            .ThenInclude(u => u!.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(r => r.ReqCategory)
            .Include(r => r.ReqPriority)
            .Include(r => r.ReqStatus)
            .Include(r => r.ReqType)
            .Include(r => r.Executor)
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

        var dtoResult = _mapper.Map<RequestDto>(request);

        // Attach file info for request
        if (request.FileId.HasValue)
        {
            var file = await _fileService.GetFileAsync(request.FileId.Value);
            dtoResult.File = file == null ? null : new FileEntityDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Url = $"/api/files/{file.Id}"
            };
        }

        if (request.User?.ProfilePhotoId != null)
        {
            var userPhoto = await _fileService.GetFileAsync(request.User.ProfilePhotoId.Value);

            dtoResult.User!.ProfilePhoto = userPhoto;
        }

        if (request.Executor != null)
        {
            dtoResult.Executor = _mapper.Map<UserDto>(request.Executor);
            dtoResult.ExecutorId = request.ExecutorId;
        }


        _logger.LogInformation("Request {RequestId} with all responses retrieved successfully", id);
        return dtoResult;
    }

    public async Task TakeRequestAsync(int requestId, int executorId)
    {
        _logger.LogInformation("User {ExecutorId} is trying to take request {RequestId}", executorId, requestId);

        var request = await _context.Requests
            .Include(r => r.Executor)
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.isDeleted);

        if (request == null)
            throw new NotFoundException("Request not found");

        if (request.ExecutorId != null)
            throw new BadRequestException("This request is already taken by another user");

        request.ExecutorId = executorId;


        request.ReqStatusId = 2;

        if (request.FirstOperationDate == null)
        {
            request.FirstOperationDate = DateTime.UtcNow;
        }
        try
        {
            await _context.SaveChangesAsync();

            _logger.LogInformation("Request {RequestId} successfully taken by {ExecutorId}", requestId, executorId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error while taking request {RequestId}", requestId);
            throw new InternalServerException("Could not take request due to database error.");
        }
    }

    public async Task<PagedResult<OutRequestDto>> GetFilteredAsync(RequestFilterDto filter)
    {

        var baseQuery = _context.Requests
            .Where(r => !r.isDeleted)
            .AsQueryable();


        if (filter.CategoryId.HasValue)
            baseQuery = baseQuery.Where(r => r.ReqCategoryId == filter.CategoryId);

        if (filter.PriorityId.HasValue)
            baseQuery = baseQuery.Where(r => r.ReqPriorityId == filter.PriorityId);

        if (filter.ExecutorId.HasValue)
            baseQuery = baseQuery.Where(r => r.ExecutorId == filter.ExecutorId);

        if (filter.FromDate.HasValue)
            baseQuery = baseQuery.Where(r => r.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            baseQuery = baseQuery.Where(r => r.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            baseQuery = baseQuery.Where(r =>
                r.Header!.ToLower().Contains(searchLower) ||
                r.Text!.ToLower().Contains(searchLower)
            );
        }

        // 🔹 3. STATUS COUNTS (status filter olmadan)
        var statusCounts = await baseQuery
            .GroupBy(r => r.ReqStatus!.Name)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Status!, x => x.Count);

        // 🔹 4. Main query (status filter BURADA)
        var query = baseQuery;

        if (filter.StatusId.HasValue)
            query = query.Where(r => r.ReqStatusId == filter.StatusId);

        // 🔹 5. Sorting
        if (!string.IsNullOrEmpty(filter.SortField))
        {
            bool asc = filter.SortDirection?.ToLower() != "desc";

            query = filter.SortField switch
            {
                "id" => asc ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id),
                "header" => asc ? query.OrderBy(r => r.Header) : query.OrderByDescending(r => r.Header),
                "username" => asc ? query.OrderBy(r => r.User!.Name) : query.OrderByDescending(r => r.User!.Name),
                "category" => asc ? query.OrderBy(r => r.ReqCategory!.Name) : query.OrderByDescending(r => r.ReqCategory!.Name),
                "status" => asc ? query.OrderBy(r => r.ReqStatus!.Name) : query.OrderByDescending(r => r.ReqStatus!.Name),
                "priority" => asc ? query.OrderBy(r => r.ReqPriority!.Level) : query.OrderByDescending(r => r.ReqPriority!.Level),
                "executor" => asc ? query.OrderBy(r => r.Executor!.Name) : query.OrderByDescending(r => r.Executor!.Name),
                "createdAt" => asc ? query.OrderBy(r => r.CreatedAt) : query.OrderByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(r => r.CreatedAt);
        }

        // 🔹 6. Total count (status filter LI)
        var totalCount = await query.CountAsync();

        // 🔹 7. Pagination
        var requests = await query
            .Include(r => r.User)
            .Include(r => r.ReqCategory)
            .Include(r => r.ReqPriority)
            .Include(r => r.ReqStatus)
            .Include(r => r.Executor)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new OutRequestDto
            {
                Id = r.Id,
                Header = r.Header,
                Text = r.Text,
                Username = r.User!.Name,
                Usersurname = r.User!.Surname,
                CategoryName = r.ReqCategory!.Name,
                StatusName = r.ReqStatus!.Name,
                PriorityName = r.ReqPriority!.Level,
                RequestTypeName = r.ReqType!.Name,
                ExecutorName = r.Executor != null ? r.Executor.Name : null,
                ExecutorSurname = r.Executor != null ? r.Executor.Surname : null,
                CreatedAt = r.CreatedAt,
                FileId = r.FileId
            })
            .ToListAsync();

        return new PagedResult<OutRequestDto>
        {
            Items = requests,
            TotalCount = totalCount,
            StatusCounts = statusCounts
        };
    }

    // Add these methods to your RequestService class

    public async Task<RequestDto> GetRequestWithDetails(int requestId, string section)
    {
        _logger.LogInformation("Fetching request {RequestId} with section {Section}", requestId, section);

        var baseQuery = _context.Requests
            .Include(r => r.User)
                .ThenInclude(u => u!.ProfilePhotoId)
            .Include(r => r.ReqCategory)
            .Include(r => r.ReqPriority)
            .Include(r => r.ReqStatus)
            .Include(r => r.ReqType)
            .Include(r => r.Executor)
            .Where(r => r.Id == requestId && !r.isDeleted);

        switch (section.ToLower())
        {
            case "request":
                // Load request with responses/comments
                var requestWithComments = await baseQuery
                    .Include(r => r.Response)
                        .ThenInclude(resp => resp!.User)
                        .ThenInclude(u => u!.ProfilePhotoId)
                    .FirstOrDefaultAsync();

                if (requestWithComments == null)
                    throw new NotFoundException("Request not found");

                return _mapper.Map<RequestDto>(requestWithComments);

            case "comment":
                // Only load comments
                var requestComments = await baseQuery
                    .Include(r => r.Response)
                        .ThenInclude(resp => resp!.User)
                        .ThenInclude(u => u!.ProfilePhotoId)
                    .FirstOrDefaultAsync();

                if (requestComments == null)
                    throw new NotFoundException("Request not found");

                return _mapper.Map<RequestDto>(requestComments);

            case "history":
                // Load history via separate call
                var requestHistory = await baseQuery.FirstOrDefaultAsync();

                if (requestHistory == null)
                    throw new NotFoundException("Request not found");

                var dto = _mapper.Map<RequestDto>(requestHistory);
                // History will be loaded separately via RequestHistoryService
                return dto;

            case "requestinfo":
                // Load request info only (no responses/history)
                var requestInfo = await baseQuery.FirstOrDefaultAsync();

                if (requestInfo == null)
                    throw new NotFoundException("Request not found");

                return _mapper.Map<RequestDto>(requestInfo);

            default:
                // Default: load everything
                var fullRequest = await baseQuery
                    .Include(r => r.Response)
                        .ThenInclude(resp => resp!.User)
                    .FirstOrDefaultAsync();

                if (fullRequest == null)
                    throw new NotFoundException("Request not found");

                return _mapper.Map<RequestDto>(fullRequest);
        }
    }

    // Get comment count for a request
    public async Task<int> GetCommentCountAsync(int requestId)
    {
        return await _context.Responses
            .Where(r => r.RequestId == requestId && !r.isDeleted)
            .CountAsync();
    }




}
