using Lungora.Dtos.DashboardDtos;

namespace Lungora.Bl.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
    }

}
