using System.Diagnostics;
using CustomerManagementSystem.Api.Contracts.Customers;
using CustomerManagementSystem.Api.Security;
using CustomerManagementSystem.Application.Customers.Commands;
using CustomerManagementSystem.Application.Customers.Queries;
using CustomerManagementSystem.Domain.Entities;
using CustomerManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CreateCustomerCommandHandler _createCustomerHandler;
    private readonly UpdateCustomerCommandHandler _updateCustomerHandler;
    private readonly DeleteCustomerCommandHandler _deleteCustomerHandler;
    private readonly GetCustomerByIdQueryHandler _getCustomerByIdHandler;
    private readonly GetAllCustomersQueryHandler _getAllCustomersQueryHandler;
    private readonly ChangeClassificationCommandHandler _changeClassificationHandler;
    private readonly CustomerAuthorizationService _authorizationService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        CreateCustomerCommandHandler createCustomerHandler,
        UpdateCustomerCommandHandler updateCustomerHandler,
        DeleteCustomerCommandHandler deleteCustomerHandler,
        GetCustomerByIdQueryHandler getCustomerByIdHandler,
        GetAllCustomersQueryHandler getAllCustomersQueryHandler,
        ChangeClassificationCommandHandler changeClassificationHandler,
        CustomerAuthorizationService authorizationService,
        ICustomerService customerService,
        ILogger<CustomersController> logger)
    {
        _createCustomerHandler = createCustomerHandler;
        _updateCustomerHandler = updateCustomerHandler;
        _deleteCustomerHandler = deleteCustomerHandler;
        _getCustomerByIdHandler = getCustomerByIdHandler;
        _getAllCustomersQueryHandler = getAllCustomersQueryHandler;
        _changeClassificationHandler = changeClassificationHandler;
        _authorizationService = authorizationService;
        _customerService = customerService;
        _logger = logger;
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet]
    public async Task<ActionResult<CustomersPageResponse>> GetCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Incoming request /api/customers pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);

        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("Invalid pagination parameters.");

        int? assignedSalesRepId = null;
        var role = User.GetRole();
        if (string.Equals(role, "SalesRep", StringComparison.OrdinalIgnoreCase))
            assignedSalesRepId = User.GetAssignedRepId();

        _logger.LogDebug("User role={Role}, assignedSalesRepId={AssignedSalesRepId}", role, assignedSalesRepId);

        try
        {
            var result = await _getAllCustomersQueryHandler.HandleAsync(new GetAllCustomersQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                AssignedSalesRepId = assignedSalesRepId
            }, cancellationToken);

            var customers = result.Items.Select(MapCustomerToResponseBasic).ToArray();
            _logger.LogInformation("/api/customers query returned {Count} rows", customers.Length);

            if (customers.Length == 0)
            {
                _logger.LogWarning("No customers found in DB; returning sample response.");

                var sample = MapCustomerToResponseBasic(new Customer
                {
                    CustomerId = 0,
                    CustomerName = "Sample Customer",
                    Email = "sample@company.com",
                    Phone = "000-000-0000",
                    Classification = CustomerClassification.Prospect,
                    Type = CustomerType.Business,
                    Segment = CustomerSegment.SMB,
                    AccountValue = 1000m,
                    AssignedSalesRepId = assignedSalesRepId ?? 0,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                });

                return Ok(new CustomersPageResponse
                {
                    Items = new[] { sample },
                    TotalCount = 1
                });
            }

            return Ok(new CustomersPageResponse
            {
                Items = customers,
                TotalCount = result.TotalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customers for page {PageNumber} size {PageSize}", pageNumber, pageSize);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet("{customerId:int}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById([FromRoute] int customerId, CancellationToken cancellationToken = default)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to access customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null)
            return NotFound();

        return Ok(MapCustomerToResponse(customer));
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var role = User.GetRole();
        if (string.Equals(role, "SalesRep", StringComparison.OrdinalIgnoreCase))
        {
            var assignedRepId = User.GetAssignedRepId();
            if (request.AssignedSalesRepId != assignedRepId)
                return Forbid();
        }

        var command = new CreateCustomerCommand
        {
            CustomerName = request.CustomerName,
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            Industry = request.Industry,
            CompanySize = request.CompanySize,
            Classification = request.Classification,
            Type = request.Type,
            Segment = request.Segment,
            AccountValue = request.AccountValue,
            AssignedSalesRepId = request.AssignedSalesRepId
        };

        var customer = await _createCustomerHandler.HandleAsync(command, cancellationToken);
        stopwatch.Stop();
        _logger.LogInformation("CreateCustomer completed in {ElapsedMilliseconds}ms for {CustomerName}", stopwatch.ElapsedMilliseconds, request.CustomerName);
        return CreatedAtAction(nameof(GetCustomerById), new { customerId = customer.CustomerId }, MapCustomerToResponse(customer));
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPut("{customerId:int}")]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer([FromRoute] int customerId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to update customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        var role = User.GetRole();
        if (string.Equals(role, "SalesRep", StringComparison.OrdinalIgnoreCase))
        {
            var assignedRepId = User.GetAssignedRepId();
            if (request.AssignedSalesRepId != assignedRepId)
                return Forbid();
        }

        var command = new UpdateCustomerCommand
        {
            CustomerId = customerId,
            CustomerName = request.CustomerName,
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            Industry = request.Industry,
            CompanySize = request.CompanySize,
            Classification = request.Classification,
            Type = request.Type,
            Segment = request.Segment,
            AccountValue = request.AccountValue,
            AssignedSalesRepId = request.AssignedSalesRepId
        };

        try
        {
            var updated = await _updateCustomerHandler.HandleAsync(command, cancellationToken);
            if (updated is null)
                return NotFound();

            return Ok(MapCustomerToResponse(updated));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [Authorize(Roles = "SalesManager,Admin")]
    [HttpPatch("{customerId:int}/classification")]
    public async Task<IActionResult> ChangeClassification([FromRoute] int customerId, [FromBody] ChangeClassificationRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to change classification for customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        var changed = await _changeClassificationHandler.HandleAsync(new ChangeClassificationCommand
        {
            CustomerId = customerId,
            Classification = request.Classification
        }, cancellationToken);

        if (!changed)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpDelete("{customerId:int}")]
    public async Task<IActionResult> DeleteCustomer([FromRoute] int customerId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to delete customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        try
        {
            var deleted = await _deleteCustomerHandler.HandleAsync(new DeleteCustomerCommand { CustomerId = customerId }, cancellationToken);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    private static CustomerResponse MapCustomerToResponseBasic(Customer c) =>
        new()
        {
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            Website = c.Website,
            Industry = c.Industry,
            CompanySize = c.CompanySize,
            Classification = c.Classification,
            Type = c.Type,
            Segment = c.Segment,
            AccountValue = c.AccountValue,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate,
            AssignedSalesRepId = c.AssignedSalesRepId,
            Contacts = new List<ContactResponse>(),
            Addresses = new List<AddressResponse>(),
            Interactions = new List<InteractionResponse>()
        };

    private static CustomerResponse MapCustomerToResponse(Customer c) =>
        new()
        {
            CustomerId = c.CustomerId,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            Website = c.Website,
            Industry = c.Industry,
            CompanySize = c.CompanySize,
            Classification = c.Classification,
            Type = c.Type,
            Segment = c.Segment,
            AccountValue = c.AccountValue,
            CreatedDate = c.CreatedDate,
            ModifiedDate = c.ModifiedDate,
            AssignedSalesRepId = c.AssignedSalesRepId,
            Contacts = (c.Contacts ?? Array.Empty<Contact>()).Select(x => new ContactResponse
            {
                ContactId = x.ContactId,
                CustomerId = x.CustomerId,
                Name = string.IsNullOrWhiteSpace(x.Name) ? $"{x.FirstName} {x.LastName}".Trim() : x.Name,
                Title = x.Title,
                Email = x.Email,
                Phone = x.Phone,
                IsPrimary = x.IsPrimary
            }).ToList(),
            Addresses = (c.Addresses ?? Array.Empty<Address>()).Select(x => new AddressResponse
            {
                AddressId = x.AddressId,
                CustomerId = x.CustomerId,
                AddressType = x.AddressType,
                Street = string.IsNullOrWhiteSpace(x.Street) ? x.Line1 : x.Street,
                City = x.City,
                State = x.State,
                PostalCode = x.PostalCode,
                Country = x.Country,
                IsPrimary = x.IsPrimary
            }).ToList(),
            Interactions = (c.Interactions ?? Array.Empty<Interaction>()).Select(x => new InteractionResponse
            {
                InteractionId = x.InteractionId,
                CustomerId = x.CustomerId,
                InteractionDate = x.InteractionDate,
                Type = x.Type,
                Subject = string.IsNullOrWhiteSpace(x.Subject) ? x.Summary : x.Subject,
                Details = x.Details ?? x.Notes,
                PerformedBy = x.PerformedBy
            }).ToList()
        };
}

