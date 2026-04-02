using System.Text.Json;
using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CustomerManagementSystem.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(AppDbContext dbContext, IDistributedCache cache, ILogger<AnalyticsService> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LifetimeValueResult> GetLifetimeValueAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var cacheKey = AnalyticsCacheKeys.LifetimeValue(customerId);
        var cached = await GetCachedAsync<LifetimeValueResult>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        if (customer is null) return new LifetimeValueResult(0m);

        var interactionsCount = await _dbContext.Interactions
            .AsNoTracking()
            .CountAsync(i => i.CustomerId == customerId, cancellationToken);

        // Simple lifetime-value approximation: base account value + interaction-derived value.
        var lifetimeValue = customer.AccountValue + (interactionsCount * 1000m);

        var result = new LifetimeValueResult(lifetimeValue);
        await SetCachedAsync(cacheKey, result, cancellationToken);
        return result;
    }

    public async Task<HealthScoreResult> GetHealthScoreAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var cacheKey = AnalyticsCacheKeys.HealthScore(customerId);
        var cached = await GetCachedAsync<HealthScoreResult>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var customer = await _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        if (customer is null) return new HealthScoreResult(0);

        var latestInteraction = await _dbContext.Interactions
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InteractionDate)
            .Select(i => (DateTime?)i.InteractionDate)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var daysSince = latestInteraction.HasValue ? (now - latestInteraction.Value).Days : int.MaxValue;

        var recencyScore =
            daysSince <= 7 ? 40 :
            daysSince <= 30 ? 30 :
            daysSince <= 90 ? 20 :
            daysSince <= 180 ? 10 :
            0;

        // Map account value to 0..30 (roughly $10k -> 20; clamp at 30).
        var accountScore = (int)Math.Min(30, Math.Round(customer.AccountValue / 10000m));

        // Map classification to 0..30.
        var classificationScore = customer.Classification switch
        {
            CustomerClassification.Prospect => 10,
            CustomerClassification.Active => 25,
            CustomerClassification.Inactive => 15,
            CustomerClassification.VIP => 30,
            CustomerClassification.AtRisk => 0,
            _ => 0
        };

        var score = recencyScore + accountScore + classificationScore;
        score = Math.Clamp(score, 0, 100);

        var result = new HealthScoreResult(score);
        await SetCachedAsync(cacheKey, result, cancellationToken);
        return result;
    }

    public async Task<SegmentationDistributionResult> GetSegmentationDistributionAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = AnalyticsCacheKeys.SegmentationDistribution();
        var cached = await GetCachedAsync<SegmentationDistributionResult>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var counts = await _dbContext.Customers
            .AsNoTracking()
            .GroupBy(c => c.Segment)
            .Select(g => new { Segment = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new SegmentationDistributionResult(
            counts.ToDictionary(x => x.Segment, x => x.Count)
        );

        await SetCachedAsync(cacheKey, result, cancellationToken);
        return result;
    }

    public async Task<ChurnRiskResult> GetChurnRiskAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = AnalyticsCacheKeys.ChurnRisk();
        var cached = await GetCachedAsync<ChurnRiskResult>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var totalCount = await _dbContext.Customers.AsNoTracking().CountAsync(cancellationToken);
        var atRiskCount = await _dbContext.Customers.AsNoTracking().CountAsync(c => c.Classification == CustomerClassification.AtRisk, cancellationToken);

        var result = new ChurnRiskResult(atRiskCount, totalCount);
        await SetCachedAsync(cacheKey, result, cancellationToken);
        return result;
    }

    private async Task<T?> GetCachedAsync<T>(string key, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for key {CacheKey}. Falling back to database.", key);
            return null;
        }
    }

    private async Task SetCachedAsync<T>(string key, T value, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for key {CacheKey}. Continuing without cache.", key);
        }
    }
}

