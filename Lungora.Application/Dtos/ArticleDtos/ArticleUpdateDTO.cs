using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.ArticleDtos
{
    public class ArticleUpdateDTO
    {
        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        public IFormFile ?CoverImage { get; set; }
    }
}
