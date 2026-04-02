using CustomerManagementSystem.Infrastructure.Persistence;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CustomerManagementSystem.Application.Analytics.Queries;
using CustomerManagementSystem.Application.Customers.Commands;
using CustomerManagementSystem.Application.Customers.Queries;
using CustomerManagementSystem.Api.Security;

namespace CustomerManagementSystem.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<CustomerAuthorizationService>();

        // CQRS handlers (no MediatR)
        services.AddScoped<CreateCustomerCommandHandler>();
        services.AddScoped<UpdateCustomerCommandHandler>();
        services.AddScoped<DeleteCustomerCommandHandler>();
        services.AddScoped<AddContactCommandHandler>();
        services.AddScoped<UpdateContactCommandHandler>();
        services.AddScoped<DeleteContactCommandHandler>();
        services.AddScoped<AddInteractionCommandHandler>();
        services.AddScoped<UpdateInteractionCommandHandler>();
        services.AddScoped<DeleteInteractionCommandHandler>();
        services.AddScoped<ChangeClassificationCommandHandler>();
        services.AddScoped<AddAddressCommandHandler>();
        services.AddScoped<UpdateAddressCommandHandler>();
        services.AddScoped<DeleteAddressCommandHandler>();
        services.AddScoped<GetCustomerByIdQueryHandler>();
        services.AddScoped<GetAllCustomersQueryHandler>();
        services.AddScoped<GetCustomerAnalyticsQueryHandler>();
        services.AddScoped<GetCustomerInteractionsQueryHandler>();

        return services;
    }
}

