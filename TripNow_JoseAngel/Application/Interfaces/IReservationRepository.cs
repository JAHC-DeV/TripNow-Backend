using TripNow_JoseAngel.Domain.Entities;
using TripNow_JoseAngel.Domain.Enums;

namespace TripNow_JoseAngel.Application.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);
        Task<Reservation?> GetByIdAsync(int id, string idempotencyKey);
        Task<Reservation?> GetByIdOnlyAsync(int id);
        Task<IEnumerable<Reservation>> GetAllAsync(string idempotencyKey);
        Task<Reservation> UpdateAsync(Reservation reservation);
        Task<bool> DeleteAsync(int id, string idempotencyKey);
        Task<IEnumerable<Reservation>> GetByStatusAsync(ReservationStatus status, int count);
    }
}
