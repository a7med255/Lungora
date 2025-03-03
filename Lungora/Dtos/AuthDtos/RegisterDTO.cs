using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Lungora.Dtos.AuthDtos
{
    public class RegisterDTO
    {

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255), EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 9, ErrorMessage = "Password must be at least 9 characters long.")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
