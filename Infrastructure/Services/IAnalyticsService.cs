using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Infrastructure.Services;

public record LifetimeValueResult(decimal TotalValue);

public record HealthScoreResult(int Score);

public record SegmentationDistributionResult(IDictionary<CustomerSegment, int> Counts);

public record ChurnRiskResult(int AtRiskCount, int TotalCount);

public interface IAnalyticsService
{
    Task<LifetimeValueResult> GetLifetimeValueAsync(int customerId, CancellationToken cancellationToken = default);
    Task<HealthScoreResult> GetHealthScoreAsync(int customerId, CancellationToken cancellationToken = default);
    Task<SegmentationDistributionResult> GetSegmentationDistributionAsync(CancellationToken cancellationToken = default);
    Task<ChurnRiskResult> GetChurnRiskAsync(CancellationToken cancellationToken = default);
}

