using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.WorkingHourDtos
{
    public class WorkingHourCreateDTO
    {
        [Required]
        [MaxLength(10)]
        public string DayOfWeek { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required]
        public int DoctorId { get; set; }
    }
}
