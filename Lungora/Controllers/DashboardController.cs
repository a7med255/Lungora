using Lungora.Bl;
using Lungora.Bl.Interfaces;
using Lungora.Migrations;
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
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly API_Resonse response;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
            response = new API_Resonse();

        }

        [HttpGet("GetDashboardData")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var result = await _dashboardService.GetDashboardDataAsync();

                if (result == null)
                {
                    response.Result = string.Empty;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.Errors = new List<string> { "Dashboard data not found." };
                    return NotFound(response);
                }

                response.Result = result;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Result = ex.Message;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.Errors = new List<string> { "An unexpected error occurred: " + ex.Message };
                return StatusCode(500, response);
            }
        }
    }

}
