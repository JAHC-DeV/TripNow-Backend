using TripNow_JoseAngel.Application.DTOs;

namespace TripNow_JoseAngel.Application.Interfaces
{
    public interface IRiskEvaluation
    {
        Task<RiskEvaluationResponseDto> GetRiskEvaluationAsync(RiskEvaluationRequestDto request);
    }
}
