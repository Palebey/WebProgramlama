using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace web.Pages.Planlarim
{
    [Authorize] // Sadece giriþ yapmýþ üyeler girebilir
    public class IndexModel : PageModel
    {
        private readonly SporSalonuDbContext _context;

        public IndexModel(SporSalonuDbContext context)
        {
            _context = context;
        }

        // Tek bir plan yerine bir liste tutalým, böylece geçmiþi de listeleriz
        public IList<YapayZekaPlani> GecmisPlanlar { get; set; } = new List<YapayZekaPlani>();

        // Ekranda detayýný göstereceðimiz en son plan
        public YapayZekaPlani? EnSonPlan { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Giriþ yapan kullanýcýnýn kimliðini al
            var uyeId = User.Identity?.Name;

            if (!string.IsNullOrEmpty(uyeId))
            {
                // 2. SADECE bu kullanýcýya ait planlarý çek (Sorunu çözen satýr burasý)
                GecmisPlanlar = await _context.YapayZekaPlanlari
                                              .Where(x => x.UyeId == uyeId) // <-- FÝLTRELEME
                                              .OrderByDescending(x => x.OlusturulmaTarihi)
                                              .ToListAsync();

                // Listenin ilk elemaný en güncel plandýr
                EnSonPlan = GecmisPlanlar.FirstOrDefault();
            }
        }
    }
}