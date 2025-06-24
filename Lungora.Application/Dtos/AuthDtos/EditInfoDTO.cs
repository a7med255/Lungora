using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.AuthDtos
{
    public class EditInfoDTO
    {
        [MaxLength(255)]
        public string? FullName { get; set; }
        public IFormFile? ImageUser { get; set; }
    }
}
