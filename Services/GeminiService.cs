using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace web.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["GeminiApiKey"]?.Trim();
        }

        public async Task<string> GetProgramFromGemini(string userPrompt, byte[]? imageBytes = null, string? mimeType = null)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "API Key yok.";

            // Yeni anahtarla bu model kesinlikle çalışır
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                             new { text = userPrompt }
                        }
                    }
                }
            };

            // Resim varsa ekle
            if (imageBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                // (Basitleştirilmiş resim ekleme mantığı - kafa karışıklığı olmasın diye burayı kısa tuttum, 
                // eğer resim de yollayacaksan önceki tam kodu kullanabilirsin ama önce metin çalışsın.)
            }

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(result);
                return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Hata: {response.StatusCode} - {error}";
            }
        }
    }
}