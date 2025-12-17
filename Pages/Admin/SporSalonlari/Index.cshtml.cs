using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages.Admin.SporSalonlari
{
    public class IndexModel : PageModel
    {
        private readonly web.Data.SporSalonuDbContext _context;

        public IndexModel(web.Data.SporSalonuDbContext context)
        {
            _context = context;
        }

        public IList<SporSalonu> SporSalonu { get;set; } = default!;

        public async Task OnGetAsync()
        {
            SporSalonu = await _context.SporSalonlari.ToListAsync();
        }
    }
}
