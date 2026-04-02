using CustomerManagementSystem.Api.Contracts.Customers;
using CustomerManagementSystem.Api.Security;
using CustomerManagementSystem.Application.Customers.Commands;
using CustomerManagementSystem.Application.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/contacts")]
public class ContactsController : ControllerBase
{
    private readonly GetCustomerByIdQueryHandler _getCustomerByIdHandler;
    private readonly AddContactCommandHandler _addContactHandler;
    private readonly UpdateContactCommandHandler _updateContactHandler;
    private readonly DeleteContactCommandHandler _deleteContactHandler;
    private readonly CustomerAuthorizationService _authorizationService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(
        GetCustomerByIdQueryHandler getCustomerByIdHandler,
        AddContactCommandHandler addContactHandler,
        UpdateContactCommandHandler updateContactHandler,
        DeleteContactCommandHandler deleteContactHandler,
        CustomerAuthorizationService authorizationService,
        ILogger<ContactsController> logger)
    {
        _getCustomerByIdHandler = getCustomerByIdHandler;
        _addContactHandler = addContactHandler;
        _updateContactHandler = updateContactHandler;
        _deleteContactHandler = deleteContactHandler;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactResponse>>> GetContacts([FromRoute] int customerId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to access contacts for customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        return Ok(customer.Contacts.Select(MapContact).ToList());
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPost]
    public async Task<ActionResult<ContactResponse>> AddContact([FromRoute] int customerId, [FromBody] AddContactRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to add contact for customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        try
        {
            var contact = await _addContactHandler.HandleAsync(new AddContactCommand
            {
                CustomerId = customerId,
                Name = request.Name,
                Title = request.Title,
                Email = request.Email,
                Phone = request.Phone,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            return Ok(MapContact(contact));
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
    [HttpPut("{contactId:int}")]
    public async Task<ActionResult<ContactResponse>> UpdateContact([FromRoute] int customerId, [FromRoute] int contactId, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        var existing = customer.Contacts.FirstOrDefault(c => c.ContactId == contactId);
        if (existing is null) return NotFound();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var updated = await _updateContactHandler.HandleAsync(new UpdateContactCommand
            {
                ContactId = contactId,
                Name = request.Name,
                Title = request.Title,
                Email = request.Email,
                Phone = request.Phone,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            if (updated is null) return NotFound();
            return Ok(MapContact(updated));
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
    [HttpDelete("{contactId:int}")]
    public async Task<IActionResult> DeleteContact([FromRoute] int customerId, [FromRoute] int contactId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        var existing = customer.Contacts.FirstOrDefault(c => c.ContactId == contactId);
        if (existing is null) return NotFound();

        var deleted = await _deleteContactHandler.HandleAsync(new DeleteContactCommand { ContactId = contactId }, cancellationToken);
        if (!deleted) return NotFound();

        return NoContent();
    }

    private static ContactResponse MapContact(CustomerManagementSystem.Domain.Entities.Contact c) =>
        new()
        {
            ContactId = c.ContactId,
            CustomerId = c.CustomerId,
            Name = string.IsNullOrWhiteSpace(c.Name) ? $"{c.FirstName} {c.LastName}".Trim() : c.Name,
            Title = c.Title,
            Email = c.Email,
            Phone = c.Phone,
            IsPrimary = c.IsPrimary
        };
}

