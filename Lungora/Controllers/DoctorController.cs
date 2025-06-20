using Lungora.Bl.Interfaces;
using Lungora.Bl.Repositories;
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
        private readonly ICategory ClsCategories;
        private readonly IWorkingHour ClsWorkingHours;
        private readonly IImageService imageService;
        private readonly API_Resonse response;
        public DoctorController(IDoctor doctor, IImageService imageService, ICategory category, IWorkingHour WorkingHour)
        {
            ClsDoctors = doctor;
            response = new API_Resonse();
            this.imageService = imageService;
            ClsCategories = category;
            ClsWorkingHours = WorkingHour;
        }

        [HttpGet("GetAllDoctors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDoctors()
        {
            try
            {
                var Doctors = await ClsDoctors.GetAll();
                response.Result = new { Doctors = Doctors };
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Result = new { Message = ex.Message };
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }
        }

        [HttpGet("GetAllDoctorsWithMobile")]
        public async Task<IActionResult> GetAllDoctorsWithMobile(double? Latitude,double? Longitude,int? distance)
        {
            var Doctors = await ClsDoctors.GetAllAsync(Latitude, Longitude,distance);
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
                var workingHours = await ClsWorkingHours.GetAllByDoctorIdAsync(Id);

                if (Doctor is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Doctor does not exist.");
                    return NotFound(response);
                }
                response.Result =new { Doctor = Doctor , WorkingHours= workingHours };
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctor([FromForm] DoctorCreateDTO DoctorDTO)
        {
            // Check if the Name already exists 
            var existsName = await ClsDoctors.GetSingleAsync(x => x.Name.ToLower() == DoctorDTO.Name.ToLower());

            if (existsName is not null)
                ModelState.AddModelError("", "Doctor already exists!");

            var userId = User.FindFirstValue("FullName");

            if (userId == null)
                return Unauthorized("User ID not found");


            if (ModelState.IsValid)
            {
                string imageUrl = null;
                if (DoctorDTO.ImageDoctor != null && DoctorDTO.ImageDoctor.Length > 0)
                {
                    imageUrl = await imageService.UploadOneImageAsync(DoctorDTO.ImageDoctor, "Doctors");
                }
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
                    ImageDoctor = imageUrl,
                    Latitude = DoctorDTO.Latitude,
                    Longitude = DoctorDTO.Longitude,
                    CategoryId = DoctorDTO.CategoryId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                    Category = await ClsCategories.GetSingleAsync(a=>a.Id==DoctorDTO.CategoryId)
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
        [HttpPut("EditDoctor/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditDoctor(int Id, DoctorUpdateDTO DoctorUpdateDTO)
        {

            var Name = User.FindFirstValue("FullName");

            if (Name == null)
                return Unauthorized("User ID not found");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentDoctor = await ClsDoctors.GetSingleAsync(x => x.Id == Id);

                    if (currentDoctor is null)
                    {
                        response.Result = string.Empty;
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.IsSuccess = false;
                        response.Errors.Add("Doctor not found!");
                        return NotFound(response);
                    }
                    string imageUrl = null;
                    if (DoctorUpdateDTO.ImageDoctor != null && DoctorUpdateDTO.ImageDoctor.Length > 0)
                    {
                        imageUrl = await imageService.UploadOneImageAsync(DoctorUpdateDTO.ImageDoctor, "Doctors");
                    }
                    else
                    {
                        imageUrl = currentDoctor.ImageDoctor;
                    }

                    currentDoctor.Name = DoctorUpdateDTO.Name;
                    currentDoctor.NumOfPatients = DoctorUpdateDTO.NumOfPatients;
                    currentDoctor.About = DoctorUpdateDTO.About;
                    currentDoctor.EmailDoctor = DoctorUpdateDTO.EmailDoctor;
                    currentDoctor.Phone = DoctorUpdateDTO.Phone;
                    currentDoctor.Teliphone = DoctorUpdateDTO.Teliphone;
                    currentDoctor.ExperianceYears = DoctorUpdateDTO.ExperianceYears;
                    currentDoctor.Location = DoctorUpdateDTO.Location;
                    currentDoctor.LocationLink = DoctorUpdateDTO.LocationLink;
                    currentDoctor.WhatsAppLink = DoctorUpdateDTO.WhatsAppLink;
                    currentDoctor.ImageDoctor = imageUrl;
                    currentDoctor.Latitude = DoctorUpdateDTO.Latitude;
                    currentDoctor.Longitude = DoctorUpdateDTO.Longitude;
                    currentDoctor.CategoryId = DoctorUpdateDTO.CategoryId;
                    currentDoctor.CreatedAt = currentDoctor.CreatedAt;
                    currentDoctor.CreatedBy = currentDoctor.CreatedBy;
                    currentDoctor.UpdatedAt = DateTime.Now;
                    currentDoctor.UpdatedBy = Name;
                    currentDoctor.Category = await ClsCategories.GetSingleAsync(a => a.Id == DoctorUpdateDTO.CategoryId);

                    await ClsDoctors.UpdateAsync(Id, currentDoctor);

                    response.Result = currentDoctor;
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

            response.Result = string.Empty;
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        [HttpDelete("RemoveDoctor/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveDoctor(int Id)
        {
            try
            {


                await ClsDoctors.RemoveAsync(a => a.Id == Id);

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
