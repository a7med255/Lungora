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

            modelBuilder.Entity<Article>()
                .HasOne(c => c.Category) //category 1=>m Articles
                .WithMany(a => a.Articles)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasOne(c => c.Category) //category 1=>m Doctors
                .WithMany(a => a.Doctors)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkingHour>()
              .HasOne(w => w.Doctor)//Doctor 1=>m WorkingHours
              .WithMany(d => d.WorkingHours)
              .HasForeignKey(w => w.DoctorId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkingHour>()
            .HasIndex(w => new { w.DoctorId, w.DayOfWeek })
            .IsUnique();
        }




        public virtual DbSet<ApplicationUser> Users { get; set; }
        public virtual DbSet<Article> TbArticles { get; set; }
        public virtual DbSet<Category> TbCategories { get; set; }
        public virtual DbSet<Doctor> TbDoctors { get; set; }
        public virtual DbSet<WorkingHour> TbWorkingHours { get; set; }
        public virtual DbSet<UserAIResult> UserAIResults { get; set; }


    }
}
