using Lungora.Bl.Interfaces;
using Lungora.Models;

namespace Lungora.Bl.Repositories
{
    public class ClsWorkingHours:Repository<WorkingHour>,IWorkingHour
    {
        private readonly LungoraContext context;
        public ClsWorkingHours(LungoraContext context):base(context)
        {
            this.context = context;
        }
    }

}
