using Lungora.Dtos.WorkingHourDtos;
using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IWorkingHour: IRepository<WorkingHour>
    {
        Task<List<WorkingHourDetails>> GetAllByDoctorIdAsync(int doctorId);
        Task<WorkingHour> UpdateAsync(int Id, WorkingHour WorkingHour);
    }
}
