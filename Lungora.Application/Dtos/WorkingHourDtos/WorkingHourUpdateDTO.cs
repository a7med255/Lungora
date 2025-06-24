using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.WorkingHourDtos
{
    public class WorkingHourUpdateDTO
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }

    }
}
