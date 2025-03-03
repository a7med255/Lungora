using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class AddRoleDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
