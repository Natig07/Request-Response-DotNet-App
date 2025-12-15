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
            _logger.LogInformation("Creating new report for user {UserId}", dto.UserId);
            _logger.LogInformation("Report dto before saving:{@Dto}", dto);

            var report = _mapper.Map<Report>(dto);
            report.isDeleted = false;

            try
            {
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating report for user {UserId}", dto.UserId);
                throw new InternalServerException("Could not create report due to database error.");
            }

            var savedReport = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.ReqCategory)
                .Include(r => r.ReqStatus)
                .Include(r => r.Executor)
                .Include(r => r.Request)
                .FirstOrDefaultAsync(r => r.Id == report.Id);

            if (savedReport == null)
            {
                _logger.LogError("Report {ReportId} could not be retrieved after save", report.Id);
                throw new InternalServerException("Report could not be retrieved after save.");
            }

            var dtoResult = _mapper.Map<ReportDto>(savedReport);
            _logger.LogInformation("Report {ReportId} created successfully", report.Id);
            return dtoResult;
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
    }
}