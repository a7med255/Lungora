using Lungora.Bl.Interfaces;
using Lungora.Models;
using Lungora.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System.Net;
using System;
using Microsoft.AspNetCore.Authorization;
using Lungora.Enum;
using Lungora.Bl.Repositories;
using System.Security.Claims;
using Lungora.Dtos.Model_AIDtos;

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelAIController : ControllerBase
    {
        private readonly IModelService _modelService;
        private readonly IImageService imageService;
        private readonly API_Resonse response;
        public ModelAIController(IModelService modelService, IImageService imageService)
        {
            _modelService = modelService;
            response = new API_Resonse();
            this.imageService = imageService;
        }
        /// <summary>
        /// Upload image to AI model
        /// </summary>  
        /// <param name="image"></param>
        /// <returns></returns>

        [HttpPost("AI_Model")]
        [Authorize]
        public async Task<IActionResult> UploadFileMultipart(IFormFile image)
        {
            try
            {

                var modelResponse = await _modelService.SendFileToModelAsync(image);

                if (modelResponse.Response == false)
                {
                    response.Result = new { predicted = string.Empty, message = modelResponse.predicted_label };
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    return Ok(response);
                }

                if (modelResponse.is_success)
                {
                    if (modelResponse.is_upload)
                    {
                       var imageUrl= await imageService.UploadOneImageAsync(image, modelResponse.predicted_label);
                        var AI_Result = new UserAIResult();
                        AI_Result.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        AI_Result.ImagePath = imageUrl;
                        AI_Result.CreatedAt = DateTime.UtcNow;
                        if (modelResponse.predicted_label=="covid")
                            AI_Result.Prediction = PredictionModel.Covid;
                        else if (modelResponse.predicted_label == "pneumonia")
                            AI_Result.Prediction = PredictionModel.Pneumonia;
                        else
                            AI_Result.Prediction = PredictionModel.Normal;
                        if (modelResponse.certain)
                        {
                            response.Result = new { predicted = modelResponse.predicted_label, message = $"Clear signs of: {modelResponse.predicted_label.ToUpper()} were detected. Please consult a doctor for appropriate follow-up." };
                            response.StatusCode = HttpStatusCode.OK;
                            response.IsSuccess = true;
                            AI_Result.Status = PredictionConfidence.High;
                            await _modelService.AddAsync(AI_Result);
                            return Ok(response);
                        }
                        if (modelResponse.check_status)
                        {
                            response.Result = new { predicted = modelResponse.predicted_label, message = "We recommend consulting a healthcare professional for further evaluation." };
                            response.StatusCode = HttpStatusCode.OK;
                            response.IsSuccess = true;
                            AI_Result.Status = PredictionConfidence.Medium;
                            await _modelService.AddAsync(AI_Result);
                            return Ok(response);
                        }
                        else
                        {
                            response.Result = new { predicted = modelResponse.predicted_label, message = "The result is unclear. Please consider retaking the scan or consulting a doctor." };
                            response.StatusCode = HttpStatusCode.OK;
                            response.IsSuccess = true;
                            AI_Result.Status = PredictionConfidence.Low;
                            await _modelService.AddAsync(AI_Result);
                            return Ok(response);
                        }
                        
                    }
                    else
                    {
                        response.Result = new { predicted = string.Empty, message = "Image does not chest x-ray." };
                        response.StatusCode = HttpStatusCode.OK;
                        response.IsSuccess = true;
                        return Ok(response);
                    }
                }
                else
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors.Add("Model does not Work.");
                    return BadRequest(response);
                }


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
        [HttpGet("GetAllHistories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllHistories()
        {
            try
            {
                var Histories = await _modelService.GetAllAsync();
                response.Result = new { History = Histories };
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
        [HttpGet("GetHistoryById/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistoryById(int Id)
        {
            try
            {
                var History = await _modelService.GetSingleAsync(x => x.Id == Id);
                if (History is null)
                {
                    response.Result = string.Empty;
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Errors.Add("Hestory does not exist.");
                    return NotFound(response);
                }
                var HistoryDto = new HistoryDto()
                {
                    Id = History.Id,
                    UserId = History.User.Name,
                    ImagePath = History.ImagePath,
                    CreatedAt = History.CreatedAt,
                    Prediction = History.Prediction.ToString(),
                    IsSave = History.IsSave,
                    Status = History.Status.ToString()
                };

                response.Result = HistoryDto;
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


        [HttpDelete("RemoveHistory/{Id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveHistory(int Id)
        {
            try
            {


                await _modelService.RemoveAsync(a => a.Id == Id);

                response.Result = new { message = "Removed it Sucssfully" };
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
