using Lungora.Dtos.ArticleDtos;
using Lungora.Dtos.CategoryDtos;
using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IArticle: IRepository<Article>
    {
        Task<IEnumerable<Article>> GetAllAsync();
        Task<ArticleDetailDto> GetById(int id);
        Task<List<Article>> GetByCategoryId(int CategoryId);
        Task<Article> UpdateAsync(int Id, Article article);
    }
}
