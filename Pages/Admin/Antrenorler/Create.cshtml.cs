using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using web.Data;
using web.Models;

namespace web.Pages.Admin.Antrenorler
{
    public class CreateModel : PageModel
    {
        private readonly web.Data.SporSalonuDbContext _context;

        public CreateModel(web.Data.SporSalonuDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Antrenor Antrenor { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Antrenorler.Add(Antrenor);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
