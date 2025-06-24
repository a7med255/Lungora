using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress, MaxLength(255)]
        public string Email { get; set; }
    }
}
