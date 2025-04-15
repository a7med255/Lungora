using Azure;
using Lungora.Bl.Interfaces;
using Lungora.Bl.Repositories;
using Lungora.Dtos.WorkingHourDtos;
using Lungora.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkingHourController : ControllerBase
    {
        private readonly IWorkingHour ClsWorkingHours;
        private readonly API_Resonse response;
        public WorkingHourController(IWorkingHour workingHour)
        {
            ClsWorkingHours = workingHour;
            response = new API_Resonse();
        }

        //[HttpGet("GetAllWorkingHours")]
        //public async Task<IActionResult> GetAllWorkingHours()
        //{
        //    var WorkingHours = await ClsWorkingHours.GetAllAsync();
        //    if (WorkingHours == null)
        //    {
        //        response.Result = string.Empty;
        //        response.StatusCode = HttpStatusCode.NotFound;
        //        response.IsSuccess = false;
        //        return NotFound(response);
        //    }
        //    response.Result = new { WorkingHour = WorkingHours };
        //    response.StatusCode = HttpStatusCode.OK;
        //    response.IsSuccess = true;
        //    return Ok(response);
        //}
        [HttpGet("GetWorkingHourDoctorId/{DoctorId}")]
        public async Task<IActionResult> GetWorkingHourById(int DoctorId)
        {
            try
            {   
                var WorkingHour = await ClsWorkingHours.GetSingleAsync(x => x.DoctorId == DoctorId);

                
                if (WorkingHour is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("WorkingHour does not exist.");
                    return NotFound(response);
                }
                response.Result = WorkingHour;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Result = string.Empty;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }
        [HttpPost("CreateWorkingHour")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateWorkingHour(WorkingHourCreateDTO WorkingHourDTO)
        {


            if (ModelState.IsValid)
            {
                WorkingHour WorkingHour = new WorkingHour
                {
                    DayOfWeek = WorkingHourDTO.DayOfWeek,
                    StartTime = WorkingHourDTO.StartTime,
                    EndTime = WorkingHourDTO.EndTime,
                    DoctorId = WorkingHourDTO.DoctorId,
                };

                var model = await ClsWorkingHours.AddAsync(WorkingHour);

                response.Result = model;
                response.StatusCode = HttpStatusCode.Created;
                response.IsSuccess = true;
                return Ok(response);
            }

            response.Result = string.Empty;
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }
        //[HttpPut("EditWorkingHour/{Id}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> EditWorkingHour(int Id, WorkingHourUpdateDTO WorkingHourUpdateDTO)
        //{
        //    // Check if the new Name already exists 
        //    if (WorkingHourUpdateDTO.WorkingHourName is not null)
        //    {
        //        var existsName = await clsWorkingHours
        //            .GetSingleAsync(x => x.WorkingHourName.ToLower() == WorkingHourUpdateDTO.WorkingHourName.ToLower());

        //        if (existsName is not null && existsName.Id != Id)
        //            ModelState.AddModelError("", "WorkingHour already exists!");
        //    }
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (userId == null)
        //        return Unauthorized("User ID not found");
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var currentWorkingHour = await clsWorkingHours.GetSingleAsync(x => x.Id == Id);

        //            if (currentWorkingHour is null)
        //            {
        //                response.Result = string.Empty;
        //                response.StatusCode = HttpStatusCode.NotFound;
        //                response.IsSuccess = false;
        //                response.Errors.Add("WorkingHour not found!");
        //                return NotFound(response);
        //            }

        //            WorkingHour WorkingHour = new WorkingHour
        //            {
        //                WorkingHourName = WorkingHourUpdateDTO.WorkingHourName,
        //                UpdatedAt = DateTime.Now,
        //                UpdatedBy = userId
        //            };
        //            await clsWorkingHours.UpdateAsync(Id, WorkingHour);

        //            response.Result = WorkingHour;
        //            response.StatusCode = HttpStatusCode.OK;
        //            response.IsSuccess = true;
        //            return Ok(response);
        //        }
        //        catch (Exception ex)
        //        {
        //            response.Result = string.Empty;
        //            response.StatusCode = HttpStatusCode.NotFound;
        //            response.IsSuccess = false;
        //            response.Errors.Add(ex.Message);
        //            return NotFound(response);
        //        }
        //    }

        //    response.Result = string.Empty;
        //    response.IsSuccess = false;
        //    response.StatusCode = HttpStatusCode.BadRequest;
        //    response.Errors = ModelState.Values
        //        .SelectMany(v => v.Errors)
        //        .Select(e => e.ErrorMessage)
        //        .ToList();
        //    return BadRequest(response);
        //}

        [HttpDelete("RemoveWorkingHour/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveWorkingHour(int Id)
        {
            try
            {
                await ClsWorkingHours.RemoveAsync(a => a.Id == Id);
                response.Result = new { message = "Removed Sucssfully" };
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Result = string.Empty;
                response.StatusCode = HttpStatusCode.NotFound;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return NotFound(response);
            }
        }
    }
}
