using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Http;
using Newtonsoft.Json;
using Web.Config;
using Web.ViewModels;

namespace Web.Services
{
    public class JobService : IJobService
    {
        private readonly IHttpClient _apiClient;
        private readonly ApiConfig _apiConfig;

        public JobService(IHttpClient httpClient, ApiConfig apiConfig)
        {
            _apiClient = httpClient;
            _apiConfig = apiConfig;
        }

        public async Task<IEnumerable<Job>> GetJobs()
        {
            try
            {
                var dataString = await _apiClient.GetStringAsync(_apiConfig.JobsApiUrl+"/api/jobs");
                
                // Vérification simple pour éviter le crash JSON
                if (dataString != null && dataString.TrimStart().StartsWith("<"))
                {
                    throw new Exception($"Received HTML instead of JSON from {_apiConfig.JobsApiUrl}/api/jobs. Response: {dataString.Substring(0, Math.Min(dataString.Length, 200))}...");
                }
                
                return JsonConvert.DeserializeObject<IEnumerable<Job>>(dataString);
            }
            catch (Exception ex)
            {
                // Log l'erreur pour aider au débogage
                Console.WriteLine($"Error fetching jobs: {ex.Message}");
                throw new Exception($"Failed to fetch jobs from {_apiConfig.JobsApiUrl}/api/jobs. Error: {ex.Message}", ex);
            }
        }

        public async Task<Job> GetJob(int jobId)
        {
            var dataString = await _apiClient.GetStringAsync(_apiConfig.JobsApiUrl + "/api/jobs/"+jobId);
            return JsonConvert.DeserializeObject<Job>(dataString);
        }

        public async Task AddApplicant(JobApplicant jobApplicant)
        {
            var response = await _apiClient.PostAsync(_apiConfig.JobsApiUrl + "/api/jobs/applicants",jobApplicant);
            response.EnsureSuccessStatusCode();
        }
    }
}

