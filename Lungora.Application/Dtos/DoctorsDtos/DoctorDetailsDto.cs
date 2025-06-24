using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.DoctorsDtos
{
    public class DoctorDetailsDto
    {
        public string Name { get; set; }
        public int NumOfPatients { get; set; }
        public string About { get; set; }
        public string EmailDoctor { get; set; }
        public string Phone { get; set; }
        public string Teliphone { get; set; }
        public int ExperianceYears { get; set; }
        public string Location { get; set; }
        public string LocationLink { get; set; }
        public string WhatsAppLink { get; set; }
        public string ImageDoctor { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CategoryName { get; set; }

    }
}
