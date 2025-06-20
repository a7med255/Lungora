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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkingHourDoctorId(int DoctorId)
        {
            try
            {   
                var WorkingHour = await ClsWorkingHours.GetAllByDoctorIdAsync(DoctorId);

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

        [HttpGet("GetWorkingHourById/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetWorkingHourById(int Id)
        {
            try
            {
                var WorkingHour = await ClsWorkingHours.GetSingleAsync(a=>a.Id==Id);
                if (WorkingHour == null)
                {
                    response.Result = string.Empty;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
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

            if (!string.IsNullOrWhiteSpace(WorkingHourDTO.DayOfWeek))
            {
                var exists = await ClsWorkingHours.GetSingleAsync(
                    x => x.DayOfWeek.Equals(WorkingHourDTO.DayOfWeek, StringComparison.OrdinalIgnoreCase)
                );

                if (exists is not null)
                {
                    response.Result = string.Empty;
                    response.StatusCode = HttpStatusCode.Conflict;
                    response.IsSuccess = false;
                    response.Errors.Add("A working hour with the same DayOfWeek already exists.");
                    return Conflict(response);
                }
            }
            if (ModelState.IsValid)
            {
                WorkingHour WorkingHour = new WorkingHour
                {
                    DayOfWeek = WorkingHourDTO.DayOfWeek,
                    StartTime = WorkingHourDTO.StartTime,
                    EndTime = WorkingHourDTO.EndTime,
                    DoctorId = WorkingHourDTO.DoctorId,
                };
                if (!WorkingHour.IsValidTimeRange())
                {
                    response.Result = string.Empty;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.Errors.Add("End time must be greater than start time.");
                    return BadRequest(response);
                }
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
        [HttpPut("EditWorkingHour/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditWorkingHour(int id, WorkingHourUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                response.Result = string.Empty;
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values.SelectMany(v => v.Errors)
                                                   .Select(e => e.ErrorMessage)
                                                   .ToList();
                return BadRequest(response);
            }

            try
            {
                // Check if working hour with same DayOfWeek exists (excluding the current one)
                if (!string.IsNullOrWhiteSpace(dto.DayOfWeek))
                {
                    var exists = await ClsWorkingHours.GetSingleAsync(
                        x => x.DayOfWeek.Equals(dto.DayOfWeek, StringComparison.OrdinalIgnoreCase) && x.Id != id
                    );

                    if (exists is not null)
                    {
                        response.Result = string.Empty;
                        response.StatusCode = HttpStatusCode.Conflict;
                        response.IsSuccess = false;
                        response.Errors.Add("A working hour with the same DayOfWeek already exists.");
                        return Conflict(response);
                    }
                }

                var current = await ClsWorkingHours.GetSingleAsync(x => x.Id == id);

                if (current is null)
                {
                    response.Result = string.Empty;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSuccess = false;
                    response.Errors.Add("Working hour not found.");
                    return NotFound(response);
                }

                // Update fields
                current.DayOfWeek = dto.DayOfWeek;
                current.StartTime = dto.StartTime;
                current.EndTime = dto.EndTime;
                if(!current.IsValidTimeRange())
                {
                    response.Result = string.Empty;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    response.Errors.Add("End time must be greater than start time.");
                    return BadRequest(response);
                }
                await ClsWorkingHours.UpdateAsync(id, current);

                response.Result = current;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception here if logging is implemented
                response.Result = string.Empty;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.Errors.Add("An unexpected error occurred: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


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
