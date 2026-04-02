using CustomerManagementSystem.Api.Security;
using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Persistence;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CustomerManagementSystem.Api.Tests;

public sealed class TestEnvironment : IDisposable
{
    public AppDbContext DbContext { get; }
    public IDistributedCache Cache { get; }
    public CustomerService CustomerService { get; }
    public AnalyticsService AnalyticsService { get; }
    public CustomerAuthorizationService AuthorizationService { get; }

    public TestEnvironment(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        DbContext = new AppDbContext(options);

        Cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

        CustomerService = new CustomerService(DbContext, Cache, NullLogger<CustomerService>.Instance);
        AnalyticsService = new AnalyticsService(DbContext, Cache, NullLogger<AnalyticsService>.Instance);
        AuthorizationService = new CustomerAuthorizationService(CustomerService);
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }

    public async Task<Customer> SeedCustomerAsync(
        string name,
        string email,
        CustomerClassification classification,
        int? assignedRepId,
        decimal accountValue,
        CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            CustomerName = name,
            Email = email,
            Phone = "555-555-5555",
            Website = "https://example.com",
            Industry = "Tech",
            CompanySize = "1-10",
            Classification = classification,
            Type = CustomerType.Business,
            Segment = CustomerSegment.Enterprise,
            AccountValue = accountValue,
            AssignedSalesRepId = assignedRepId
        };

        return await CustomerService.CreateCustomerAsync(customer, cancellationToken);
    }
}

