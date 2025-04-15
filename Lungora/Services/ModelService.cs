using Lungora.Bl.Interfaces;
using Lungora.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Lungora.Services
{
    public class ModelService : IModelService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl = "https://fuel-worn-happiness-noted.trycloudflare.com/predict";

        public ModelService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ModelResponse> SendFileToModelAsync(IFormFile image)
        {
            try {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };
                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return null;
                var client = _httpClientFactory.CreateClient("AIService");
                using var formContent = new MultipartFormDataContent();

                using var stream = image.OpenReadStream();
                var imageContent = new StreamContent(stream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                formContent.Add(imageContent, name: "image", fileName: image.FileName);

                // إرسال الصورة إلى خدمة AI
                var response = await client.PostAsync(_apiUrl, formContent);


                var responseContent = await response.Content.ReadAsStringAsync();

                var predictionResult = System.Text.Json.JsonSerializer.Deserialize<ModelResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return predictionResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
