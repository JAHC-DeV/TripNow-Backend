using Moq;
using Xunit;
using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;
using TripNow_JoseAngel.Infrastructure.Persistence;
using TripNow_JoseAngel.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TripNow_JoseAngel.Tests.Repositories
{
    public class ReservationRepositoryTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddReservationToDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);
            var reservation = new Reservation
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key123",
                RiskScore = 0.0
            };

            // Act
            var result = await repository.AddAsync(reservation);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("test@example.com", result.CustomerEmail);
            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
        }

        [Fact]
        public async Task GetByIdOnlyAsync_ShouldReturnReservation()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);
            var reservation = new Reservation
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key123",
                RiskScore = 0.0
            };
            var createdReservation = await repository.AddAsync(reservation);

            // Act
            var result = await repository.GetByIdOnlyAsync(createdReservation.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdReservation.Id, result.Id);
            Assert.Equal("test@example.com", result.CustomerEmail);
        }

        [Fact]
        public async Task GetByIdOnlyAsync_ShouldReturnNullForNonExistentId()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);

            // Act
            var result = await repository.GetByIdOnlyAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateReservation()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);
            var reservation = new Reservation
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key123",
                RiskScore = 0.0
            };
            var createdReservation = await repository.AddAsync(reservation);
            var originalCreatedAt = createdReservation.CreatedAt;

            // Act
            createdReservation.Status = ReservationStatus.APPROVED;
            await Task.Delay(100); // Ensure UpdatedAt is different
            var result = await repository.UpdateAsync(createdReservation);

            // Assert
            Assert.Equal(ReservationStatus.APPROVED, result.Status);
            Assert.Equal(originalCreatedAt, result.CreatedAt);
            Assert.True(result.UpdatedAt > originalCreatedAt);
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnReservationsWithStatus()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);
            
            await repository.AddAsync(new Reservation
            {
                CustomerEmail = "test1@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key1",
                RiskScore = 0.0
            });

            await repository.AddAsync(new Reservation
            {
                CustomerEmail = "test2@example.com",
                TripCountry = "US",
                Amount = 2000,
                Status = ReservationStatus.APPROVED,
                IdempotencyKey = "key2",
                RiskScore = 0.0
            });

            // Act
            var result = await repository.GetByStatusAsync(ReservationStatus.PENDING_RISK_CHECK, 10);

            // Assert
            Assert.Single(result);
            Assert.Equal("test1@example.com", result.First().CustomerEmail);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveReservation()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);
            var reservation = new Reservation
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key123",
                RiskScore = 0.0
            };
            var createdReservation = await repository.AddAsync(reservation);

            // Act
            var result = await repository.DeleteAsync(createdReservation.Id, createdReservation.IdempotencyKey);

            // Assert
            Assert.True(result);
            var deletedReservation = await repository.GetByIdOnlyAsync(createdReservation.Id);
            Assert.Null(deletedReservation);
        }
    }
}
