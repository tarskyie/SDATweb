using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SDATweb
{
    public class HttpRequestSender
    {
        public async Task<string> SendHTTP(string url, string data)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent(data, Encoding.UTF8, "text/plain");
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                    string responseString = await response.Content.ReadAsStringAsync();
                    return responseString;
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
