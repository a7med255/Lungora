using Lungora.Dtos.CategoryDtos;
using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface ICategory : IRepository<Category>
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryWithArticles> GetById(int id);
        Task<Category> UpdateAsync(int Id, Category article);
    }
}
