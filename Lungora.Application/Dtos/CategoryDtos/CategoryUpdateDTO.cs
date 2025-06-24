using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.CategoryDtos
{
    public class CategoryUpdateDTO
    {
        [Required, StringLength(255)]
        public string? CategoryName { get; set; }
    }
}
