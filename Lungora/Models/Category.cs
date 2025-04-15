using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lungora.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
        [JsonIgnore]
        public ICollection<Article> Articles { get; set; }
        [JsonIgnore]
        public ICollection<Doctor> Doctors { get; set; }
    }
}
