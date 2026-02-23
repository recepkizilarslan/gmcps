using Gmcps.Domain.Models;

namespace Gmcps.Tests;

public class RiskScoreTests
{
    [Theory]
    [InlineData(5, 3, 2, 10, 61.0)]  // 5*10 + 3*3 + 2*1 = 61
    [InlineData(0, 0, 0, 0, 0.0)]
    [InlineData(1, 0, 0, 0, 10.0)]
    [InlineData(0, 1, 0, 0, 3.0)]
    [InlineData(0, 0, 1, 0, 1.0)]
    [InlineData(10, 20, 30, 100, 190.0)] // 10*10 + 20*3 + 30*1 = 190
    public void ComputeRiskScore_VariousSummaries_ReturnsExpectedScore(
        int high, int medium, int low, int log, double expectedScore)
    {
        var summary = new ReportSummary(high, medium, low, log);
        var score = ComputeRiskScore(summary);
        Assert.Equal(expectedScore, score);
    }

    // Mirror the calculation from AnalyticsTools
    private static double ComputeRiskScore(ReportSummary summary) =>
        summary.High * 10.0 + summary.Medium * 3.0 + summary.Low * 1.0;
}
