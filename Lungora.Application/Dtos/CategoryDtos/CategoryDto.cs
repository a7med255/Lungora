using Lungora.Dtos.ArticleDtos;
using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.CategoryDtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public List<ArticlesDto> Articles{ get; set; }

    }
}
