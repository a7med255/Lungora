using Lungora.Dtos.Model_AIDtos;
using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IModelService:IRepository<UserAIResult>
    {
        Task<ModelResponse> SendFileToModelAsync(IFormFile file);
        Task<IEnumerable<HistoryDto>> GetAllAsync();
    }
}
