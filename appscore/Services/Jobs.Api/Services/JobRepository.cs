using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Jobs.Api.Models;
using Dapper;
using System.Threading;

namespace Jobs.Api.Services
{
    public class JobRepository : IJobRepository
    {
        private readonly string _connectionString;
        private const int MaxRetries = 5;
        private const int RetryDelayMs = 1000; // 1 seconde

        public JobRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Connection => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Job>> GetAll()
        {
            return await ExecuteWithRetry(async () => 
            {
                using (var dbConnection = Connection)
                {
                    dbConnection.Open();
                    return await dbConnection.QueryAsync<Job>("SELECT * FROM Jobs");
                }
            });
        }

        public async Task<Job> Get(int jobId)
        {
            return await ExecuteWithRetry(async () =>
            {
                using (var dbConnection = Connection)
                {
                    dbConnection.Open();
                    return await dbConnection.QueryFirstOrDefaultAsync<Job>("SELECT * FROM Jobs where JobId=@JobId", new { JobId = jobId });
                }
            });
        }

        public async Task<int> AddApplicant(JobApplicant jobApplicant)
        {
            return await ExecuteWithRetry(async () =>
            {
                using (var dbConnection = Connection)
                {
                    dbConnection.Open();
                    return await dbConnection.ExecuteAsync(
                        "insert JobApplicants values(@jobId,@applicantId,@name,@email,getutcdate(),1)",
                        new
                        {
                            jobId = jobApplicant.JobId,
                            applicantId = jobApplicant.ApplicantId,
                            name = jobApplicant.Name,
                            email = jobApplicant.Email
                        });
                }
            });
        }

        // Méthode générique pour ré-essayer les opérations de base de données
        private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> operation)
        {
            int retryCount = 0;
            
            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (SqlException ex)
                {
                    // Si nous avons déjà essayé le nombre maximum de fois, rethrow
                    if (retryCount >= MaxRetries)
                    {
                        throw;
                    }
                    
                    // Erreurs qui peuvent être temporaires: connexion, base de données non disponible
                    if (ex.Number == 4060 || // Cannot open database
                        ex.Number == 233 ||  // No process is on the other end of the pipe
                        ex.Number == -2 ||   // Timeout
                        ex.Number == 10928 || // Resource ID : %d. The %s limit for the database is %d and has been reached
                        ex.Number == 1205)   // Transaction was deadlocked
                    {
                        // Log l'erreur et attendre avant de réessayer
                        Console.WriteLine($"Erreur SQL {ex.Number}, retentative {retryCount + 1}/{MaxRetries} dans {RetryDelayMs}ms : {ex.Message}");
                        retryCount++;
                        
                        // Attente exponentielle (1s, 2s, 4s, 8s, 16s)
                        await Task.Delay(RetryDelayMs * (int)Math.Pow(2, retryCount - 1));
                        continue;
                    }
                    
                    // Pour les autres types d'erreurs SQL, rethrow
                    throw;
                }
            }
        }
    }
}