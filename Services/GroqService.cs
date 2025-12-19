using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace web.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _groqApiKey;
        private readonly string _hfApiKey;

        public GroqService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(3); // Süreyi uzattık
            _groqApiKey = config["GroqApiKey"]?.Trim();
            _hfApiKey = config["HuggingFaceApiKey"]?.Trim();
        }

        public async Task<string> DescribePersonInImage(byte[] imageBytes, string mimeType)
        {
            return await CallGroqApi("Analyze this fitness photo. Describe the person's gender, approximate body fat percentage, muscle mass, and body type in 2 sentences.", imageBytes, mimeType, isVisionRequest: true);
        }

        public async Task<string> GetProgramFromAi(string userPrompt)
        {
            return await CallGroqApi(userPrompt, null, null, isVisionRequest: false);
        }

        // --- GROQ API ÇAĞRISI (Aynı kalıyor) ---
        private async Task<string> CallGroqApi(string promptText, byte[]? imageBytes, string? mimeType, bool isVisionRequest)
        {
            if (string.IsNullOrEmpty(_groqApiKey)) return "Groq API Key yok.";

            var url = "https://api.groq.com/openai/v1/chat/completions";

            // GÜNCEL MODELLER
            string selectedModel = isVisionRequest
                ? "meta-llama/llama-4-scout-17b-16e-instruct" // Vision için en yenisi (April 2025)
                : "llama-3.3-70b-versatile";                   // Metin için en yenisi (Jan 2025)

            object messagesPayload;

            if (isVisionRequest && imageBytes != null)
            {
                string base64Image = Convert.ToBase64String(imageBytes);
                string dataUrl = $"data:{mimeType};base64,{base64Image}";
                messagesPayload = new[] { new { role = "user", content = new object[] { new { type = "text", text = promptText }, new { type = "image_url", image_url = new { url = dataUrl } } } } };
            }
            else
            {
                messagesPayload = new[] { new { role = "user", content = promptText } };
            }

            var requestBody = new { model = selectedModel, messages = messagesPayload, temperature = 0.6, max_tokens = 2048 };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(result);
                    return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                }
                return $"Groq API Hatası: {response.StatusCode}";
            }
            catch (Exception ex) { return $"Bağlantı Hatası: {ex.Message}"; }
        }

        // --- HUGGING FACE RESİM ÜRETME (GÜNCELLENDİ: YEDEK PLANLI) ---
        public async Task<byte[]> GenerateImageFromHuggingFace(string prompt)
        {
            if (string.IsNullOrEmpty(_hfApiKey)) return null;

            // 1. ÖNCELİK: FLUX.1-dev (En Kaliteli)
            // Not: Bu modelin çalışması için Hugging Face sitesinde lisansını kabul etmiş olman gerek.
            var imageBytes = await TryGenerateImage("black-forest-labs/FLUX.1-dev", prompt);

            if (imageBytes != null) return imageBytes;

            // 2. YEDEK PLAN: Stable Diffusion XL (Daha Hızlı ve Güvenilir)
            // Eğer FLUX hata verirse (timeout, yetki vs.) burası devreye girer.
            imageBytes = await TryGenerateImage("stabilityai/stable-diffusion-xl-base-1.0", prompt);

            if (imageBytes != null) return imageBytes;

            // 3. SON ÇARE: Stable Diffusion v1.5 (En basit ve hızlısı)
            return await TryGenerateImage("runwayml/stable-diffusion-v1-5", prompt);
        }

        // Yardımcı Metot: Belirli bir modelden resim isteme
        private async Task<byte[]> TryGenerateImage(string modelId, string prompt)
        {
            try
            {
                var url = $"https://api-inference.huggingface.co/models/{modelId}";
                var requestBody = new { inputs = prompt };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _hfApiKey);

                var response = await _httpClient.PostAsync(url, content);

                // Eğer "Model Yükleniyor" hatası (503) alırsak biraz bekleyip tekrar deneyelim
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    await Task.Delay(3000); // 3 saniye bekle
                    response = await _httpClient.PostAsync(url, content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    // Hata detayını konsola yazalım (Debug için)
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"HF Model Hatası ({modelId}): {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HF Bağlantı Hatası ({modelId}): {ex.Message}");
                return null;
            }
        }
    }
}