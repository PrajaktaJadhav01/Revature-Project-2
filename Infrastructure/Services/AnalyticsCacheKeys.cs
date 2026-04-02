using CustomerManagementSystem.Domain.Entities;

namespace CustomerManagementSystem.Infrastructure.Services;

public static class AnalyticsCacheKeys
{
    public static string LifetimeValue(int customerId) => $"analytics:lifetime:{customerId}";
    public static string HealthScore(int customerId) => $"analytics:health:{customerId}";
    public static string SegmentationDistribution() => "analytics:segmentation-distribution";
    public static string ChurnRisk() => "analytics:churn-risk";
}
