using Azure;
using Lungora.Bl.Interfaces;
using Lungora.Bl.Repositories;
using Lungora.Dtos.ArticleDtos;
using Lungora.Models;
using Lungora.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly API_Resonse response;
        public ArticleController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            response = new API_Resonse();
        }
        [HttpGet("GetAllArticles")]
        [Authorize]
        public async Task<IActionResult> GetAllArticles()
        {
            try
            {
                var Articles = await unitOfWork.ClsArticles.GetAllAsync();
                response.Result = new { Article = Articles };
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
        [HttpGet("GetArticleById/{Id}")]
        public async Task<IActionResult> GetArticleById(int Id)
        {
            try
            {
                var Article = await unitOfWork.ClsArticles.GetSingleAsync(x => x.Id == Id);
                if (Article is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Article does not exist.");
                    return NotFound(response);
                }
                response.Result = Article;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Result =new{message=ex.Message };
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }
        [HttpGet("GetArticlesByCategoryId/{CategoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetArticlesByCategoryId(int CategoryId)
        {
            try
            {
                var articles = await unitOfWork.ClsArticles.GetByCategoryId(CategoryId);
                var NumerOfArticles = articles.Count();

                response.Result = new{articles,NumerOfArticles};
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
        [HttpGet("GetArticleByIdWithMobile/{Id}")]
        public async Task<IActionResult> GetArticleByCategoryIdWithMobile(int Id)
        {
            try
            {
                var Article = await unitOfWork.ClsArticles.GetById(Id);
                if (Article is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Article does not exist.");
                    return NotFound(response);
                }
                response.Result = Article;
                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Result = new { message = ex.Message };
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }
        [HttpPost("CreateArticle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateArticle([FromForm]ArticleCreateDTO ArticleDTO)
        {
            // Check if the Title already exists 
            var existsTitle = await unitOfWork.ClsArticles.GetSingleAsync(x => x.Title.ToLower() == ArticleDTO.Title.ToLower());

            if (existsTitle is not null)
                ModelState.AddModelError("", "Article already exists!");

            var userId = User.FindFirstValue("FullName");

            if (userId == null)
                return Unauthorized("User ID not found");


            if (ModelState.IsValid)
            {
                string imageUrl = null;
                if (ArticleDTO.CoverImage != null && ArticleDTO.CoverImage.Length > 0)
                {
                    imageUrl = await unitOfWork.IImageService.UploadOneImageAsync(ArticleDTO.CoverImage, "Articles");
                }
                Article Article = new Article
                {
                    Title = ArticleDTO.Title,
                    Description = ArticleDTO.Description,
                    Content = ArticleDTO.Content,
                    CategoryId = ArticleDTO.CategoryId,
                    CoverImage= imageUrl,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId,
                };

                var model = await unitOfWork.ClsArticles.AddAsync(Article);
                await unitOfWork.SaveChangesAsync();

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
        [HttpPut("EditArticle/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditArticle(int Id, ArticleUpdateDTO ArticleUpdateDTO)
        {
            // Check if the new Name already exists 
            if (ArticleUpdateDTO.Title is not null)
            {
                var existsName = await unitOfWork.ClsArticles
                    .GetSingleAsync(x => x.Title.ToLower() == ArticleUpdateDTO.Title.ToLower());

                if (existsName is not null && existsName.Id != Id)
                    ModelState.AddModelError("", "Article already exists!");
            }
            var Name = User.FindFirstValue("FullName");


            if (Name == null)
                return Unauthorized("User ID not found");



            if (ModelState.IsValid)
            {
                try
                {
                    var currentArticle = await unitOfWork.ClsArticles.GetSingleAsync(x => x.Id == Id);

                    if (currentArticle is null)
                    {
                        response.Result = string.Empty;
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.IsSuccess = false;
                        response.Errors.Add("Article not found!");
                        return NotFound(response);
                    }
                    string imageUrl = null;
                    if (ArticleUpdateDTO.CoverImage != null && ArticleUpdateDTO.CoverImage.Length > 0)
                    {
                        imageUrl = await unitOfWork.IImageService.UploadOneImageAsync(ArticleUpdateDTO.CoverImage, "Articles");
                    }
                    else
                    {
                        imageUrl = currentArticle.CoverImage;
                    }
                        Article Article = new Article
                        {
                            Title = ArticleUpdateDTO.Title,
                            Content = ArticleUpdateDTO.Content,
                            Description = ArticleUpdateDTO.Description,
                            CoverImage = imageUrl,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = Name
                        };
                    await unitOfWork.ClsArticles.UpdateAsync(Id, Article);
                    await unitOfWork.SaveChangesAsync();

                    response.Result = Article;
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

        [HttpDelete("RemoveArticle/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveArticle(int Id)
        {
            try
            {
                await unitOfWork.ClsArticles.RemoveAsync(a => a.Id == Id);
                await unitOfWork.SaveChangesAsync();

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
