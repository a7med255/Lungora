using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.WorkingHourDtos
{
    public class WorkingHourDetails
    {
        public int Id { get; set; }
        public string? DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DoctorId { get; set; }
    }
}
