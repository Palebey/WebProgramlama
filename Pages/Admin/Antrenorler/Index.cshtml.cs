using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages.Admin.Antrenorler
{
    public class IndexModel : PageModel
    {
        private readonly web.Data.SporSalonuDbContext _context;

        public IndexModel(web.Data.SporSalonuDbContext context)
        {
            _context = context;
        }

        public IList<Antrenor> Antrenor { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu).ToListAsync();
        }
    }
}
