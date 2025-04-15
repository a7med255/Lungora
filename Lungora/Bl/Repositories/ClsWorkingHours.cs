using Lungora.Bl.Interfaces;
using Lungora.Dtos.ArticleDtos;
using Lungora.Models;
using Microsoft.EntityFrameworkCore;

namespace Lungora.Bl.Repositories
{
    public class ClsWorkingHours:Repository<WorkingHour>,IWorkingHour
    {
        private readonly LungoraContext context;
        public ClsWorkingHours(LungoraContext context):base(context)
        {
            this.context = context;
        }
        public async Task<List<WorkingHour>> GetAllByDoctorIdAsync(int doctorId)
        {
            try
            {
                var WorkingHours = await context.TbWorkingHours
                                .Where(c => c.DoctorId == doctorId)
                                .ToListAsync();
                if (WorkingHours == null)
                {
                    return new List<WorkingHour>();
                }

                return WorkingHours;

            }
            catch
            {
                return new List<WorkingHour>();
            }
        }
        public async Task<WorkingHour?> UpdateAsync(int id, WorkingHour updatedWorkingHour)
        {
            var existingWorkingHour = await GetSingleAsync(x => x.Id == id);
            if (existingWorkingHour is null)
                return null;

            // Update only fields that are allowed to change
            existingWorkingHour.DayOfWeek = updatedWorkingHour.DayOfWeek;
            existingWorkingHour.StartTime = updatedWorkingHour.StartTime;
            existingWorkingHour.EndTime = updatedWorkingHour.EndTime;

            if (!existingWorkingHour.IsValidTimeRange())
            {
                throw new ArgumentException("End time must be greater than start time.");
            }


            context.TbWorkingHours.Update(existingWorkingHour); // EF automatically tracks the entity
            await context.SaveChangesAsync();

            return existingWorkingHour;
        }
    }

}
