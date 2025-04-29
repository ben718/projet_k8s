using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Http
{
    public class StandardHttpClient : IHttpClient
    {
        private static readonly HttpClient Client = new HttpClient();
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 200; // Milliseconds
        
        public async Task<string> GetStringAsync(string uri)
        {
            return await ExecuteWithRetryAsync(async () => 
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                var response = await Client.SendAsync(requestMessage);
                
                // Vérification du status code pour rendre les erreurs HTTP plus explicites
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Request to {uri} failed with status {response.StatusCode}. " +
                        $"Response: {content.Substring(0, Math.Min(content.Length, 500))}");
                }
                
                return await response.Content.ReadAsStringAsync();
            });
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string uri, T item)
        {
            return await ExecuteWithRetryAsync(async () => 
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8,"application/json")
                };

                var response = await Client.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Request to {uri} failed with status {response.StatusCode}. " +
                        $"Response: {content.Substring(0, Math.Min(content.Length, 500))}");
                }

                return response;
            });
        }
        
        // Méthode générique pour ré-essayer les opérations HTTP
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
        {
            int attempts = 0;
            
            while (true)
            {
                try
                {
                    attempts++;
                    return await operation();
                }
                catch (Exception ex)
                {
                    // Si on a déjà essayé le nombre maximum de fois ou si l'erreur n'est pas liée 
                    // à la connectivité, on la propage
                    if (attempts >= MaxRetries || 
                        !(ex is HttpRequestException || ex is WebException || ex is TaskCanceledException))
                    {
                        throw;
                    }
                    
                    // Log l'erreur et attendre avant de réessayer
                    Console.WriteLine($"Erreur HTTP, retentative {attempts}/{MaxRetries} dans {RetryDelayMs}ms : {ex.Message}");
                    
                    // Attente exponentielle (200ms, 400ms, 800ms)
                    await Task.Delay(RetryDelayMs * (int)Math.Pow(2, attempts - 1));
                }
            }
        }
    }
}
