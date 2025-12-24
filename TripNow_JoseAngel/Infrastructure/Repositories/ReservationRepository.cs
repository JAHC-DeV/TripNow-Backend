using Microsoft.EntityFrameworkCore;
using TripNow_JoseAngel.Application.Interfaces;
using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;
using TripNow_JoseAngel.Infrastructure.Persistence;

namespace TripNow_JoseAngel.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation> AddAsync(Reservation reservation)
        {
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation?> GetByIdAsync(int id, string idempotencyKey)
        {
            return await _context.Reservations
                .Where(r => r.Id == id && r.IdempotencyKey == idempotencyKey)
                .FirstOrDefaultAsync();
        }

        public async Task<Reservation?> GetByIdOnlyAsync(int id)
        {
            return await _context.Reservations
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync(string idempotencyKey)
        {
            return await _context.Reservations
                .Where(r => r.IdempotencyKey == idempotencyKey)
                .ToListAsync();
        }

        public async Task<Reservation> UpdateAsync(Reservation reservation)
        {
            reservation.UpdatedAt = DateTime.UtcNow;
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<bool> DeleteAsync(int id, string idempotencyKey)
        {
            var reservation = await _context.Reservations
                .Where(r => r.Id == id && r.IdempotencyKey == idempotencyKey)
                .FirstOrDefaultAsync();

            if (reservation == null)
                return false;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Reservation>> GetByStatusAsync(ReservationStatus status, int count)
        {
            return await _context.Reservations
                .Where(r => r.Status == status)
                .Take(count)
                .ToListAsync();
        }
    }
}
