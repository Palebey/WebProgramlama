using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // Bu kütüphane gerekli

namespace web.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // IConfiguration'ı constructor'a ekledik (Dependency Injection)
        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            // appsettings.json dosyasındaki "GeminiApiKey" değerini okuyoruz
            _apiKey = config["GeminiApiKey"];
        }

        public async Task<string> GetProgramFromGemini(string userPrompt, byte[]? imageBytes = null, string? mimeType = null)
        {
            // Eğer anahtar okunamazsa hata vermeyelim, uyarı dönelim (Güvenlik önlemi)
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "API Anahtarı bulunamadı. Lütfen appsettings.json dosyasını kontrol edin.";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            // ... Kodun geri kalanı aynı şekilde devam ediyor ...
            // (Aşağıdaki kısımlarda değişiklik yapmana gerek yok, aynen kalsın)

            object partText = new { text = userPrompt };
            object[] parts;

            if (imageBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                string base64Image = Convert.ToBase64String(imageBytes);
                object partImage = new
                {
                    inline_data = new
                    {
                        mime_type = mimeType,
                        data = base64Image
                    }
                };
                parts = new[] { partText, partImage };
            }
            else
            {
                parts = new[] { partText };
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = parts }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                try
                {
                    return doc.RootElement.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text")
                                      .GetString();
                }
                catch
                {
                    return "AI cevabı işlenirken format hatası oluştu.";
                }
            }

            return "Gemini API Hatası: " + response.StatusCode;
        }
    }
}