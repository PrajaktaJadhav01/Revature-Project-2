using System.Security.Claims;
using CustomerManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace CustomerManagementSystem.Api.Tests.Tests;

public class AuthorizationTests
{
    [Fact]
    public async Task SalesRep_CanOnlyAccessAssignedCustomers()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        var c1 = await env.SeedCustomerAsync(
            name: "Rep1",
            email: "rep1@x.com",
            classification: CustomerClassification.Active,
            assignedRepId: 1,
            accountValue: 1000m);

        var c2 = await env.SeedCustomerAsync(
            name: "Rep2",
            email: "rep2@x.com",
            classification: CustomerClassification.Active,
            assignedRepId: 2,
            accountValue: 1000m);

        var rep1Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "SalesRep"),
            new Claim("assignedRepId", "1")
        }, "test"));

        Assert.True(await env.AuthorizationService.CanAccessCustomerAsync(rep1Principal, c1.CustomerId));
        Assert.False(await env.AuthorizationService.CanAccessCustomerAsync(rep1Principal, c2.CustomerId));
    }

    [Fact]
    public async Task SalesManager_CanAccessAllCustomers()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        var c1 = await env.SeedCustomerAsync(
            name: "Any1",
            email: "any1@x.com",
            classification: CustomerClassification.Active,
            assignedRepId: 1,
            accountValue: 1000m);

        var c2 = await env.SeedCustomerAsync(
            name: "Any2",
            email: "any2@x.com",
            classification: CustomerClassification.Active,
            assignedRepId: 2,
            accountValue: 1000m);

        var managerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "SalesManager")
        }, "test"));

        Assert.True(await env.AuthorizationService.CanAccessCustomerAsync(managerPrincipal, c1.CustomerId));
        Assert.True(await env.AuthorizationService.CanAccessCustomerAsync(managerPrincipal, c2.CustomerId));
    }
}

