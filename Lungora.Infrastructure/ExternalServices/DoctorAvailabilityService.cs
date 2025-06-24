using Lungora.Bl.Interfaces;
using Lungora.Models;

namespace Lungora.Services
{
    public class DoctorAvailabilityService : IDoctorAvailabilityService
    {
        public string GetNextAvailableTime(IEnumerable<WorkingHour> workingHours)
        {
            var now = DateTime.Now;
            var today = now.DayOfWeek;
            var currentTime = now.TimeOfDay;

            var todayAvailable = workingHours
                .Where(a => a.DayOfWeek == today && a.StartTime > currentTime)
                .OrderBy(a => a.StartTime)
                .FirstOrDefault();

            if (todayAvailable != null)
            {
                return $"Available Today from {todayAvailable.StartTime:hh\\:mm} - {todayAvailable.EndTime:hh\\:mm}";
            }

            var nextAvailable = workingHours
                .Where(a =>
                    (int)a.DayOfWeek > (int)today ||
                    (int)a.DayOfWeek < (int)today)
                .OrderBy(a => ((int)a.DayOfWeek - (int)today + 7) % 7)
                .ThenBy(a => a.StartTime)
                .FirstOrDefault();

            if (nextAvailable != null)
            {
                var nextDayName = nextAvailable.DayOfWeek == ((DayOfWeek)(((int)today + 1) % 7))
                    ? "Tomorrow"
                    : nextAvailable.DayOfWeek.ToString();

                return $"Available {nextDayName} from {nextAvailable.StartTime:hh\\:mm} - {nextAvailable.EndTime:hh\\:mm}";
            }

            return "No available time";
        }
    }
}
