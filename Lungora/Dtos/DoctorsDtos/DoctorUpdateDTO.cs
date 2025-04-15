using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.DoctorsDtos
{
    public class DoctorUpdateDTO
    {
        [Required]
        [MaxLength(255)]
        public string? Name { get; set; }
        [Required]
        public int NumOfPatients { get; set; }
        [Required]
        [MaxLength(500)]
        public string? About { get; set; }

        [Required]
        [EmailAddress]
        public string? EmailDoctor { get; set; }

        [Phone]
        [Required]
        [StringLength(11)]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "In Valid This Number")]
        public string? Phone { get; set; }

        [Phone]
        [Required]
        [StringLength(12)]
        [RegularExpression(@"^(01[0-2,5]{1}[0-9]{8}|0[2-9]{1}[0-9]{7,8})$", ErrorMessage = "In Valid This  Number")]
        public string? Teliphone { get; set; }
        public int ExperianceYears { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Location { get; set; }
        [Required]
        public string? LocationLink { get; set; }

        [Required]
        public string? WhatsAppLink { get; set; }

        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public IFormFile? ImageDoctor { get; set; }
    }
}
