using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // BU SATIR EKLENMELİ
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Data
{
    // DİKKAT: Burası artık "DbContext" değil "IdentityDbContext" olmalı
    public class SporSalonuDbContext : IdentityDbContext
    {
        public SporSalonuDbContext(DbContextOptions<SporSalonuDbContext> options)
            : base(options)
        {
        }

        // Senin mevcut tabloların aynen kalıyor
        public DbSet<SporSalonu> SporSalonlari { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}