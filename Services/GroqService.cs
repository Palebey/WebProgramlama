using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace web.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            // Timeout'u Program.cs'den alıyor, burası temiz.
            _apiKey = config["GroqApiKey"]?.Trim();
        }

        public async Task<string> GetProgramFromAi(string userPrompt, byte[]? imageBytes = null, string? mimeType = null)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "API Key bulunamadı.";

            var url = "https://api.groq.com/openai/v1/chat/completions";

            // --- DEĞİŞİKLİK BURADA ---
            // Senin paylaştığın dökümana göre Llama 3 serisi (Vision ve Versatile) Nisan 2025'te kalkmış.
            // Hem resim hem metin için önerilen yeni "Multimodal" model:
            string selectedModel = "meta-llama/llama-4-scout-17b-16e-instruct";

            var messageContent = new List<object>
            {
                new { type = "text", text = userPrompt }
            };

            // Resim varsa ekle
            if (imageBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                string base64Image = Convert.ToBase64String(imageBytes);
                string dataUrl = $"data:{mimeType};base64,{base64Image}";

                messageContent.Add(new
                {
                    type = "image_url",
                    image_url = new
                    {
                        url = dataUrl
                    }
                });
            }

            var requestBody = new
            {
                model = selectedModel,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = messageContent
                    }
                },
                temperature = 0.7,
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(result);
                    return doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"API Hatası ({response.StatusCode}): {error}";
                }
            }
            catch (Exception ex)
            {
                return $"Bağlantı Hatası: {ex.Message}";
            }
        }
    }
}