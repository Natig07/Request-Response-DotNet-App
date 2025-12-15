using AutoMapper;
using Data;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Services;

namespace Services
{
    public class RequestHistoryService : IRequestHistoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RequestHistoryService> _logger;

        public RequestHistoryService(
            AppDbContext context,
            IMapper mapper,
            ILogger<RequestHistoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddHistoryAsync(int requestId, int userId, string action, string description)
        {
            _logger.LogInformation("Adding history for request {RequestId}: {Action}", requestId, action);

            var history = new RequestHistory
            {
                RequestId = requestId,
                UserId = userId,
                Action = action,
                OldValue = null,
                NewValue = null,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestHistories.Add(history);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<OutRequestHistoryDto>> GetRequestHistoryAsync(int requestId)
        {
            _logger.LogInformation("Fetching history for request {RequestId}", requestId);

            var histories = await _context.RequestHistories
                .Where(h => h.RequestId == requestId)
                .Include(h => h.User)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new OutRequestHistoryDto
                {
                    Id = h.Id,
                    UserName = h.User != null ? h.User.Name : null,
                    UserSurname = h.User != null ? h.User.Surname : null,
                    UserPosition = h.User != null ? h.User.Position : null,
                    Action = h.Action,
                    Description = h.Description,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return histories;
        }
    }
}