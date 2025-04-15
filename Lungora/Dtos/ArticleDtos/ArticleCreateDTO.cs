using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.ArticleDtos
{
    public class ArticleCreateDTO
    {
        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public IFormFile CoverImage { get; set; }
    }
}
