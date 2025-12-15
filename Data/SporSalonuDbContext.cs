using Microsoft.EntityFrameworkCore;
using web.Models; // "web" yerine proje ismini yaz

namespace web.Data // "web" yerine proje ismini yaz
{
    public class SporSalonuDbContext : DbContext
    {
        public SporSalonuDbContext(DbContextOptions<SporSalonuDbContext> options) : base(options)
        {
        }

        // Veritabanındaki "SporSalonlari" tablosunu temsil eder
        public DbSet<SporSalonu> SporSalonlari { get; set; }
    }
}