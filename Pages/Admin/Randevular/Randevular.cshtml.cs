using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages.Admin
{
    // [Authorize(Roles = "Admin")] // Eðer rol sisteminiz aktifse bunu açýn
    public class RandevularModel : PageModel
    {
        private readonly SporSalonuDbContext _context;

        public RandevularModel(SporSalonuDbContext context)
        {
            _context = context;
        }

        public List<Randevu> BekleyenRandevular { get; set; }

        public async Task OnGetAsync()
        {
            // Sadece 'Onay Bekliyor' olanlarý, Üye ve Antrenör bilgisiyle beraber getir
            BekleyenRandevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Where(r => r.Durum == "Onay Bekliyor")
                .OrderBy(r => r.TarihSaat)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostOnaylaAsync(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = "Onaylandý";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReddetAsync(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = "Reddedildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}