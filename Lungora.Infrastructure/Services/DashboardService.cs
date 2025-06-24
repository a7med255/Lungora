using Lungora.Bl.Interfaces;
using Lungora.Bl;
using Lungora.Dtos.DashboardDtos;
using Lungora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lungora.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly LungoraContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(LungoraContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var now = DateTime.UtcNow;
            var oneWeekAgo = now.AddDays(-7);

            var doctorsQuery = _context.TbDoctors
                .Include(a => a.Category)
                .Include(a => a.WorkingHours);

            var randomDoctors = await doctorsQuery.OrderBy(_ => Guid.NewGuid()).Take(5).ToListAsync();

            var randomArticles = await _context.TbArticles.Include(a => a.Category)
                .OrderBy(_ => Guid.NewGuid())
                .Take(5)
                .ToListAsync();

            var totalDoctors = await _context.TbDoctors.CountAsync();
            var doctorsLastWeek = await _context.TbDoctors.CountAsync(d => d.CreatedAt >= oneWeekAgo);
            var doctorPercentage = totalDoctors == 0 ? 0 : Math.Round((double)doctorsLastWeek / totalDoctors * 100, 2);

            var totalUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted);
            var usersLastWeek = await _userManager.Users.CountAsync(u => !u.IsDeleted && u.CreatedDate >= oneWeekAgo);
            var userPercentage = totalUsers == 0 ? 0 : Math.Round((double)usersLastWeek / totalUsers * 100, 2);

            var groupedPredictions = await _context.UserAIResults
                .GroupBy(p => p.Prediction)
                .Select(g => new PredictionStatsDto
                {
                    result = g.Key.ToString(),
                    count = g.Count(),
                    weeklyCount = g.Count(p => p.CreatedAt >= oneWeekAgo),
                    weeklyPercentage = g.Count() == 0 ? 0 : Math.Round((double)g.Count(p => p.CreatedAt >= oneWeekAgo) / g.Count() * 100, 2)
                })
                .ToListAsync();

            var aiModels = await _context.UserAIResults
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new AiModelDto
                {
                    user = a.User.Email,
                    createdAt = a.CreatedAt,
                    result = a.Prediction.ToString(),
                    status = a.Status.ToString()
                })
                .ToListAsync();

            return new DashboardDto
            {
                randomDoctorsFullData = randomDoctors,
                randomArticles = randomArticles,
                totalDoctors = totalDoctors,
                doctorsLastWeekPercentage = doctorPercentage,
                activeUserCount = totalUsers,
                activeUsersLastWeekPercentage = userPercentage,
                predictionStats = groupedPredictions,
                aiResult = aiModels
            };
        }
    }

}
