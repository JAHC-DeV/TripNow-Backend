using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using TripNow_JoseAngel.Application.DTOs;
using TripNow_JoseAngel.Application.Interfaces;

namespace TripNow_JoseAngel.Infrastructure.ExternalServices
{
    public class RiskEvaluationService : IRiskEvaluation
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RiskEvaluationService> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _policy;

        public RiskEvaluationService(HttpClient httpClient, IConfiguration configuration, ILogger<RiskEvaluationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _policy = CreateResiliencePolicy();
        }

        private IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy()
        {
            // Política de reintentos
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync<HttpResponseMessage>(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} after {timespan.TotalSeconds}s");
                    }
                );

            // Política de circuit breaker
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync<HttpResponseMessage>(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, duration) =>
                    {
                        _logger.LogError($"Circuit breaker opened for {duration.TotalSeconds}s");
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset");
                    }
                );

            // Política de timeout
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(10)
            );

            // Política de fallback
            var fallbackPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .OrResult(r => !r.IsSuccessStatusCode)
                .FallbackAsync(
                    new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(
                            new RiskEvaluationResponseDto { RiskScore = 0, Status = "FALLBACK" }
                        ))
                    },
                    onFallbackAsync: (context) =>
                    {
                        _logger.LogWarning("Fallback policy activated for risk evaluation");
                        return Task.CompletedTask;
                    }
                );

            // Combinar todas las políticas
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy, fallbackPolicy);
        }

        public async Task<RiskEvaluationResponseDto> GetRiskEvaluationAsync(RiskEvaluationRequestDto request)
        {
            try
            {
                var riskEvaluationUrl = _configuration["ExternalServices:RiskEvaluationUrl"];
                _logger.LogInformation($"Calling risk evaluation service at {riskEvaluationUrl} for customer {request.CustomerEmail}");
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _policy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync(riskEvaluationUrl, jsonContent)
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Risk evaluation service returned status code: {response.StatusCode}");
                    return new RiskEvaluationResponseDto { RiskScore = 0, Status = "ERROR" };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var riskResponse = JsonSerializer.Deserialize<RiskEvaluationResponseDto>(responseContent);

                _logger.LogInformation($"Risk evaluation completed successfully. RiskScore: {riskResponse?.RiskScore}, Status: {riskResponse?.Status}");
                return riskResponse ?? new RiskEvaluationResponseDto { RiskScore = 0, Status = "UNKNOWN" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling risk evaluation service: {ex.Message}");
                return new RiskEvaluationResponseDto { RiskScore = 0, Status = "ERROR" };
            }
        }
    }
}
