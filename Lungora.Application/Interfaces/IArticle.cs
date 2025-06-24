using Lungora.Models;
using Lungora.Dtos.ArticleDtos;

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
