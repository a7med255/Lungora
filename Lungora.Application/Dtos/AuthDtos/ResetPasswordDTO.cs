using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class ResetPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(4)]
        public string Code { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 9, ErrorMessage = "Password must be at least 9 characters long.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
