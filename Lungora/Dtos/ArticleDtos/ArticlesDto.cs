using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.ArticleDtos
{
    public class ArticlesDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
