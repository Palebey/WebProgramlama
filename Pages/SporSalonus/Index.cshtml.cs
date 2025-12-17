using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data; // Proje ismin farklýysa burayý düzelt
using web.Models;

namespace web.Pages.SporSalonus
{
    public class IndexModel : PageModel
    {
        private readonly SporSalonuDbContext _context;

        public IndexModel(SporSalonuDbContext context)
        {
            _context = context;
        }

        public IList<SporSalonu> SporSalonu { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.SporSalonlari != null)
            {
                // Tüm salonlarý veritabanýndan getiriyoruz
                SporSalonu = await _context.SporSalonlari.ToListAsync();
            }
        }
    }
}