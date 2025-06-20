using Lungora.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Lungora.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        public int NumOfPatients { get; set; }
        [Required]
        [MaxLength(500)]
        public string About { get; set; }

        [Required]
        [EmailAddress]
        public string EmailDoctor { get; set; }

        [Phone]
        [Required]
        [StringLength(11)]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "In Valid This Number")]
        public string Phone { get; set; }

        [Phone]
        [Required]
        [StringLength(12)]
        [RegularExpression(@"^(01[0-2,5]{1}[0-9]{8}|0[2-9]{1}[0-9]{7,8})$", ErrorMessage = "In Valid This  Number")]
        public string Teliphone { get; set; }
        public int ExperianceYears { get; set; }

        [Required]
        [MaxLength(100)]
        public string Location { get; set; }
        [Required]
        public string LocationLink { get; set; }

        [Required]
        public string WhatsAppLink { get; set; }
        [Required]
        public string ImageDoctor { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public List<WorkingHour> WorkingHours { get; set; } = new List<WorkingHour>();
    }
}
