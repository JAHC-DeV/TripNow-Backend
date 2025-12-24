using Microsoft.AspNetCore.Mvc;
using TripNow_JoseAngel.Application.DTOs;
using TripNow_JoseAngel.Application.Interfaces;
using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;

namespace TripNow_JoseAngel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationRepository _reservationRepository;        
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            IReservationRepository reservationRepository,
            ILogger<ReservationsController> logger)
        {
            _reservationRepository = reservationRepository;            
            _logger = logger;
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult<Reservation>> CreateReservation(CreateReservationRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Creating new reservation for customer {request.CustomerEmail}");

                // Validate input
                if (string.IsNullOrWhiteSpace(request.CustomerEmail) ||
                    string.IsNullOrWhiteSpace(request.TripCountry) ||
                    request.Amount <= 0 ||
                    string.IsNullOrWhiteSpace(request.IdempotencyKey))
                {
                    _logger.LogWarning("Invalid reservation request data");
                    return BadRequest("Invalid reservation data");
                }

                // Evaluate risk
                _logger.LogInformation($"Evaluating risk for reservation");
                var riskEvaluationRequest = new RiskEvaluationRequestDto
                {
                    CustomerEmail = request.CustomerEmail,
                    TripCountry = request.TripCountry,
                    Amount = request.Amount
                };

                // var riskEvaluationResponse = await _riskEvaluation.GetRiskEvaluationAsync(riskEvaluationRequest);
                // _logger.LogInformation($"Risk evaluation completed with score {riskEvaluationResponse.RiskScore}");

                // // Determine reservation status based on risk
                // ReservationStatus status = riskEvaluationResponse.Status switch
                // {
                //     "APPROVED" => ReservationStatus.APPROVED,
                //     "REJECTED" => ReservationStatus.REJECTED,
                //     _ => ReservationStatus.PENDING_RISK_CHECK
                // };

                // Create reservation
                var reservation = new Reservation
                {
                    CustomerEmail = request.CustomerEmail,
                    Amount = request.Amount!,
                    Status = ReservationStatus.PENDING_RISK_CHECK,
                    RiskScore = 0,
                    IdempotencyKey = request.IdempotencyKey
                };

                var createdReservation = await _reservationRepository.AddAsync(reservation);
                _logger.LogInformation($"Reservation created successfully with ID {createdReservation.Id}");

                return CreatedAtAction(nameof(CreateReservation), new { id = createdReservation.Id }, createdReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating reservation: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the reservation");
            }
        }

        [HttpGet]
        [Route("by-idempotency-key/{idempotencyKey}")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservationsByIdempotencyKey(string idempotencyKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    _logger.LogWarning("Invalid idempotency key provided");
                    return BadRequest("Idempotency key is required");
                }

                _logger.LogInformation($"Fetching reservations for idempotency key: {idempotencyKey}");
                var reservations = await _reservationRepository.GetAllAsync(idempotencyKey);

                if (reservations == null || !reservations.Any())
                {
                    _logger.LogInformation($"No reservations found for idempotency key: {idempotencyKey}");
                    return NotFound("No reservations found for the provided idempotency key");
                }

                _logger.LogInformation($"Found {reservations.Count()} reservations for idempotency key: {idempotencyKey}");
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching reservations: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching reservations");
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Reservation>> GetReservationById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"Invalid reservation ID provided: {id}");
                    return BadRequest("Invalid reservation ID");
                }

                _logger.LogInformation($"Fetching reservation with ID: {id}");
                var reservation = await _reservationRepository.GetByIdOnlyAsync(id);

                if (reservation == null)
                {
                    _logger.LogInformation($"Reservation not found with ID: {id}");
                    return NotFound($"Reservation with ID {id} not found");
                }

                _logger.LogInformation($"Reservation retrieved successfully with ID: {id}");
                return Ok(reservation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching reservation: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching the reservation");
            }
        }
    }
}
