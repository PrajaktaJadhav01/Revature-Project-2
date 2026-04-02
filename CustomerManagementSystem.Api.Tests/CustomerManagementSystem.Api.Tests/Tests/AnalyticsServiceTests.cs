using CustomerManagementSystem.Domain.Entities;
using Xunit;

namespace CustomerManagementSystem.Api.Tests.Tests;

public class AnalyticsServiceTests
{
    [Fact]
    public async Task HealthScore_UsesRecencyAccountValueAndClassification()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        var customer = await env.SeedCustomerAsync(
            name: "VIPCo",
            email: "vip@vipco.com",
            classification: CustomerClassification.VIP,
            assignedRepId: 1,
            accountValue: 20000m);

        await env.CustomerService.AddInteractionAsync(new Interaction
        {
            CustomerId = customer.CustomerId,
            InteractionDate = DateTime.UtcNow.AddHours(-1),
            Type = InteractionType.Meeting,
            Summary = "Kickoff"
        });

        var result = await env.AnalyticsService.GetHealthScoreAsync(customer.CustomerId);

        // recencyScore=40 (<=7 days), accountScore=round(20000/10000)=2, classificationScore=VIP=30 => 72
        Assert.Equal(72, result.Score);
    }

    [Fact]
    public async Task AnalyticsCaching_StaysStaleUntilCustomerUpdateInvalidates()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        var customer = await env.SeedCustomerAsync(
            name: "CacheCo",
            email: "cache@cacheco.com",
            classification: CustomerClassification.VIP,
            assignedRepId: 1,
            accountValue: 20000m);

        var interaction = new Interaction
        {
            CustomerId = customer.CustomerId,
            InteractionDate = DateTime.UtcNow.AddHours(-1),
            Type = InteractionType.Meeting,
            Summary = "Initial"
        };

        env.DbContext.Interactions.Add(interaction);
        await env.DbContext.SaveChangesAsync();

        var score1 = await env.AnalyticsService.GetHealthScoreAsync(customer.CustomerId);
        Assert.Equal(72, score1.Score);

        // Mutate DB directly without invalidating cache: score should be stale.
        interaction.InteractionDate = DateTime.UtcNow.AddDays(-60); // recencyScore=20
        await env.DbContext.SaveChangesAsync();

        var score2 = await env.AnalyticsService.GetHealthScoreAsync(customer.CustomerId);
        Assert.Equal(72, score2.Score); // returned from cache

        // Now update customer (CustomerService invalidates cache). Score should recalc with new AccountValue.
        customer.AccountValue = 50000m; // accountScore=round(5)=5, recencyScore=20, VIP=30 => 55
        var updatedCustomer = await env.CustomerService.UpdateCustomerAsync(customer);

        Assert.NotNull(updatedCustomer);

        var score3 = await env.AnalyticsService.GetHealthScoreAsync(customer.CustomerId);
        Assert.Equal(55, score3.Score);
    }
}

