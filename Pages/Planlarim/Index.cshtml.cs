using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages.Planlarim
{
    public class IndexModel : PageModel
    {
        private readonly SporSalonuDbContext _context;

        public IndexModel(SporSalonuDbContext context)
        {
            _context = context;
        }

        // View tarafýnda kullandýðýn Property
        public YapayZekaPlani? MevcutPlan { get; set; }

        public async Task OnGetAsync()
        {
            // Veritabanýndan en son oluþturulan planý getiriyoruz.
            // Eðer giriþ yapmýþ kullanýcýya özel olsun istersen Where(x => x.UyeId == User.Identity.Name) ekleyebilirsin.
            // Þimdilik en son eklenen planý getiriyoruz:
            MevcutPlan = await _context.YapayZekaPlanlari
                                       .OrderByDescending(p => p.OlusturulmaTarihi)
                                       .FirstOrDefaultAsync();
        }
    }
}