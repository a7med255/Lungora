using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.ArticleDtos
{
    public class ArticleDetailDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? CoverImage { get; set; }
    }
}
