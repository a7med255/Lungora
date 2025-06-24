using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Lungora.Dtos.AuthDtos
{
    public class ChangePasswordDTO
    {
        [Required]
        [PasswordPropertyText]
        public string CurrentPassword { get; set; }

        [Required]
        [PasswordPropertyText]
        public string NewPassword { get; set; }
    }
}
