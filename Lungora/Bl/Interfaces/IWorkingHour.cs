using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IWorkingHour: IRepository<WorkingHour>
    {
        Task<List<WorkingHour>> GetAllByDoctorIdAsync(int doctorId);
        Task<WorkingHour> UpdateAsync(int Id, WorkingHour WorkingHour);
    }
}
