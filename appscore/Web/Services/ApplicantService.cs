using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Http;
using Newtonsoft.Json;
using Web.Config;
using Web.ViewModels;

namespace Web.Services
{
    public class ApplicantService : IApplicantService
    {
        private readonly IHttpClient _apiClient;
        private readonly ApiConfig _apiConfig;

        public ApplicantService(IHttpClient httpClient, ApiConfig apiConfig)
        {
            _apiClient = httpClient;
            _apiConfig = apiConfig;
        }

        public async Task<IEnumerable<Applicant>> GetApplicants()
        {
            try
            {
                var dataString = await _apiClient.GetStringAsync(_apiConfig.ApplicantsApiUrl + "/api/applicants");
                
                // Vérification simple pour éviter le crash JSON
                if (dataString != null && dataString.TrimStart().StartsWith("<"))
                {
                    throw new Exception($"Received HTML instead of JSON from {_apiConfig.ApplicantsApiUrl}/api/applicants. Response: {dataString.Substring(0, Math.Min(dataString.Length, 200))}...");
                }
                
                return JsonConvert.DeserializeObject<IEnumerable<Applicant>>(dataString);
            }
            catch (Exception ex)
            {
                // Log l'erreur pour aider au débogage
                Console.WriteLine($"Error fetching applicants: {ex.Message}");
                throw new Exception($"Failed to fetch applicants from {_apiConfig.ApplicantsApiUrl}/api/applicants. Error: {ex.Message}", ex);
            }
        }
    }
}