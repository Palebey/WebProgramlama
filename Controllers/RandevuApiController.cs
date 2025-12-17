using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;

namespace web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapanlar API'yi kullanabilir
    public class RandevuApiController : ControllerBase
    {
        private readonly SporSalonuDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RandevuApiController(SporSalonuDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/RandevuApi/GetMyAppointment
        [HttpGet("GetMyAppointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // LINQ SORGUSU (İsterlerdeki filtreleme şartı)
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Where(r => r.UyeId == user.Id) // Sadece benim randevularım
                .OrderByDescending(r => r.TarihSaat) // Tarihe göre sırala
                .Select(r => new
                {
                    // Döngüye girmesin diye sadece lazım olanları seçiyoruz (DTO mantığı)
                    Id = r.Id,
                    Tarih = r.TarihSaat.ToString("dd.MM.yyyy HH:mm"),
                    Antrenor = r.Antrenor.AdSoyad,
                    Durum = r.Durum,
                    DurumRenk = r.Durum == "Onaylandı" ? "success" : (r.Durum == "Reddedildi" ? "danger" : "warning")
                })
                .ToListAsync();

            return Ok(randevular);
        }
    }
}