using Xunit;
using Microsoft.EntityFrameworkCore;
using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;
using TripNow_JoseAngel.Infrastructure.Persistence;
using TripNow_JoseAngel.Infrastructure.Repositories;

namespace TripNow_JoseAngel.Tests.Integration
{
    public class ReservationIntegrationTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ReservationLifecycle_ShouldCompleteSuccessfully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);

            // Act - Create reservation
            var newReservation = new Reservation
            {
                CustomerEmail = "integration@example.com",
                TripCountry = "CU",
                Amount = 500,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                RiskScore = 0.0,
                IdempotencyKey = "integration-key-001"
            };

            var createdReservation = await repository.AddAsync(newReservation);

            // Assert - Reservation was created
            Assert.NotNull(createdReservation);
            Assert.True(createdReservation.Id > 0);
            Assert.Equal(ReservationStatus.PENDING_RISK_CHECK, createdReservation.Status);

            // Act - Retrieve by ID
            var retrievedReservation = await repository.GetByIdOnlyAsync(createdReservation.Id);

            // Assert - Reservation can be retrieved
            Assert.NotNull(retrievedReservation);
            Assert.Equal(createdReservation.Id, retrievedReservation.Id);
            Assert.Equal("integration@example.com", retrievedReservation.CustomerEmail);

            // Act - Get by idempotency key
            var reservationsByKey = await repository.GetAllAsync("integration-key-001");

            // Assert - Can retrieve by idempotency key
            Assert.NotEmpty(reservationsByKey);
            Assert.Single(reservationsByKey);

            // Act - Update reservation (simulate risk evaluation)
            createdReservation.Status = ReservationStatus.APPROVED;
            createdReservation.RiskScore = 25.5;
            var updatedReservation = await repository.UpdateAsync(createdReservation);

            // Assert - Reservation was updated
            Assert.Equal(ReservationStatus.APPROVED, updatedReservation.Status);
            Assert.Equal(25.5, updatedReservation.RiskScore);
            Assert.True(updatedReservation.UpdatedAt > updatedReservation.CreatedAt);

            // Act - Get by status
            var approvedReservations = await repository.GetByStatusAsync(ReservationStatus.APPROVED, 10);

            // Assert - Can retrieve approved reservations
            Assert.NotEmpty(approvedReservations);
            Assert.Contains(approvedReservations, r => r.Id == createdReservation.Id);

            // Act - Delete reservation
            var deleteResult = await repository.DeleteAsync(createdReservation.Id, createdReservation.IdempotencyKey);

            // Assert - Reservation was deleted
            Assert.True(deleteResult);
            var deletedReservation = await repository.GetByIdOnlyAsync(createdReservation.Id);
            Assert.Null(deletedReservation);
        }

        [Fact]
        public async Task MultipleReservations_ShouldBeHandledCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);

            // Act - Create multiple reservations
            var reservation1 = await repository.AddAsync(new Reservation
            {
                CustomerEmail = "user1@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key1",
                RiskScore = 0.0
            });

            var reservation2 = await repository.AddAsync(new Reservation
            {
                CustomerEmail = "user2@example.com",
                TripCountry = "ES",
                Amount = 2000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "key2",
                RiskScore = 0.0
            });

            var reservation3 = await repository.AddAsync(new Reservation
            {
                CustomerEmail = "user3@example.com",
                TripCountry = "FR",
                Amount = 1500,
                Status = ReservationStatus.APPROVED,
                IdempotencyKey = "key3",
                RiskScore = 0.0
            });

            // Assert - All reservations created
            Assert.True(reservation1.Id > 0);
            Assert.True(reservation2.Id > 0);
            Assert.True(reservation3.Id > 0);

            // Act - Get pending reservations
            var pendingReservations = await repository.GetByStatusAsync(ReservationStatus.PENDING_RISK_CHECK, 10);

            // Assert - Correct count of pending
            Assert.Equal(2, pendingReservations.Count());

            // Act - Get approved reservations
            var approvedReservations = await repository.GetByStatusAsync(ReservationStatus.APPROVED, 10);

            // Assert - Correct count of approved
            Assert.Single(approvedReservations);

            // Act - Get by idempotency key
            var reservationsByKey1 = await repository.GetAllAsync("key1");

            // Assert - Correct filtering
            Assert.Single(reservationsByKey1);
            Assert.Equal("user1@example.com", reservationsByKey1.First().CustomerEmail);
        }

        [Fact]
        public async Task ReservationStatusUpdate_ShouldPreserveCreatedAtTimestamp()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new ReservationRepository(context);

            // Act - Create reservation
            var reservation = await repository.AddAsync(new Reservation
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000,
                Status = ReservationStatus.PENDING_RISK_CHECK,
                IdempotencyKey = "test-key",
                RiskScore = 0.0
            });

            var originalCreatedAt = reservation.CreatedAt;
            await Task.Delay(100); // Small delay to ensure time difference

            // Act - Update reservation
            reservation.Status = ReservationStatus.REJECTED;
            reservation.RiskScore = 85.0;
            var updatedReservation = await repository.UpdateAsync(reservation);

            // Assert - CreatedAt should not change, UpdatedAt should
            Assert.Equal(originalCreatedAt, updatedReservation.CreatedAt);
            Assert.True(updatedReservation.UpdatedAt > originalCreatedAt);
        }
    }
}
