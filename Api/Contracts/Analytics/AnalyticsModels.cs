using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Api.Contracts.Analytics;

public class LifetimeValueResponse
{
    public decimal TotalValue { get; set; }
}

public class HealthScoreResponse
{
    public int Score { get; set; }
}

public class SegmentationDistributionResponse
{
    public IDictionary<CustomerSegment, int> Counts { get; set; } = new Dictionary<CustomerSegment, int>();
}

public class ChurnRiskResponse
{
    public int AtRiskCount { get; set; }
    public int TotalCount { get; set; }
}

public class AnalyticsSummaryResponse
{
    public int TotalCustomers { get; set; }
    public int AtRiskCustomers { get; set; }
    public decimal ChurnRiskPct { get; set; }
    public IDictionary<CustomerSegment, int> SegmentationCounts { get; set; } = new Dictionary<CustomerSegment, int>();
    public int ActiveAccounts { get; set; }
    public decimal Revenue { get; set; }
    public bool HasData => TotalCustomers > 0;
}

