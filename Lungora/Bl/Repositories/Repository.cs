using Lungora.Bl.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lungora.Bl.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly LungoraContext context;
        private readonly DbSet<T> dbSet;
        public Repository(LungoraContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }
        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                IQueryable<T> query = dbSet;

                var result = await query.FirstOrDefaultAsync(filter);

                return result;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public async Task<T> AddAsync(T Entity)
        {
            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

            try
            {
                await dbSet.AddAsync(Entity);
                await context.SaveChangesAsync();

                return Entity;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        public async Task RemoveAsync(Expression<Func<T, bool>> filter)
        {
            var res = await GetSingleAsync(filter);

            if (res is not null)
            {
                dbSet.Remove(res);
                await context.SaveChangesAsync();
            }
            else
                throw new Exception("Not found!");

        }
    }
}
