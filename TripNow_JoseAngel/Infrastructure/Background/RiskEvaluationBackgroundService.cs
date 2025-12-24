using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TripNow_JoseAngel.Application.DTOs;
using TripNow_JoseAngel.Application.Interfaces;
using TripNow_JoseAngel.Domain.Enums;
using TripNow_JoseAngel.Infrastructure.Persistence;

namespace TripNow_JoseAngel.Infrastructure.Background
{
    public class RiskEvaluationBackgroundService : BackgroundService
    {
        private readonly ILogger<RiskEvaluationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private TimeSpan _interval = TimeSpan.FromMinutes(0.2);

        public RiskEvaluationBackgroundService(
            ILogger<RiskEvaluationBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Risk Evaluation Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingReservations(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in Risk Evaluation Background Service: {ex.Message}");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Risk Evaluation Background Service stopped");
        }

        private async Task ProcessPendingReservations(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var riskEvaluationService = scope.ServiceProvider.GetRequiredService<IRiskEvaluation>();
                    var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();

                    _logger.LogInformation("Processing pending reservations");

                    // Get pending reservations
                    var pendingReservations = await reservationRepository.GetByStatusAsync(
                        ReservationStatus.PENDING_RISK_CHECK, 
                        int.MaxValue);

                    if (pendingReservations == null || !pendingReservations.Any())
                    {
                        _logger.LogInformation("No pending reservations to process");
                        return;
                    }

                    _logger.LogInformation($"Found {pendingReservations.Count()} pending reservations to process");

                    foreach (var reservation in pendingReservations)
                    {
                        try
                        {
                            _logger.LogInformation($"Processing reservation ID: {reservation.Id}");

                            // Assume we have trip country info stored or accessible; using a placeholder here
                            // Evaluate risk
                            var riskRequest = new RiskEvaluationRequestDto
                            {
                                CustomerEmail = reservation.CustomerEmail,
                                TripCountry = reservation.TripCountry ?? "NA",
                                Amount = reservation.Amount
                            };

                            var riskResponse = await riskEvaluationService.GetRiskEvaluationAsync(riskRequest);

                            // Update reservation with risk score and status
                            reservation.RiskScore = riskResponse.RiskScore;
                            reservation.Status = riskResponse.Status switch
                            {
                                "APPROVED" => ReservationStatus.APPROVED,
                                "REJECTED" => ReservationStatus.REJECTED,                                
                                _ => ReservationStatus.PENDING_RISK_CHECK
                            };

                            // Save changes
                            await reservationRepository.UpdateAsync(reservation);
                            _logger.LogInformation($"Reservation ID {reservation.Id} updated with status {reservation.Status} and risk score {riskResponse.RiskScore}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing reservation ID {reservation.Id}: {ex.Message}");
                            // Continue with the next reservation without throwing
                        }
                    }

                    _logger.LogInformation("Completed processing pending reservations");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing database in background service: {ex.Message}");
                // Don't throw, just log the error
            }
        }
    }
}
