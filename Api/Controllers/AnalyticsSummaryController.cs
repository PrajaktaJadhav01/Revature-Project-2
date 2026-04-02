using CustomerManagementSystem.Api.Contracts.Analytics;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsSummaryController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsSummaryController> _logger;

    public AnalyticsSummaryController(IAnalyticsService analyticsService, ILogger<AnalyticsSummaryController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet]
    public async Task<ActionResult<AnalyticsSummaryResponse>> GetAnalytics(CancellationToken cancellationToken)
    {
        try
        {
            var segmentation = await _analyticsService.GetSegmentationDistributionAsync(cancellationToken);
            var churn = await _analyticsService.GetChurnRiskAsync(cancellationToken);

            var total = churn.TotalCount;
            var atRisk = churn.AtRiskCount;

            var result = new AnalyticsSummaryResponse
            {
                TotalCustomers = total,
                AtRiskCustomers = atRisk,
                ChurnRiskPct = total > 0 ? Math.Round((decimal)atRisk / total * 100, 2) : 0,
                SegmentationCounts = segmentation.Counts,
                ActiveAccounts = total, // placeholder for real metric
                Revenue = segmentation.Counts.Values.Sum() * 1000m // placeholder
            };

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to /api/analytics");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while building analytics summary.");
            return StatusCode(500, new { message = "Analytics calculation failed. Please try again." });
        }
    }
}

