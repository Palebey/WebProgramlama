using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Data
{
    public class SporSalonuDbContext : IdentityDbContext
    {
        public SporSalonuDbContext(DbContextOptions<SporSalonuDbContext> options)
            : base(options)
        {
        }

        public DbSet<SporSalonu> SporSalonlari { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<YapayZekaPlani> YapayZekaPlanlari { get; set; }

        // --- BU METODU SINIFIN İÇİNE EKLİYORSUN ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. DİKKAT: Identity tablolarının (AspNetUsers vs.) oluşması için bu satır ŞARTTIR.
            // Bunu silersen "IdentityUser" hatası alırsın.
            base.OnModelCreating(modelBuilder);

            // 2. Senin decimal (para birimi) ayarın:
            modelBuilder.Entity<web.Models.SporSalonu>()
                .Property(s => s.UyelikUcreti)
                .HasColumnType("decimal(18,2)");
        }
    }
}