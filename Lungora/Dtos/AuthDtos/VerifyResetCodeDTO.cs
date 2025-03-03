using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class VerifyResetCodeDTO
    {
        [Required]
        [EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        [Required]
        [MaxLength(4)]
        public string Code { get; set; }
    }
}
