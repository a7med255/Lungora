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
                .HasOne(c => c.Category)
                .WithMany(a => a.Articles)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }




        public virtual DbSet<ApplicationUser> Users { get; set; }
        public virtual DbSet<Article> TbArticles { get; set; }
        public virtual DbSet<Category> TbCategories { get; set; }


    }
}
