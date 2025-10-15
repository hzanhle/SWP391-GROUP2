using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService;
using UserService.Models;
using UserService.Models.Enums;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<VerificationController> _logger;

        public VerificationController(MyDbContext context, ILogger<VerificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public class VerificationItem
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string? FullName { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public DocumentState Citizen { get; set; } = new();
            public DocumentState Driver { get; set; } = new();
        }

        public class DocumentState
        {
            public string? Status { get; set; }
            public bool? IsApproved { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class PagedResult<T>
        {
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public List<T> Items { get; set; } = new();
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? status = null, // none | submitted | approved
            [FromQuery] string? query = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 200);

            var baseUsers = _context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                baseUsers = baseUsers.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(q)) ||
                    (u.Email != null && u.Email.ToLower().Contains(q)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(q)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(q))
                );
            }

            var users = await baseUsers
                .Select(u => new { u.Id, u.UserName, u.FullName, u.Email, u.PhoneNumber })
                .ToListAsync();

            var citizenLatest = await _context.CitizenInfos
                .AsNoTracking()
                .GroupBy(ci => ci.UserId)
                .Select(g => g.OrderByDescending(x => x.DayCreated).FirstOrDefault())
                .ToListAsync();
            var citizenDict = citizenLatest
                .Where(x => x != null)
                .ToDictionary(x => x!.UserId, x => x);

            var driverLatest = await _context.DriverLicenses
                .AsNoTracking()
                .GroupBy(dl => dl.UserId)
                .Select(g => g.OrderByDescending(x => x.DateCreated).FirstOrDefault())
                .ToListAsync();
            var driverDict = driverLatest
                .Where(x => x != null)
                .ToDictionary(x => x!.UserId, x => x);

            bool IsApprovedBoth(int userId)
            {
                var c = citizenDict.ContainsKey(userId) ? citizenDict[userId] : null;
                var d = driverDict.ContainsKey(userId) ? driverDict[userId] : null;
                return c != null && d != null && c.Status == StatusInformation.Approved && d.Status == StatusInformation.Approved;
            }

            bool IsSubmitted(int userId)
            {
                var c = citizenDict.ContainsKey(userId) ? citizenDict[userId] : null;
                var d = driverDict.ContainsKey(userId) ? driverDict[userId] : null;
                return (c != null && c.Status == StatusInformation.Pending) || (d != null && d.Status == StatusInformation.Pending);
            }

            bool IsNone(int userId)
            {
                return !citizenDict.ContainsKey(userId) && !driverDict.ContainsKey(userId);
            }

            IEnumerable<VerificationItem> enriched = users.Select(u => new VerificationItem
            {
                UserId = u.Id,
                UserName = u.UserName ?? string.Empty,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber,
                Citizen = new DocumentState
                {
                    Status = citizenDict.ContainsKey(u.Id) ? citizenDict[u.Id].Status : null,
                    IsApproved = citizenDict.ContainsKey(u.Id) ? citizenDict[u.Id].IsApproved : null,
                    CreatedAt = citizenDict.ContainsKey(u.Id) ? citizenDict[u.Id].DayCreated : null
                },
                Driver = new DocumentState
                {
                    Status = driverDict.ContainsKey(u.Id) ? driverDict[u.Id].Status : null,
                    IsApproved = driverDict.ContainsKey(u.Id) ? driverDict[u.Id].IsApproved : null,
                    CreatedAt = driverDict.ContainsKey(u.Id) ? driverDict[u.Id].DateCreated : null
                }
            });

            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim().ToLower();
                if (s == "approved") enriched = enriched.Where(x => IsApprovedBoth(x.UserId));
                else if (s == "submitted") enriched = enriched.Where(x => IsSubmitted(x.UserId) && !IsApprovedBoth(x.UserId));
                else if (s == "none") enriched = enriched.Where(x => IsNone(x.UserId));
            }

            var total = enriched.Count();
            var items = enriched
                .OrderByDescending(x => x.Citizen.CreatedAt ?? DateTime.MinValue)
                .ThenByDescending(x => x.Driver.CreatedAt ?? DateTime.MinValue)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<VerificationItem>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = items
            };
            return Ok(result);
        }
    }
}
