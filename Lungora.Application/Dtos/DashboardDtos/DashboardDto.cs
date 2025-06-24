using Lungora.Models;

namespace Lungora.Dtos.DashboardDtos
{
    public class DashboardDto
    {
        public List<Doctor> randomDoctorsFullData { get; set; }
        public List<Article> randomArticles { get; set; }
        public int totalDoctors { get; set; }
        public double doctorsLastWeekPercentage { get; set; }
        public int activeUserCount { get; set; }
        public double activeUsersLastWeekPercentage { get; set; }
        public List<PredictionStatsDto> predictionStats { get; set; }
        public List<AiModelDto> aiResult { get; set; }
    }
}
