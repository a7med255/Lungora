using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Lungora.Enum;

namespace Lungora.Models
{
    public class UserAIResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public string ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public PredictionModel Prediction { get; set; }
        public bool IsSave { get; set; } = false;

        public PredictionConfidence Status { get; set; }  


        // Navigation property
        public virtual ApplicationUser User { get; set; }

    }
}
