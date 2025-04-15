using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class EditUser
    {
        public string? FullName { get; set; }
        [MaxLength(255), EmailAddress]
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        [Required]
        public List<string> Roles { get; set; } = new();
        public IFormFile? ImageUser { get; set; }

    }
}
