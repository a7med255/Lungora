using Lungora.Bl;
using Lungora.Bl.Interfaces;
using Lungora.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly API_Resonse response;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly LungoraContext context;




        public DashboardController(LungoraContext context, UserManager<ApplicationUser> userManager)
        {
            response = new API_Resonse();
            this.userManager = userManager;
            this.context = context;
        }

        [HttpGet("GetDashboardData")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var now = DateTime.UtcNow;
                var oneWeekAgo = now.AddDays(-7);

                // 1. All doctors (Name, Image, Location only)
                var allDoctors = await context.TbDoctors
                    .Select(d => new
                    {
                        d.Name,
                        d.ImageDoctor,
                        d.Latitude,
                        d.Longitude
                    })
                    .ToListAsync();

                // 2. 5 Random Doctors (Full data)
                var randomDoctorsFull = await context.TbDoctors
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(5)
                    .ToListAsync();

                // 3. 4 Random Articles (random)
                var randomArticles = await context.TbArticles
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(4)
                    .ToListAsync();

                // 4. Users: Active Count & Weekly Percentage
                var totalActiveUsers = await userManager.Users
                    .CountAsync(u => u.IsDeleted == false);

                var activeUsersLastWeek = await userManager.Users
                    .CountAsync(u => u.IsDeleted == false && u.CreatedDate >= oneWeekAgo);

                var activePercentage = totalActiveUsers == 0 ? 0 :
                    Math.Round((double)activeUsersLastWeek / totalActiveUsers * 100, 2);

                // 5. AI Prediction Counts and Weekly %
                var groupedPrediction = await context.UserAIResults
                    .GroupBy(p => p.Prediction)
                    .Select(g => new
                    {
                        Result = g.Key,
                        Count = g.Count(),
                        WeeklyCount = g.Count(p => p.CreatedAt >= oneWeekAgo)
                    })
                    .ToListAsync();

                var predictionStats = groupedPrediction
                    .Select(g => new
                    {
                        Result = g.Result.ToString(),
                        g.Count,
                        g.WeeklyCount,
                        WeeklyPercentage = g.Count == 0 ? 0 :
                            Math.Round((double)g.WeeklyCount / g.Count * 100, 2)
                    })
                    .ToList();

                // Final response
                response.Result = new
                {
                    AllDoctors = allDoctors,
                    RandomDoctorsFullData = randomDoctorsFull,
                    RandomArticles = randomArticles,
                    ActiveUserCount = totalActiveUsers,
                    ActiveUsersLastWeekPercentage = activePercentage,
                    PredictionStats = predictionStats
                };

                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.Result = string.Empty;
                response.Errors.Add(ex.Message);
                return StatusCode((int)response.StatusCode, response);
            }
        }


    }
}
