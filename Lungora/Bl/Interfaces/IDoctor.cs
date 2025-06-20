using Lungora.Dtos.DoctorsDtos;
using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IDoctor : IRepository<Doctor>
    {
        Task<List<DoctorDto>> GetAllAsync(double? Latitude, double? Longitude, int? distance);
        Task<IEnumerable<Doctor>> GetAll();
        Task<DoctorDetailsDto> GetByIdAsync(int id);
        Task<Doctor> UpdateAsync(int Id, Doctor doctor);
    }
}
