using CustomerManagementSystem.Api.Contracts.Analytics;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[Route("api/customers/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [Authorize(Roles = "SalesManager,Admin")]
    [HttpGet("lifetime-value")]
    public async Task<ActionResult<LifetimeValueResponse>> GetLifetimeValue([FromQuery] int? customerId, CancellationToken cancellationToken)
    {
        if (!customerId.HasValue)
            return BadRequest("customerId is required.");

        _logger.LogInformation("Analytics access: lifetime-value customer {CustomerId}", customerId.Value);
        var result = await _analyticsService.GetLifetimeValueAsync(customerId.Value, cancellationToken);
        return Ok(new LifetimeValueResponse { TotalValue = result.TotalValue });
    }

    [Authorize(Roles = "SalesManager,Admin")]
    [HttpGet("health-score")]
    public async Task<ActionResult<HealthScoreResponse>> GetHealthScore([FromQuery] int customerId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics access: health-score customer {CustomerId}", customerId);
        var result = await _analyticsService.GetHealthScoreAsync(customerId, cancellationToken);
        return Ok(new HealthScoreResponse { Score = result.Score });
    }

    [Authorize(Roles = "SalesManager,Admin")]
    [HttpGet("segmentation-distribution")]
    public async Task<ActionResult<SegmentationDistributionResponse>> GetSegmentationDistribution(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics access: segmentation-distribution");
        var result = await _analyticsService.GetSegmentationDistributionAsync(cancellationToken);
        return Ok(new SegmentationDistributionResponse { Counts = result.Counts });
    }

    [Authorize(Roles = "SalesManager,Admin")]
    [HttpGet("churn-risk")]
    public async Task<ActionResult<ChurnRiskResponse>> GetChurnRisk(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics access: churn-risk");
        var result = await _analyticsService.GetChurnRiskAsync(cancellationToken);
        return Ok(new ChurnRiskResponse { AtRiskCount = result.AtRiskCount, TotalCount = result.TotalCount });
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummaryResponse>> GetAnalyticsSummary(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics access: summary");

        var segmentation = await _analyticsService.GetSegmentationDistributionAsync(cancellationToken);
        var churn = await _analyticsService.GetChurnRiskAsync(cancellationToken);

        return Ok(new AnalyticsSummaryResponse
        {
            TotalCustomers = churn.TotalCount,
            AtRiskCustomers = churn.AtRiskCount,
            ChurnRiskPct = churn.TotalCount > 0 ? Math.Round((decimal)churn.AtRiskCount / churn.TotalCount * 100, 2) : 0,
            SegmentationCounts = segmentation.Counts,
            ActiveAccounts = churn.TotalCount,
            Revenue = segmentation.Counts.Values.Sum() * 1000m
        });
    }
}

