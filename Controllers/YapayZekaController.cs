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
        private readonly GeminiService _geminiService;
        private readonly IWebHostEnvironment _env;
        private readonly SporSalonuDbContext _context;

        public YapayZekaController(GeminiService geminiService, IWebHostEnvironment env, SporSalonuDbContext context)
        {
            _geminiService = geminiService;
            _env = env;
            _context = context;
        }

        // POST: api/YapayZeka/plan-olustur
        [HttpPost("plan-olustur")]
        public async Task<IActionResult> Olustur([FromForm] YapayZekaTalepDto talep)
        {
            // Validasyon
            if (string.IsNullOrEmpty(talep.IstekMetni))
            {
                return BadRequest(new { mesaj = "Lütfen hedefinizi yazın." });
            }

            try
            {
                string? kaydedilenResimYolu = null;
                byte[]? imageBytes = null;
                string? mimeType = null;

                // 1. Resim İşleme (Güvenlik Önlemli)
                if (talep.Gorsel != null && talep.Gorsel.Length > 0)
                {
                    // Önce dosyayı RAM'e (Byte Array) alalım
                    using (var memoryStream = new MemoryStream())
                    {
                        await talep.Gorsel.CopyToAsync(memoryStream);
                        imageBytes = memoryStream.ToArray();
                        mimeType = talep.Gorsel.ContentType;
                    }

                    // Diske kaydetmek için klasör yolu
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(talep.Gorsel.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Diske yazma (Akış hatası olmaması için byte array'den yazıyoruz)
                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                    kaydedilenResimYolu = "/uploads/" + fileName;
                }

                // 2. Gemini Yapay Zeka Çağrısı
                string prompt = $"Sen profesyonel bir spor koçusun. Kullanıcı Hedefi: {talep.IstekMetni}. ";
                if (talep.Boy > 0 && talep.Kilo > 0) prompt += $"Kullanıcı Boyu: {talep.Boy} cm, Kilosu: {talep.Kilo} kg. ";
                if (imageBytes != null) prompt += "Lütfen yüklenen vücut/malzeme görselini de analiz ederek tavsiye ver.";

                var aiCevabi = await _geminiService.GetProgramFromGemini(prompt, imageBytes, mimeType);

                // 3. Veritabanına Kayıt
                var yeniPlan = new YapayZekaPlani
                {
                    Hedef = talep.IstekMetni,
                    ProgramMetni = aiCevabi,
                    ResimUrl = kaydedilenResimYolu,
                    Boy = talep.Boy,
                    Kilo = talep.Kilo,
                    UyeId = "Misafir",
                    OlusturulmaTarihi = DateTime.Now
                };

                _context.YapayZekaPlanlari.Add(yeniPlan);
                await _context.SaveChangesAsync();

                // 4. Sonucu Döndür
                return Ok(new
                {
                    success = true,
                    data = aiCevabi,
                    imageUrl = kaydedilenResimYolu
                });
            }
            catch (Exception ex)
            {
                // Hatayı konsolda görebilmek için
                Console.WriteLine("HATA OLUŞTU: " + ex.Message);
                return StatusCode(500, new { mesaj = "Sunucu hatası", detay = ex.Message });
            }
        }

        // GET: api/YapayZeka/gecmis-planlar
        [HttpGet("gecmis-planlar")]
        public async Task<IActionResult> GetGecmisPlanlar()
        {
            var planlar = await _context.YapayZekaPlanlari
                                        .OrderByDescending(p => p.OlusturulmaTarihi)
                                        .Take(5)
                                        .Select(p => new {
                                            p.Id,
                                            p.Hedef,
                                            Ozet = p.ProgramMetni.Length > 50 ? p.ProgramMetni.Substring(0, 50) + "..." : p.ProgramMetni,
                                            p.OlusturulmaTarihi,
                                            p.ResimUrl
                                        })
                                        .ToListAsync();

            return Ok(planlar);
        }
    }

    // DTO Sınıfı
    public class YapayZekaTalepDto
    {
        public string IstekMetni { get; set; }
        public float? Boy { get; set; }
        public float? Kilo { get; set; }
        public IFormFile? Gorsel { get; set; }
    }
}