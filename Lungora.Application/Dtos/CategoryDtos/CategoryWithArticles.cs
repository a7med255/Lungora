using Lungora.Dtos.ArticleDtos;

namespace Lungora.Dtos.CategoryDtos
{
    public class CategoryWithArticles
    {
        public string? CategoryName { get; set; }
        public List<ArticlesDto> Articles { get; set; }
    }
}
