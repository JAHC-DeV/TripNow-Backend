namespace TripNow_JoseAngel.Application.DTOs
{
    public class RiskEvaluationRequestDto
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public string TripCountry { get; set; } = string.Empty;
        public double Amount { get; set; }
    }
}
