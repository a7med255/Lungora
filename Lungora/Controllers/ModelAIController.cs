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

namespace Lungora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelAIController : ControllerBase
    {
        private readonly IModelService _modelService;
        private readonly API_Resonse response;
        public ModelAIController(IModelService modelService)
        {
            _modelService = modelService;
            response = new API_Resonse();
        }
        /// <summary>
        /// Upload image to AI model
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>

        [HttpPost("AI_Model")]
        public async Task<IActionResult> UploadFileMultipart( IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return BadRequest("Error");

               
                var modelResponse = await _modelService.SendFileToModelAsync(image);

                if (modelResponse.is_success)
                {
                    if (modelResponse.is_upload)
                    {
                        response.Result = new{ predicted= modelResponse.predicted_label};
                        response.StatusCode = HttpStatusCode.OK;
                        response.IsSuccess = true;
                        return Ok(response);
                    }
                    if (modelResponse.check_status)
                    {
                        response.Result = new {message="Image Again"};
                        response.StatusCode = HttpStatusCode.OK;
                        response.IsSuccess = true;
                        return Ok(response);
                    }
                    else
                    {
                        response.Result = new { message = "Image Error" };
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
                    response.Errors.Add("Model does not exist.");
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

    }
}
