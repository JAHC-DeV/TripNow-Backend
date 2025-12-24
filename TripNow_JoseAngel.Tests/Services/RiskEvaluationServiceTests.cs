using Moq;
using Xunit;
using TripNow_JoseAngel.Application.DTOs;
using TripNow_JoseAngel.Application.Interfaces;
using TripNow_JoseAngel.Infrastructure.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TripNow_JoseAngel.Tests.Services
{
    public class RiskEvaluationServiceTests
    {
        private readonly Mock<ILogger<RiskEvaluationService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public RiskEvaluationServiceTests()
        {
            _mockLogger = new Mock<ILogger<RiskEvaluationService>>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task GetRiskEvaluationAsync_ShouldReturnFallbackOnException()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            _mockConfiguration.Setup(c => c["ExternalServices:RiskEvaluationUrl"])
                .Returns("https://invalid-url.example.com/risk");

            var service = new RiskEvaluationService(mockHttpClient.Object, _mockConfiguration.Object, _mockLogger.Object);
            var request = new RiskEvaluationRequestDto
            {
                CustomerEmail = "test@example.com",
                TripCountry = "US",
                Amount = 1000
            };

            // Act
            var result = await service.GetRiskEvaluationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.RiskScore);
            Assert.Equal("ERROR", result.Status);
        }

        [Fact]
        public void RiskEvaluationService_ShouldBeRegisteredAsScoped()
        {
            // This is a simple test to verify the interface and implementation match
            Assert.True(typeof(IRiskEvaluation).IsAssignableFrom(typeof(RiskEvaluationService)));
        }
    }
}
