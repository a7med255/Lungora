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
        public async Task<IEnumerable<Doctor>> GetAll()
        {
            try
            {
                var doctors = await context.TbDoctors
                    .Include(d => d.Category)
                    .Include(d => d.WorkingHours)
                    .ToListAsync();
                return doctors;
            }
            catch
            {
                return new List<Doctor>();
            }
        }
        public async Task<List<DoctorDto>> GetAllAsync(double? latitude, double? longitude, int? distance)
        {
            try
            {
                var doctors = await context.TbDoctors
                    .Include(d => d.Category)
                    .Include(d => d.WorkingHours)
                    .ToListAsync();

                List<(Doctor doctor, double? distance)> doctorWithDistances = new();

                foreach (var doctor in doctors)
                {
                    double? calculatedDistance = null;

                    if (latitude.HasValue && longitude.HasValue)
                    {
                        calculatedDistance = GetDistanceInKm(latitude.Value, longitude.Value, doctor.Latitude, doctor.Longitude);
                    }

                    doctorWithDistances.Add((doctor, calculatedDistance));
                }

                if (latitude.HasValue && longitude.HasValue)
                {
                    int effectiveDistance = distance ?? 10;

                    doctorWithDistances = doctorWithDistances
                        .Where(x => x.distance.HasValue && x.distance.Value <= effectiveDistance)
                        .OrderBy(x => x.distance)
                        .ToList();
                }

                return doctorWithDistances.Select(x => new DoctorDto
                {
                    Id = x.doctor.Id,
                    CategoryName = x.doctor.Category?.CategoryName,
                    Name = x.doctor.Name,
                    ImageDoctor = x.doctor.ImageDoctor,
                    WhatsAppLink = x.doctor.WhatsAppLink,
                    TimeAvailable = GetNextAvailableTime(x.doctor.WorkingHours),
                }).ToList();
            }
            catch
            {
                return new List<DoctorDto>();
            }
        }




        private double GetDistanceInKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in KM

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
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


        public async Task<Doctor> UpdateAsync(int Id, Doctor Doctor)
        {
            var UpdatedDoctor = await GetSingleAsync(x => x.Id == Id);
            if (UpdatedDoctor is not null)
            {
                UpdatedDoctor.Name = Doctor.Name;
                UpdatedDoctor.NumOfPatients = Doctor.NumOfPatients;
                UpdatedDoctor.About = Doctor.About;
                UpdatedDoctor.EmailDoctor = Doctor.EmailDoctor;
                UpdatedDoctor.Phone = Doctor.Phone;
                UpdatedDoctor.Teliphone = Doctor.Teliphone;
                UpdatedDoctor.ExperianceYears = Doctor.ExperianceYears;
                UpdatedDoctor.Location = Doctor.Location;
                UpdatedDoctor.LocationLink = Doctor.LocationLink;
                UpdatedDoctor.WhatsAppLink = Doctor.WhatsAppLink;
                UpdatedDoctor.ImageDoctor = Doctor.ImageDoctor;
                UpdatedDoctor.Latitude = Doctor.Latitude;
                UpdatedDoctor.Longitude = Doctor.Longitude;
                UpdatedDoctor.UpdatedAt = DateTime.Now;
                UpdatedDoctor.UpdatedBy = Doctor.UpdatedBy;
                UpdatedDoctor.CategoryId = Doctor.CategoryId;

                context.Update(UpdatedDoctor).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await context.SaveChangesAsync();
                return UpdatedDoctor;
            }
            return null;
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
