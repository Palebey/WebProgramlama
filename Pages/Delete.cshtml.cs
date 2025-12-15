using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly web.Data.SporSalonuDbContext _context;

        public DeleteModel(web.Data.SporSalonuDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SporSalonu SporSalonu { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporsalonu = await _context.SporSalonlari.FirstOrDefaultAsync(m => m.Id == id);

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sporsalonu = await _context.SporSalonlari.FindAsync(id);
            if (sporsalonu != null)
            {
                SporSalonu = sporsalonu;
                _context.SporSalonlari.Remove(SporSalonu);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
