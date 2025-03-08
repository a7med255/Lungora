using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class EditInfoDTO
    {
        [Required, MaxLength(255)]
        public string FullName { get; set; }
        [Required]
        public IFormFile ImageUser { get; set; }
    }
}
