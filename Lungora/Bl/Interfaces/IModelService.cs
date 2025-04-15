using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IModelService
    {
        Task<ModelResponse> SendFileToModelAsync(IFormFile file);
    }
}
