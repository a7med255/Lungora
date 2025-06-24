using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IDoctorAvailabilityService
    {
        string GetNextAvailableTime(IEnumerable<WorkingHour> workingHours);
    }
}
