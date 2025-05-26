using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using dosyayonetim.api.Models;

namespace dosyayonetim.api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<FileEntity> Files { get; set; }
        public DbSet<FileShareLink> FileShareLinks { get; set; }
        public DbSet<DownloadHistory> DownloadHistories { get; set; }
    }
} 