using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lungora.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int TokenVersion { get; set; } = 1;
        [MaxLength(4)]
        public string? PasswordResetCode { get; set; }
        public DateTime? PasswordResetCodeExpiry { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
