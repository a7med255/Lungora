using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lungora.Models;
using System.Reflection.Emit;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
namespace Lungora.Bl
{
    public class LungoraContext : IdentityDbContext<ApplicationUser>
    {
        public LungoraContext()
        {

        }
        public LungoraContext(DbContextOptions<LungoraContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


           
                
        }


       

        public DbSet<ApplicationUser> Users { get; set; }


    }
}
