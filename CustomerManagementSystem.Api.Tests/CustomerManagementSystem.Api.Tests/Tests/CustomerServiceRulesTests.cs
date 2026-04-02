using CustomerManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CustomerManagementSystem.Api.Tests.Tests;

public class CustomerServiceRulesTests
{
    [Fact]
    public async Task CreateCustomer_DuplicateEmailAndName_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        await env.SeedCustomerAsync(
            name: "Acme",
            email: "a@acme.com",
            classification: CustomerClassification.Active,
            assignedRepId: 1,
            accountValue: 10000m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await env.CustomerService.CreateCustomerAsync(new Customer
            {
                CustomerName = "Acme",
                Email = "a@acme.com",
                Phone = "555-555-5555",
                Website = "https://example.com",
                Industry = "Tech",
                CompanySize = "1-10",
                Classification = CustomerClassification.Active,
                Type = CustomerType.Business,
                Segment = CustomerSegment.Enterprise,
                AccountValue = 20000m,
                AssignedSalesRepId = 1
            });
        });

        Assert.Contains("Duplicate customer", ex.Message);
    }

    [Fact]
    public async Task AddContact_AllowsSinglePrimaryContact()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        var customer = await env.SeedCustomerAsync(
            name: "Beta",
            email: "b@beta.com",
            classification: CustomerClassification.Active,
            assignedRepId: 1,
            accountValue: 5000m);

        await env.CustomerService.AddContactAsync(new Contact
        {
            CustomerId = customer.CustomerId,
            FirstName = "A",
            LastName = "One",
            Email = "a.one@beta.com",
            Phone = "555-555-5555",
            IsPrimary = true
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await env.CustomerService.AddContactAsync(new Contact
            {
                CustomerId = customer.CustomerId,
                FirstName = "B",
                LastName = "Two",
                Email = "b.two@beta.com",
                Phone = "555-555-5555",
                IsPrimary = true
            });
        });

        Assert.Contains("primary contact", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateCustomer_InvalidClassification_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        using var env = new TestEnvironment(dbName);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await env.CustomerService.CreateCustomerAsync(new Customer
            {
                CustomerName = "Gamma",
                Email = "g@gamma.com",
                Phone = "555-555-5555",
                Website = "https://example.com",
                Industry = "Tech",
                CompanySize = "1-10",
                Classification = (CustomerClassification)999,
                Type = CustomerType.Business,
                Segment = CustomerSegment.Enterprise,
                AccountValue = 10000m,
                AssignedSalesRepId = 1
            });
        });
    }
}

