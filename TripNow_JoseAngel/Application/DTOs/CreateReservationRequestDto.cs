namespace TripNow_JoseAngel.Application.DTOs
{
    public class CreateReservationRequestDto
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public string TripCountry { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
