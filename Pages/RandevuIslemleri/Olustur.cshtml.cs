using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;

namespace web.Pages.RandevuIslemleri
{
    [Authorize]
    public class OlusturModel : PageModel
    {
        private readonly SporSalonuDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OlusturModel(SporSalonuDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Randevu Randevu { get; set; } = new();

        public Antrenor SecilenAntrenor { get; set; }

        // Sayfa ilk açýldýðýnda çalýþýr
        public async Task<IActionResult> OnGetAsync(int antrenorId)
        {
            SecilenAntrenor = await _context.Antrenorler.FindAsync(antrenorId);

            if (SecilenAntrenor == null)
            {
                return NotFound();
            }

            // Formda antrenör ID'sini tutmak için modele atýyoruz
            Randevu.AntrenorId = antrenorId;

            return Page();
        }

        // Frontend'den AJAX ile çaðrýlan saat hesaplama metodu
        public JsonResult OnGetSaatleriGetir(int antrenorId, string tarih)
        {
            if (string.IsNullOrEmpty(tarih)) return new JsonResult(null);

            var secilenTarih = DateTime.Parse(tarih).Date;
            var antrenor = _context.Antrenorler.Find(antrenorId);
            if (antrenor == null) return new JsonResult(null);

            var randevular = _context.Randevular
                .Where(r => r.AntrenorId == antrenorId && r.TarihSaat.Date == secilenTarih)
                .ToList();

            List<SaatSlotu> slotlar = new List<SaatSlotu>();

            TimeSpan suankiSaat = antrenor.MesaiBaslangic;
            while (suankiSaat < antrenor.MesaiBitis)
            {
                var slot = new SaatSlotu
                {
                    Saat = suankiSaat.ToString(@"hh\:mm"),
                    DurumKod = "Musait",
                    Renk = "btn-success",
                    Tiklanabilir = true
                };

                var randevu = randevular.FirstOrDefault(r => r.TarihSaat.TimeOfDay == suankiSaat);

                if (randevu != null)
                {
                    if (randevu.Durum == "Onay Bekliyor")
                    {
                        slot.DurumKod = "Onay Bekliyor";
                        slot.Renk = "btn-warning";
                        slot.Tiklanabilir = false;
                    }
                    else // Onaylandý veya Reddedildi (Reddedilenleri genelde boþ gösteririz ama burada dolu varsayalým)
                    {
                        slot.DurumKod = "Dolu";
                        slot.Renk = "btn-danger";
                        slot.Tiklanabilir = false;
                    }
                }

                // Geçmiþ saat kontrolü
                if (secilenTarih == DateTime.Today && suankiSaat < DateTime.Now.TimeOfDay)
                {
                    slot.DurumKod = "Geçmiþ";
                    slot.Renk = "btn-secondary";
                    slot.Tiklanabilir = false;
                }

                slotlar.Add(slot);
                suankiSaat = suankiSaat.Add(TimeSpan.FromHours(1));
            }

            return new JsonResult(slotlar);
        }

        // Form Post edildiðinde çalýþýr
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Antrenör bilgisini tekrar çek (Sayfa yenilenirse baþlýk kaybolmasýn diye)
            var antrenor = await _context.Antrenorler.FindAsync(Randevu.AntrenorId);
            if (antrenor == null) return NotFound();

            // *** KRÝTÝK: Hata olursa ekran bozulmasýn diye nesneyi dolduruyoruz ***
            SecilenAntrenor = antrenor;

            // 2. Tarih ve Saat Kontrolleri
            if (Randevu.TarihSaat.Year < 2000)
            {
                ModelState.AddModelError("Randevu.TarihSaat", "Lütfen takvimden geçerli bir tarih ve saat seçiniz.");
                return Page();
            }

            TimeSpan secilenSaat = Randevu.TarihSaat.TimeOfDay;
            if (secilenSaat < antrenor.MesaiBaslangic || secilenSaat > antrenor.MesaiBitis)
            {
                ModelState.AddModelError("Randevu.TarihSaat", "Seçilen saat mesai saatleri dýþýndadýr.");
                return Page();
            }

            if (Randevu.TarihSaat < DateTime.Now)
            {
                ModelState.AddModelError("Randevu.TarihSaat", "Geçmiþ zamana randevu alýnamaz.");
                return Page();
            }

            // 3. Kullanýcýyý bul ve modele ekle
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login"); // Oturum düþmüþse

            Randevu.UyeId = user.Id;
            Randevu.Durum = "Onay Bekliyor";

            // *** KRÝTÝK: Navigation Property Hatalarýný Temizle ***
            // Formdan Uye ve Antrenor nesneleri gelmediði için ModelState hata verir. Bunlarý siliyoruz.
            ModelState.Remove("Randevu.Uye");
            ModelState.Remove("Randevu.Antrenor");
            ModelState.Remove("Randevu.UyeId");
            ModelState.Remove("Randevu.Durum");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Randevular.Add(Randevu);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }

    // AJAX Dönüþ Modeli
    public class SaatSlotu
    {
        public string Saat { get; set; }
        public string DurumKod { get; set; }
        public string Renk { get; set; }
        public bool Tiklanabilir { get; set; }
    }
}