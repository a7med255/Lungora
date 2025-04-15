using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lungora.Models
{
    public class WorkingHour
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [MaxLength(10)]
        public string DayOfWeek { get; set; } 
        [Required]
        public TimeSpan StartTime { get; set; } 
        [Required]
        public TimeSpan EndTime { get; set; }
        public bool IsValidTimeRange()
        {
            return EndTime > StartTime;
        }
        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }
    }
}
