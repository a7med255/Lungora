using System.ComponentModel.DataAnnotations;

namespace Lungora.Dtos.DoctorsDtos
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WhatsAppLink { get; set; }
        public string CategoryName { get; set; }
        public string ImageDoctor { get; set; }
        public string TimeAvailable { get; set; }
    }
}
