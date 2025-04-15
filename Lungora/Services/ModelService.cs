using Lungora.Bl;
using Lungora.Bl.Interfaces;
using Lungora.Bl.Repositories;
using Lungora.Dtos.CategoryDtos;
using Lungora.Dtos.Model_AIDtos;
using Lungora.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Lungora.Services
{
    public class ModelService : Repository<UserAIResult> ,IModelService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl = "https://lakes-warriors-received-easy.trycloudflare.com/predict";
        private readonly LungoraContext context;

        public ModelService(IHttpClientFactory httpClientFactory ,LungoraContext context) : base(context) // Assuming you have a context to pass here, replace null with the actual context
        {
            _httpClientFactory = httpClientFactory;
            this.context = context;
        }
        public async Task<IEnumerable<HistoryDto>> GetAllAsync()
        {
            try
            {
                var histories = await context.UserAIResults.Include(a=>a.User).ToListAsync();

                return histories.Select(c => new HistoryDto
                {
                    Id = c.Id,
                    UserId = c.User.Name,
                    ImagePath = c.ImagePath,
                    CreatedAt = c.CreatedAt,
                    Prediction = c.Prediction.ToString(),
                    IsSave = c.IsSave,
                    Status = c.Status.ToString()
                }).ToList();
            }
            catch
            {
                return new List<HistoryDto>();
            }
        }
        public async Task<ModelResponse> SendFileToModelAsync(IFormFile image)
        {
            try {
                const long MaxFileSize = 10 * 1024 * 1024;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };

                var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (image == null || image.Length == 0)
                {
                    return new ModelResponse
                    {
                        Response = false,
                        predicted_label = "Image is required."
                    };
                }

                if (image.Length > MaxFileSize)
                {
                    return new ModelResponse
                    {
                        Response = false,
                        predicted_label = "Image size must be 10MB or less."
                    };
                }

                if (!allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLowerInvariant()))
                {
                    return new ModelResponse
                    {
                        Response = false,
                        predicted_label = "Unsupported file extension."
                    };
                }

                var client = _httpClientFactory.CreateClient("AIService");
                using var formContent = new MultipartFormDataContent();

                using var stream = image.OpenReadStream();
                var imageContent = new StreamContent(stream);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                formContent.Add(imageContent, name: "image", fileName: image.FileName);

                var response = await client.PostAsync(_apiUrl, formContent);


                var responseContent = await response.Content.ReadAsStringAsync();

                var predictionResult = System.Text.Json.JsonSerializer.Deserialize<ModelResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return predictionResult ?? new ModelResponse
                {
                    Response = false,
                    predicted_label = "Empty response from AI service."
                };
            }
            catch (Exception ex)
            {
                return new ModelResponse
                {
                    Response = false,
                    predicted_label = $"Unexpected error: {ex.Message}"
                };
            }
        }


    }
}
