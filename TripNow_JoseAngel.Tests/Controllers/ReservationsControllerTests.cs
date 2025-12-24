using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TripNow_JoseAngel.Application.DTOs;
using TripNow_JoseAngel.Application.Interfaces;
using TripNow_JoseAngel.Controllers;
using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;

namespace TripNow_JoseAngel.Tests.Controllers
{
    public class ReservationsControllerTests
    {
        private readonly Mock<IReservationRepository> _mockRepository;
        private readonly Mock<ILogger<ReservationsController>> _mockLogger;

        public ReservationsControllerTests()
        {
            _mockRepository = new Mock<IReservationRepository>();
            _mockLogger = new Mock<ILogger<ReservationsController>>();
        }

        [Fact]
        public async Task CreateReservation_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var controller = new ReservationsController(_mockRepository.Object, _mockLogger.Object);
            var request = new CreateReservationRequestDto
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                IdempotencyKey = "key123"
            };

            var reservation = new Reservation
            {
                Id = 1,
                CustomerEmail = request.CustomerEmail,
                Amount = request.Amount,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                RiskScore = 0,
                IdempotencyKey = request.IdempotencyKey
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
                .ReturnsAsync(reservation);

            // Act
            var result = await controller.CreateReservation(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult.Value);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task CreateReservation_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var controller = new ReservationsController(_mockRepository.Object, _mockLogger.Object);
            var request = new CreateReservationRequestDto
            {
                CustomerEmail = "",
                TripCountry = "US",
                Amount = 1000,
                IdempotencyKey = "key123"
            };

            // Act
            var result = await controller.CreateReservation(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Never);
        }

        [Fact]
        public async Task GetReservationById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var controller = new ReservationsController(_mockRepository.Object, _mockLogger.Object);
            var reservation = new Reservation
            {
                Id = 1,
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key123"
            };

            _mockRepository.Setup(r => r.GetByIdOnlyAsync(1))
                .ReturnsAsync(reservation);

            // Act
            var result = await controller.GetReservationById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(reservation, okResult.Value);
        }

        [Fact]
        public async Task GetReservationById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var controller = new ReservationsController(_mockRepository.Object, _mockLogger.Object);
            
            _mockRepository.Setup(r => r.GetByIdOnlyAsync(999))
                .ReturnsAsync((Reservation)null);

            // Act
            var result = await controller.GetReservationById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        [Fact]
        public async Task GetReservationsByIdempotencyKey_WithValidKey_ShouldReturnOk()
        {
            // Arrange
            var controller = new ReservationsController(_mockRepository.Object, _mockLogger.Object);
            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    CustomerEmail = "test@example.com",
                    TripCountry = "US",
                    Amount = 1000,
                    Status = ReservationStatus.PENDING_RISK_CHECK,
                    IdempotencyKey = "key123"
                }
            };

            _mockRepository.Setup(r => r.GetAllAsync("key123"))
                .ReturnsAsync(reservations);

            // Act
            var result = await controller.GetReservationsByIdempotencyKey("key123");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(reservations, okResult.Value);
        }
    }
}
