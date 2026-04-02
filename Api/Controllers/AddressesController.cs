using CustomerManagementSystem.Api.Contracts.Customers;
using CustomerManagementSystem.Api.Security;
using CustomerManagementSystem.Application.Customers.Commands;
using CustomerManagementSystem.Application.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/addresses")]
public class AddressesController : ControllerBase
{
    private readonly GetCustomerByIdQueryHandler _getCustomerByIdHandler;
    private readonly AddAddressCommandHandler _addAddressHandler;
    private readonly UpdateAddressCommandHandler _updateAddressHandler;
    private readonly DeleteAddressCommandHandler _deleteAddressHandler;
    private readonly CustomerAuthorizationService _authorizationService;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(
        GetCustomerByIdQueryHandler getCustomerByIdHandler,
        AddAddressCommandHandler addAddressHandler,
        UpdateAddressCommandHandler updateAddressHandler,
        DeleteAddressCommandHandler deleteAddressHandler,
        CustomerAuthorizationService authorizationService,
        ILogger<AddressesController> logger)
    {
        _getCustomerByIdHandler = getCustomerByIdHandler;
        _addAddressHandler = addAddressHandler;
        _updateAddressHandler = updateAddressHandler;
        _deleteAddressHandler = deleteAddressHandler;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressResponse>>> GetAddresses([FromRoute] int customerId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to access addresses for customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        return Ok(customer.Addresses.Select(MapAddress).ToList());
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPost]
    public async Task<ActionResult<AddressResponse>> AddAddress([FromRoute] int customerId, [FromBody] AddAddressRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        try
        {
            var address = await _addAddressHandler.HandleAsync(new AddAddressCommand
            {
                CustomerId = customerId,
                AddressType = request.AddressType,
                Line1 = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            address.AddressType = request.AddressType;
            address.Street = request.Street;
            address.Line1 = request.Street;

            return Ok(MapAddress(address));
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

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPut("{addressId:int}")]
    public async Task<ActionResult<AddressResponse>> UpdateAddress([FromRoute] int customerId, [FromRoute] int addressId, [FromBody] UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        var existing = customer.Addresses.FirstOrDefault(a => a.AddressId == addressId);
        if (existing is null) return NotFound();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var updated = await _updateAddressHandler.HandleAsync(new UpdateAddressCommand
            {
                AddressId = addressId,
                CustomerId = customerId,
                AddressType = request.AddressType,
                Line1 = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            if (updated is null) return NotFound();
            updated.AddressType = request.AddressType;
            updated.Street = request.Street;
            updated.Line1 = request.Street;
            return Ok(MapAddress(updated));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpDelete("{addressId:int}")]
    public async Task<IActionResult> DeleteAddress([FromRoute] int customerId, [FromRoute] int addressId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        var existing = customer.Addresses.FirstOrDefault(a => a.AddressId == addressId);
        if (existing is null) return NotFound();

        var deleted = await _deleteAddressHandler.HandleAsync(new DeleteAddressCommand { AddressId = addressId }, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    private static AddressResponse MapAddress(CustomerManagementSystem.Domain.Entities.Address a) =>
        new()
        {
            AddressId = a.AddressId,
            CustomerId = a.CustomerId,
            AddressType = a.AddressType,
            Street = string.IsNullOrWhiteSpace(a.Street) ? a.Line1 : a.Street,
            City = a.City,
            State = a.State,
            PostalCode = a.PostalCode,
            Country = a.Country,
            IsPrimary = a.IsPrimary
        };
}

