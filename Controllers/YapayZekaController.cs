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
        private readonly GroqService _aiService;
        private readonly IWebHostEnvironment _env;
        private readonly SporSalonuDbContext _context;
        private readonly HttpClient _httpClient; // Resim indirmek için

        public YapayZekaController(GroqService aiService, IWebHostEnvironment env, SporSalonuDbContext context)
        {
            _aiService = aiService;
            _env = env;
            _context = context;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        [HttpPost("plan-olustur")]
        public async Task<IActionResult> Olustur([FromForm] YapayZekaTalepDto talep)
        {
            if (string.IsNullOrEmpty(talep.IstekMetni))
                return BadRequest(new { mesaj = "Lütfen hedefinizi yazın." });

            try
            {
                string? finalResimYolu = null;
                string personDescription = "a fitness enthusiast"; // Varsayılan tarif

                // 1. KULLANICI RESİM YÜKLEDİ Mİ?
                if (talep.Gorsel != null && talep.Gorsel.Length > 0)
                {
                    byte[] originalImageBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await talep.Gorsel.CopyToAsync(memoryStream);
                        originalImageBytes = memoryStream.ToArray();
                    }

                    // A) ÖNCE RESMİ ANALİZ ET (Groq Vision)
                    // Resmi tarif ettiriyoruz ki benzerini çizebilelim
                    var analizSonucu = await _aiService.DescribePersonInImage(originalImageBytes, talep.Gorsel.ContentType);

                    if (!analizSonucu.StartsWith("Groq API Hatası"))
                    {
                        personDescription = analizSonucu;
                    }

                    // B) GARANTİLİ RESİM ÜRETİMİ (Pollinations.ai)
                    // Prompt: "Tarif edilen kişinin [hedef] sonrası fit hali"
                    string prompt = $"highly detailed realistic fitness photo of {personDescription}, body transformation after 6 months of {talep.IstekMetni}, sweaty skin texture, gym lighting, 8k, cinematic shot";

                    // Pollinations URL'si (Boşlukları temizle)
                    string pollUrl = $"https://image.pollinations.ai/prompt/{Uri.EscapeDataString(prompt)}?width=1024&height=1024&nologo=true";

                    try
                    {
                        // Resmi internetten indirip sunucuya kaydedelim
                        var aiImageBytes = await _httpClient.GetByteArrayAsync(pollUrl);
                        finalResimYolu = await DosyayiKaydet(aiImageBytes, ".jpg");
                    }
                    catch
                    {
                        // Eğer internetten inmezse mecburen orijinali kaydet
                        finalResimYolu = await DosyayiKaydet(originalImageBytes, Path.GetExtension(talep.Gorsel.FileName));
                    }
                }
                else
                {
                    // Resim yoksa standart görsel üret
                    string prompt = $"cinematic fitness photo, gym workout, person doing {talep.IstekMetni}, dramatic lighting, 8k, high detail";
                    string pollUrl = $"https://image.pollinations.ai/prompt/{Uri.EscapeDataString(prompt)}?width=1024&height=1024&nologo=true";

                    var aiImageBytes = await _httpClient.GetByteArrayAsync(pollUrl);
                    finalResimYolu = await DosyayiKaydet(aiImageBytes, ".jpg");
                }

                // 2. METİN PLANI OLUŞTUR (SADECE METİN GÖNDER)
                string textPrompt = $"Sen uzman bir antrenörsün. Kullanıcı Hedefi: {talep.IstekMetni}. ";
                if (talep.Boy > 0) textPrompt += $"Boy: {talep.Boy} cm. ";
                if (talep.Kilo > 0) textPrompt += $"Kilo: {talep.Kilo} kg. ";

                if (personDescription != "a fitness enthusiast")
                {
                    textPrompt += $"Kullanıcının vücut analizi: {personDescription}. Buna göre kişiselleştirilmiş program yaz.";
                }

                textPrompt += " Çıktıyı Markdown formatında ver.";

                var aiCevabi = await _aiService.GetProgramFromAi(textPrompt);

                // 3. KAYIT
                var yeniPlan = new YapayZekaPlani
                {
                    Hedef = talep.IstekMetni,
                    ProgramMetni = aiCevabi,
                    ResimUrl = finalResimYolu, // <-- BURASI ARTIK KESİN AI GÖRSELİ
                    Boy = talep.Boy,
                    Kilo = talep.Kilo,
                    UyeId = User.Identity?.Name ?? "Misafir",
                    OlusturulmaTarihi = DateTime.Now
                };

                _context.YapayZekaPlanlari.Add(yeniPlan);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = aiCevabi, imageUrl = finalResimYolu });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mesaj = "Hata oluştu", detay = ex.Message });
            }
        }

        private async Task<string> DosyayiKaydet(byte[] fileBytes, string extension)
        {
            // "ai-generated" klasörüne kaydediyoruz ki arayüzde ayırt edebilelim
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "ai-generated");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            return $"/uploads/ai-generated/{fileName}";
        }

        // GET Metodu (Değişiklik Yok)
        [HttpGet("gecmis-planlar")]
        public async Task<IActionResult> GetGecmisPlanlar()
        {
            var aktifKullanici = User.Identity?.Name;
            if (string.IsNullOrEmpty(aktifKullanici)) return Ok(new List<object>());

            var planlar = await _context.YapayZekaPlanlari
                .Where(p => p.UyeId == aktifKullanici)
                .OrderByDescending(p => p.OlusturulmaTarihi)
                .Take(5)
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