using System.Text.Json.Serialization;

namespace TripNow_JoseAngel.Application.DTOs
{
    public class RiskEvaluationResponseDto
    {
        [JsonPropertyName("riskScore")]
        public double RiskScore { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
