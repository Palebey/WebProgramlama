using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Services;
using web.Models;
using web.Data;

namespace web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YapayZekaController : ControllerBase
    {
        // Değişiklik: GeminiService -> GroqService
        private readonly GroqService _aiService;
        private readonly IWebHostEnvironment _env;
        private readonly SporSalonuDbContext _context;

        public YapayZekaController(GroqService aiService, IWebHostEnvironment env, SporSalonuDbContext context)
        {
            _aiService = aiService;
            _env = env;
            _context = context;
        }

        // POST: api/YapayZeka/plan-olustur
        [HttpPost("plan-olustur")]
        public async Task<IActionResult> Olustur([FromForm] YapayZekaTalepDto talep)
        {
            if (string.IsNullOrEmpty(talep.IstekMetni))
            {
                return BadRequest(new { mesaj = "Lütfen hedefinizi yazın." });
            }

            try
            {
                string? kaydedilenResimYolu = null;
                byte[]? imageBytes = null;
                string? mimeType = null;

                // 1. Resim İşleme
                if (talep.Gorsel != null && talep.Gorsel.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await talep.Gorsel.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                        mimeType = talep.Gorsel.ContentType;
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(talep.Gorsel.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    kaydedilenResimYolu = "/uploads/" + fileName;
                }

                // 2. Groq AI Çağrısı (Prompt Hazırlama)
                string prompt = $"Sen deneyimli bir spor koçusun. Lütfen şu isteğe göre Markdown formatında, okunabilir bir program hazırla.\n\n" +
                                $"Kullanıcı Hedefi: {talep.IstekMetni}. ";

                if (talep.Boy > 0 && talep.Kilo > 0)
                    prompt += $"Kullanıcı Boyu: {talep.Boy} cm, Kilosu: {talep.Kilo} kg. ";

                if (imageBytes != null)
                    prompt += "Ek olarak; yüklenen fotoğraftaki vücut tipini veya ekipmanları analiz ederek buna uygun özel tavsiyeler ekle.";

                // Servis çağrısı
                var aiCevabi = await _aiService.GetProgramFromAi(prompt, imageBytes, mimeType);

                // 3. Veritabanına Kayıt
                var yeniPlan = new YapayZekaPlani
                {
                    Hedef = talep.IstekMetni,
                    ProgramMetni = aiCevabi,
                    ResimUrl = kaydedilenResimYolu,
                    Boy = talep.Boy,
                    Kilo = talep.Kilo,
                    UyeId = User.Identity?.Name ?? "Misafir", // Giriş yapmışsa email, yoksa misafir
                    OlusturulmaTarihi = DateTime.Now
                };

                _context.YapayZekaPlanlari.Add(yeniPlan);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = aiCevabi,
                    imageUrl = kaydedilenResimYolu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Sunucu hatası", detay = ex.Message });
            }
        }

        // GET: api/YapayZeka/gecmis-planlar (Değişiklik yok, aynen kalabilir)
        [HttpGet("gecmis-planlar")]
        public async Task<IActionResult> GetGecmisPlanlar()
        {
            // 1. O an giriş yapmış kullanıcının adını/mailini al
            var aktifKullanici = User.Identity?.Name;

            // Eğer giriş yapmamışsa boş liste döndür (Güvenlik)
            if (string.IsNullOrEmpty(aktifKullanici))
            {
                return Ok(new List<object>());
            }

            // 2. Veritabanından SADECE bu kullanıcıya ait (Where şartı) planları getir
            var planlar = await _context.YapayZekaPlanlari
                                        .Where(p => p.UyeId == aktifKullanici) // <-- İŞTE SİHİRLİ KOD BURASI
                                        .OrderByDescending(p => p.OlusturulmaTarihi)
                                        .Take(5)
                                        .Select(p => new {
                                            p.Id,
                                            p.Hedef,
                                            // Özet metni
                                            Ozet = p.ProgramMetni.Length > 50 ? p.ProgramMetni.Substring(0, 50) + "..." : p.ProgramMetni,
                                            p.OlusturulmaTarihi,
                                            p.ResimUrl
                                        })
                                        .ToListAsync();

            return Ok(planlar);
        }
    }

    public class YapayZekaTalepDto
    {
        public string IstekMetni { get; set; }
        public float? Boy { get; set; }
        public float? Kilo { get; set; }
        public IFormFile? Gorsel { get; set; }
    }
}