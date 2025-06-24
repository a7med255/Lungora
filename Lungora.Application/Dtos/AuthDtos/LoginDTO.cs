using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Lungora.Dtos.AuthDtos
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress, MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [PasswordPropertyText, MaxLength(100)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
