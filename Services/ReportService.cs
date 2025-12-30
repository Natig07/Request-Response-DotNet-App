using AutoMapper;
using Data;
using DTOs;
using Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Services.Interfaces;

namespace Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            AppDbContext context,
            IMapper mapper,
            ILogger<ReportService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReportDto> CreateReportAsync(CreateReportDto dto)
        {
            _logger.LogInformation("Creating report for user {UserId}", dto.UserId);

            var report = _mapper.Map<Report>(dto);


            report.isDeleted = false;

            try
            {
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error while creating report");
                throw new InternalServerException("Could not create report.");
            }

            var savedReport = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ReqCategory)
                .Include(r => r.ReqPriority)
                .Include(r => r.ReqType)
                .Include(r => r.ReqStatus)
                .Include(r => r.Executor)
                .Include(r => r.Request)
                .FirstOrDefaultAsync(r => r.Id == report.Id);

            if (savedReport == null)
                throw new InternalServerException("Report saved but could not be loaded.");

            return _mapper.Map<ReportDto>(savedReport);
        }

        public async Task<IEnumerable<OutReportDto>> GetAllReportsAsync()
        {
            _logger.LogInformation("Fetching all active reports");

            try
            {
                var reports = await _context.Reports
                    .Where(r => !r.isDeleted)
                    .Include(r => r.User)
                    .Include(r => r.ReqCategory)
                    .Include(r => r.ReqStatus)
                    .Include(r => r.Executor)
                    .Select(r => new OutReportDto
                    {
                        Id = r.Id,
                        Sender = r.User != null ? $"{r.User.Name} {r.User.Surname}" : null,
                        CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                        CreatedAt = r.CreatedAt,
                        FirstOperDate = r.FirstOperationDate,
                        OperationTime = r.OperationTime,
                        Executor = r.Executor != null ? $"{r.Executor.Name} {r.Executor.Surname}" : null,
                        CloseDate = r.CloseDate,
                        StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} reports", reports.Count);
                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching all reports");
                throw new InternalServerException("Could not fetch reports.");
            }
        }

        public async Task<OutReportDto?> GetReportByIdAsync(int id)
        {
            _logger.LogInformation("Fetching report with ID {ReportId}", id);

            try
            {
                var report = await _context.Reports
                    .Where(r => r.Id == id && !r.isDeleted)
                    .Include(r => r.User)
                    .Include(r => r.ReqCategory)
                    .Include(r => r.ReqStatus)
                    .Include(r => r.Executor)
                    .Select(r => new OutReportDto
                    {
                        Id = r.Id,
                        Sender = r.User != null ? $"{r.User.Name} {r.User.Surname}" : null,
                        CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                        CreatedAt = r.CreatedAt,
                        FirstOperDate = r.FirstOperationDate,
                        OperationTime = r.OperationTime,
                        Executor = r.Executor != null ? $"{r.Executor.Name} {r.Executor.Surname}" : null,
                        CloseDate = r.CloseDate,
                        StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null
                    })
                    .FirstOrDefaultAsync();

                if (report == null)
                {
                    _logger.LogWarning("Report with ID {ReportId} not found", id);
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching report {ReportId}", id);
                throw new InternalServerException("Could not fetch report.");
            }
        }

        public async Task<PagedReportResult> GetFilteredReportsAsync(ReportFilterDto filter)
        {
            _logger.LogInformation("Fetching filtered reports with parameters: {@Filter}", filter);

            try
            {
                var query = _context.Reports
                    .Where(r => !r.isDeleted)
                    .Include(r => r.User)
                    .Include(r => r.ReqCategory)
                    .Include(r => r.ReqStatus)
                    .Include(r => r.Executor)
                    .AsQueryable();

                // Apply category filter
                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(r => r.ReqCategoryId == filter.CategoryId.Value);
                }

                // Apply executor filter
                if (filter.ExecutorId.HasValue)
                {
                    query = query.Where(r => r.ExecutorId == filter.ExecutorId.Value);
                }

                // Apply status filter
                if (filter.StatusId.HasValue)
                {
                    query = query.Where(r => r.ReqStatusId == filter.StatusId.Value);
                }

                // Apply search filter (searches across multiple fields)
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var searchLower = filter.Search.ToLower();
                    query = query.Where(r =>
                        r.Id.ToString().Contains(searchLower) ||
                        (r.User != null && (r.User.Name + " " + r.User.Surname).ToLower().Contains(searchLower)) ||
                        (r.ReqCategory != null && r.ReqCategory.Name!.ToLower().Contains(searchLower)) ||
                        (r.Executor != null && (r.Executor.Name + " " + r.Executor.Surname).ToLower().Contains(searchLower)) ||
                        (r.ReqStatus != null && r.ReqStatus.Name!.ToLower().Contains(searchLower))
                    );
                }

                //Arrow sorting
                if (!string.IsNullOrEmpty(filter.SortField))
                {
                    bool asc = filter.SortDirection?.ToLower() != "desc";

                    query = filter.SortField switch
                    {
                        "id" => asc
                            ? query.OrderBy(r => r.Id)
                            : query.OrderByDescending(r => r.Id),

                        "sender" => asc
                            ? query.OrderBy(r => r.User!.Name)
                            : query.OrderByDescending(r => r.User!.Name),

                        "category" => asc
                            ? query.OrderBy(r => r.ReqCategory!.Name)
                            : query.OrderByDescending(r => r.ReqCategory!.Name),

                        "date" => asc
                            ? query.OrderBy(r => r.CreatedAt)
                            : query.OrderByDescending(r => r.CreatedAt),

                        "status" => asc
                            ? query.OrderBy(r => r.ReqStatus!.Name)
                            : query.OrderByDescending(r => r.ReqStatus!.Name),

                        "firstOperationDate" => asc
                            ? query.OrderBy(r => r.FirstOperationDate)
                            : query.OrderByDescending(r => r.FirstOperationDate),

                        "operationTime" => asc
                            ? query.OrderBy(r => r.OperationTime)
                            : query.OrderByDescending(r => r.OperationTime),

                        "executor" => asc
                            ? query.OrderBy(r => r.Executor!.Name)
                            : query.OrderByDescending(r => r.Executor!.Name),

                        "closedate" => asc
                            ? query.OrderBy(r => r.CloseDate)
                            : query.OrderByDescending(r => r.CloseDate),


                        _ => query.OrderByDescending(r => r.CreatedAt),

                    };
                }
                else
                {
                    query = query.OrderByDescending(r => r.CreatedAt);
                }


                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.Date; // ignore time
                    query = query.Where(r => r.CreatedAt >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = filter.ToDate.Value.Date.AddDays(1); // include entire day
                    query = query.Where(r => r.CreatedAt < to);
                }



                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(r => new OutReportDto
                    {
                        Id = r.Id,
                        Sender = r.User != null ? $"{r.User.Name} {r.User.Surname}" : null,
                        CategoryName = r.ReqCategory != null ? r.ReqCategory.Name : null,
                        CreatedAt = r.CreatedAt,
                        FirstOperDate = r.FirstOperationDate,
                        OperationTime = r.OperationTime,
                        Executor = r.Executor != null ? $"{r.Executor.Name} {r.Executor.Surname}" : null,
                        CloseDate = r.CloseDate,
                        StatusName = r.ReqStatus != null ? r.ReqStatus.Name : null
                    })
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} filtered reports out of {Total}", items.Count, totalCount);

                return new PagedReportResult
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching filtered reports");
                throw new InternalServerException("Could not fetch filtered reports.");
            }
        }

        public async Task<OutReportDto?> GetReportByRequestId(int requestId)
        {
            _logger.LogInformation("Fetching report with requestId {requestId}", requestId);

            try
            {
                var report = await _context.Reports
                    .Include(r => r.User)
                    .Include(r => r.ReqCategory)
                    .Include(r => r.ReqPriority)
                    .Include(r => r.ReqType)
                    .Include(r => r.ReqStatus)
                    .Include(r => r.Executor)
                    .Include(r => r.Request)
                    .Where(r => r.RequestId == requestId && !r.isDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (report == null)
                {
                    _logger.LogInformation("No report found for RequestId {RequestId}", requestId);
                    return null; // Return null instead of throwing exception
                }

                _logger.LogInformation("Report for RequestId {RequestId} retrieved successfully", requestId);
                var reportDto = _mapper.Map<OutReportDto>(report);
                return reportDto;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while fetching report for RequestId {RequestId}", requestId);
                throw new InternalServerException("Could not fetch report.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching report for RequestId {RequestId}", requestId);
                throw new InternalServerException("Could not fetch report.");
            }
        }
    }
}