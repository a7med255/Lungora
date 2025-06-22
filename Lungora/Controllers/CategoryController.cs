using Lungora.Bl.Interfaces;
using Lungora.Dtos.CategoryDtos;
using Lungora.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        private readonly API_Resonse response;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;

        public CategoryController( IUnitOfWork unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            this.unitOfWork = unitOfWork;
            response = new API_Resonse();
            userManager = _userManager;
        }

        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await unitOfWork.ClsCategories.GetAllAsync();
            if (categories == null)
            {
                response.Result =string.Empty;
                response.StatusCode = HttpStatusCode.NotFound;
                response.IsSuccess = false;
                return NotFound(response);
            }
            response.Result =new {Category= categories };   
            response.StatusCode = HttpStatusCode.OK;
            response.IsSuccess = true;
            return Ok(response);
        }
        [HttpGet("GetCategoryById/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCategoryById(int Id)
        {
            try
            {
                var category = await unitOfWork.ClsCategories.GetSingleAsync(x => x.Id == Id);
                if (category is null)
                {

                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Category does not exist.");
                    return NotFound(response);
                }
                response.Result = category;
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
        [HttpGet("GetCategoryByIdByMobile/{CategoryId}")]
        public async Task<IActionResult> GetCategoryByIdByMobile(int CategoryId)
        {
            try
            {
                var category = await unitOfWork.ClsCategories.GetById(CategoryId);

                if (category.CategoryName is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Category does not exist.");
                    return NotFound(response);
                }
                response.Result = category;
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
        
        [HttpPost("CreateCategory")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryCreateDTO categoryDTO)
        {
            // Check if the Name already exists 
            var existsName = await unitOfWork.ClsCategories.GetSingleAsync(x => x.CategoryName.ToLower() == categoryDTO.CategoryName.ToLower());

            if (existsName is not null)
                ModelState.AddModelError("", "Category already exists!");

            var userId = User.FindFirstValue("FullName");

            if (userId == null)
                return Unauthorized("User ID not found");


            if (ModelState.IsValid)
            {
                Category category = new Category
                {
                    CategoryName = categoryDTO.CategoryName,
                    CreatedAt= DateTime.Now,
                    CreatedBy=userId,
                };

                var model= await unitOfWork.ClsCategories.AddAsync(category);
                await unitOfWork.SaveChangesAsync();

                response.Result=model;
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
        [HttpPut("EditCategory/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCategory(int Id, CategoryUpdateDTO categoryUpdateDTO)
        {
            // Check if the new Name already exists 
            if (categoryUpdateDTO.CategoryName is not null)
            {
                var existsName = await unitOfWork.ClsCategories
                    .GetSingleAsync(x => x.CategoryName.ToLower() == categoryUpdateDTO.CategoryName.ToLower());

                if (existsName is not null && existsName.Id != Id)
                    ModelState.AddModelError("", "Category already exists!");
            }
            var userId = User.FindFirstValue("FullName");

            if (userId == null)
                return Unauthorized("User ID not found");
            if (ModelState.IsValid)
            {
                try
                {
                    var currentCategory = await unitOfWork.ClsCategories.GetSingleAsync(x => x.Id == Id);

                    if (currentCategory is null)
                    {
                        response.Result = string.Empty;
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.IsSuccess = false;
                        response.Errors.Add("Category not found!");
                        return NotFound(response);
                    }

                    Category category = new Category
                    {
                        CategoryName = categoryUpdateDTO.CategoryName,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };
                    await unitOfWork.ClsCategories.UpdateAsync(Id, category);
                    await unitOfWork.SaveChangesAsync();

                    response.Result = category;
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

        [HttpDelete("RemoveCategory/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveCategory(int Id)
        {
            try
            {
                await unitOfWork.ClsCategories.RemoveAsync(a=>a.Id==Id);
                await unitOfWork.SaveChangesAsync();

                response.Result = new { message="Removed Sucssfully"};
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
