using Lungora.Bl.Interfaces;
using Lungora.Dtos.CategoryDtos;
using Lungora.Dtos.DoctorsDtos;
using Lungora.Models;
using Microsoft.EntityFrameworkCore;

namespace Lungora.Bl.Repositories
{
    public class ClsDoctors : Repository<Doctor>,IDoctor
    {
        private readonly LungoraContext context;
        public ClsDoctors(LungoraContext context):base(context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<DoctorDto>> GetAllAsync()
        {
            try
            {
                var doctors = await context.TbDoctors
                    .Include(d => d.Category)
                    .Include(d => d.WorkingHours) 
                    .ToListAsync();

                return doctors.Select(c => new DoctorDto
                {
                    Id = c.Id,
                    CategoryName = c.Category.CategoryName,
                    Name = c.Name,
                    ImageDoctor = c.ImageDoctor,
                    WhatsAppLink = c.WhatsAppLink,
                    TimeAvailable = GetNextAvailableTime(c.WorkingHours)
                }).ToList();
            }
            catch
            {
                return new List<DoctorDto>(); 
            }
        }
        public async Task<DoctorDetailsDto> GetByIdAsync(int id)
        {
            try
            {
                var doctors = await context.TbDoctors
                    .Include(d => d.Category).Where(d => d.Id == id)
                    .FirstOrDefaultAsync();

                if (doctors == null)
                    return null;

                return new DoctorDetailsDto
                {
                    Name = doctors.Name,
                    NumOfPatients = doctors.NumOfPatients,
                    About = doctors.About,
                    EmailDoctor = doctors.EmailDoctor,
                    Phone = doctors.Phone,
                    Teliphone = doctors.Teliphone,
                    ExperianceYears = doctors.ExperianceYears,
                    Location = doctors.Location,
                    LocationLink = doctors.LocationLink,
                    WhatsAppLink = doctors.WhatsAppLink,
                    ImageDoctor = doctors.ImageDoctor,
                    Latitude = doctors.Latitude,
                    Longitude = doctors.Longitude,
                    CategoryName = doctors.Category.CategoryName
                };
            }
            catch
            {
                return null;
            }
        }





        private string GetNextAvailableTime(IEnumerable<WorkingHour> workingHours)
        {
            var now = DateTime.Now;
            var today = now.DayOfWeek.ToString();
            var currentTime = now.TimeOfDay;

            var todayAvailable = workingHours
                .Where(a => a.DayOfWeek == today && a.StartTime > currentTime)
                .OrderBy(a => a.StartTime)
                .Select(a => "available from" + a.StartTime + " - " + a.EndTime)
                .FirstOrDefault();

            if (todayAvailable != null)
                return todayAvailable;

            var tomorrow = now.AddDays(1).DayOfWeek.ToString();
            var nextAvailable = workingHours
                .Where(a => a.DayOfWeek == tomorrow)
                .OrderBy(a => a.StartTime)
                .Select(a => "available from"+ a.StartTime + " - " + a.EndTime)
                .FirstOrDefault();

            return nextAvailable ?? "Not There Time Avilable";
        }

    }
}
