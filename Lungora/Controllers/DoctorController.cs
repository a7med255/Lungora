using Lungora.Bl.Interfaces;
using Lungora.Dtos.DoctorsDtos;
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
    public class DoctorController : ControllerBase
    {
        private readonly IDoctor ClsDoctors;
        private readonly API_Resonse response;
        public DoctorController(IDoctor doctor)
        {
            ClsDoctors = doctor;
            response = new API_Resonse();
        }
        [HttpGet("GetAllDoctorsWithMobile")]
        public async Task<IActionResult> GetAllDoctorsWithMobile()
        {
            var Doctors = await ClsDoctors.GetAllAsync();
            if (Doctors == null)
            {
                response.Result = string.Empty;
                response.StatusCode = HttpStatusCode.NotFound;
                response.IsSuccess = false;
                return NotFound(response);
            }
            response.Result = new { Doctor = Doctors };
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }
        [HttpGet("GetDoctorById/{Id}")]
        public async Task<IActionResult> GetDoctorById(int Id)
        {
            try
            {
                var Doctor = await ClsDoctors.GetByIdAsync(Id);

                if (Doctor is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Doctor does not exist.");
                    return NotFound(response);
                }
                response.Result = Doctor;
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
        [HttpPost("CreateDoctor")]
        [Authorize]
        public async Task<IActionResult> CreateDoctor(DoctorCreateDTO DoctorDTO)
        {
            // Check if the Name already exists 
            var existsName = await ClsDoctors.GetSingleAsync(x => x.Name.ToLower() == DoctorDTO.Name.ToLower());

            if (existsName is not null)
                ModelState.AddModelError("", "Doctor already exists!");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized("User ID not found");


            if (ModelState.IsValid)
            {
                Doctor Doctor = new Doctor
                {
                    Name = DoctorDTO.Name,
                    NumOfPatients = DoctorDTO.NumOfPatients,
                    About = DoctorDTO.About,
                    EmailDoctor = DoctorDTO.EmailDoctor,
                    Phone = DoctorDTO.Phone,
                    Teliphone = DoctorDTO.Teliphone,
                    ExperianceYears = DoctorDTO.ExperianceYears,
                    Location = DoctorDTO.Location,
                    LocationLink = DoctorDTO.LocationLink,
                    WhatsAppLink = DoctorDTO.WhatsAppLink,
                    ImageDoctor = DoctorDTO.ImageDoctor,
                    Latitude = DoctorDTO.Latitude,
                    Longitude = DoctorDTO.Longitude,
                    CategoryId = DoctorDTO.CategoryId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                };

                var model = await ClsDoctors.AddAsync(Doctor);

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
    }
}
