using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SDATweb
{
    public class HttpRequestSender
    {
        public async Task<string> SendHTTP(string url, string prompt, string systemPrompt)
        {
            var payload = new
            {
                model = "llama-3",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string jsonPayload = JsonSerializer.Serialize(payload);
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    string assistantReply = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();
                    return assistantReply;
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP Request Error: {httpEx.Message}");
                    return $"HTTP Request Error: {httpEx.Message}";
                }   
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return $"Error: {ex.Message}";
                }
            }
        }
    }
}
