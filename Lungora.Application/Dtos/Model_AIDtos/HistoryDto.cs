using Lungora.Enum;

namespace Lungora.Dtos.Model_AIDtos
{
    public class HistoryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Prediction { get; set; }
        public bool IsSave { get; set; } = false;
        public string Status { get; set; }
    }
}
