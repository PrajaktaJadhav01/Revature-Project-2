using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;

namespace CustomerManagementSystem.Application.Analytics.Queries;

public class GetCustomerAnalyticsQuery
{
    public int CustomerId { get; set; }
}

public class CustomerAnalyticsResult
{
    public decimal LifetimeValue { get; set; }
    public int HealthScore { get; set; }
    public IDictionary<CustomerSegment, int> SegmentationDistribution { get; set; } = new Dictionary<CustomerSegment, int>();
    public int AtRiskCount { get; set; }
    public int TotalCount { get; set; }
}

public class GetCustomerAnalyticsQueryHandler
{
    private readonly IAnalyticsService _analyticsService;

    public GetCustomerAnalyticsQueryHandler(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<CustomerAnalyticsResult> HandleAsync(GetCustomerAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        var lifetime = await _analyticsService.GetLifetimeValueAsync(query.CustomerId, cancellationToken);
        var health = await _analyticsService.GetHealthScoreAsync(query.CustomerId, cancellationToken);
        var segmentation = await _analyticsService.GetSegmentationDistributionAsync(cancellationToken);
        var churn = await _analyticsService.GetChurnRiskAsync(cancellationToken);

        return new CustomerAnalyticsResult
        {
            LifetimeValue = lifetime.TotalValue,
            HealthScore = health.Score,
            SegmentationDistribution = segmentation.Counts,
            AtRiskCount = churn.AtRiskCount,
            TotalCount = churn.TotalCount
        };
    }
}

