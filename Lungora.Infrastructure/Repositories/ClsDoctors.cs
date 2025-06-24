using Lungora.Bl.Interfaces;
using Lungora.Dtos.CategoryDtos;
using Lungora.Dtos.DoctorsDtos;
using Lungora.Helpers;
using Lungora.Models;
using Microsoft.EntityFrameworkCore;

namespace Lungora.Bl.Repositories
{
    public class ClsDoctors : Repository<Doctor>,IDoctor
    {
        private readonly LungoraContext context;
        private readonly IDoctorAvailabilityService _availabilityService;
        public ClsDoctors(LungoraContext context, IDoctorAvailabilityService availabilityService) : base(context)
        {
            this.context = context;
            _availabilityService = availabilityService;
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
                        calculatedDistance = GeoHelper.GetDistanceInKm(latitude.Value, longitude.Value, doctor.Latitude, doctor.Longitude);
                    }

                    doctorWithDistances.Add((doctor, calculatedDistance));
                }

                if (latitude.HasValue && longitude.HasValue)
                {
                    int effectiveDistance = distance ?? 40;

                    doctorWithDistances = doctorWithDistances
                        .Where(x => x.distance.HasValue && x.distance.Value <= effectiveDistance)
                        .OrderBy(x => x.distance)
                        .ToList();
                }

                if (doctorWithDistances.Count == 0)
                {

                    return doctors.Select(x => new DoctorDto
                    {
                        Id = x.Id,
                        CategoryName = x.Category?.CategoryName,
                        Name = x.Name,
                        ImageDoctor = x.ImageDoctor,
                        WhatsAppLink = x.WhatsAppLink,
                        TimeAvailable = _availabilityService.GetNextAvailableTime(x.WorkingHours),
                    }).ToList();
                }

                return doctorWithDistances.Select(x => new DoctorDto
                {
                    Id = x.doctor.Id,
                    CategoryName = x.doctor.Category?.CategoryName,
                    Name = x.doctor.Name,
                    ImageDoctor = x.doctor.ImageDoctor,
                    WhatsAppLink = x.doctor.WhatsAppLink,
                    TimeAvailable = _availabilityService.GetNextAvailableTime(x.doctor.WorkingHours),
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
                return UpdatedDoctor;
            }
            return null;
        }


        


    }
}
