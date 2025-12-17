using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages.SporSalonus
{
    public class DetailsModel : PageModel
    {
        private readonly SporSalonuDbContext _context;

        public DetailsModel(SporSalonuDbContext context)
        {
            _context = context;
        }

        public SporSalonu SporSalonu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.SporSalonlari == null)
            {
                return NotFound();
            }

            // ÖNEMLÝ: Include(s => s.Antrenorler) diyerek o salonun hocalarýný da getiriyoruz.
            var sporsalonu = await _context.SporSalonlari
                .Include(s => s.Antrenorler)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sporsalonu == null)
            {
                return NotFound();
            }
            else
            {
                SporSalonu = sporsalonu;
            }
            return Page();
        }
    }
}