using Lungora.Dtos.DoctorsDtos;
using Lungora.Models;

namespace Lungora.Bl.Interfaces
{
    public interface IDoctor : IRepository<Doctor>
    {
        Task<IEnumerable<DoctorDto>> GetAllAsync();
        Task<DoctorDetailsDto> GetByIdAsync(int id);
    }
}
