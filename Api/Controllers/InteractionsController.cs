using CustomerManagementSystem.Api.Contracts.Customers;
using CustomerManagementSystem.Api.Security;
using CustomerManagementSystem.Application.Customers.Commands;
using CustomerManagementSystem.Application.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerManagementSystem.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/interactions")]
public class InteractionsController : ControllerBase
{
    private readonly GetCustomerByIdQueryHandler _getCustomerByIdHandler;
    private readonly GetCustomerInteractionsQueryHandler _getCustomerInteractionsQueryHandler;
    private readonly AddInteractionCommandHandler _addInteractionHandler;
    private readonly UpdateInteractionCommandHandler _updateInteractionHandler;
    private readonly DeleteInteractionCommandHandler _deleteInteractionHandler;
    private readonly CustomerAuthorizationService _authorizationService;
    private readonly ILogger<InteractionsController> _logger;

    public InteractionsController(
        GetCustomerByIdQueryHandler getCustomerByIdHandler,
        GetCustomerInteractionsQueryHandler getCustomerInteractionsQueryHandler,
        AddInteractionCommandHandler addInteractionHandler,
        UpdateInteractionCommandHandler updateInteractionHandler,
        DeleteInteractionCommandHandler deleteInteractionHandler,
        CustomerAuthorizationService authorizationService,
        ILogger<InteractionsController> logger)
    {
        _getCustomerByIdHandler = getCustomerByIdHandler;
        _getCustomerInteractionsQueryHandler = getCustomerInteractionsQueryHandler;
        _addInteractionHandler = addInteractionHandler;
        _updateInteractionHandler = updateInteractionHandler;
        _deleteInteractionHandler = deleteInteractionHandler;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InteractionResponse>>> GetInteractions([FromRoute] int customerId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
        {
            _logger.LogWarning("Authorization failure: role {Role} tried to access interactions for customer {CustomerId}", User.GetRole(), customerId);
            return Forbid();
        }

        var interactions = await _getCustomerInteractionsQueryHandler.HandleAsync(new GetCustomerInteractionsQuery { CustomerId = customerId }, cancellationToken);
        return Ok(interactions.Select(MapInteraction).ToList());
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPost]
    public async Task<ActionResult<InteractionResponse>> AddInteraction([FromRoute] int customerId, [FromBody] AddInteractionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        var interaction = await _addInteractionHandler.HandleAsync(new AddInteractionCommand
        {
            CustomerId = customerId,
            InteractionDate = request.InteractionDate,
            Type = request.Type,
            Subject = request.Subject,
            Details = request.Details,
            PerformedBy = request.PerformedBy
        }, cancellationToken);

        return Ok(MapInteraction(interaction));
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpPut("{interactionId:int}")]
    public async Task<ActionResult<InteractionResponse>> UpdateInteraction([FromRoute] int customerId, [FromRoute] int interactionId, [FromBody] UpdateInteractionRequest request, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        // Verify interaction belongs to customer
        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        var existing = customer.Interactions.FirstOrDefault(i => i.InteractionId == interactionId);
        if (existing is null) return NotFound();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updated = await _updateInteractionHandler.HandleAsync(new UpdateInteractionCommand
        {
            InteractionId = interactionId,
            InteractionDate = request.InteractionDate,
            Type = request.Type,
            Subject = request.Subject,
            Details = request.Details,
            PerformedBy = request.PerformedBy
        }, cancellationToken);

        if (updated is null) return NotFound();
        return Ok(MapInteraction(updated));
    }

    [Authorize(Roles = "SalesRep,SalesManager,Admin")]
    [HttpDelete("{interactionId:int}")]
    public async Task<IActionResult> DeleteInteraction([FromRoute] int customerId, [FromRoute] int interactionId, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanAccessCustomerAsync(User, customerId, cancellationToken))
            return Forbid();

        var customer = await _getCustomerByIdHandler.HandleAsync(new GetCustomerByIdQuery { CustomerId = customerId }, cancellationToken);
        if (customer is null) return NotFound();

        var existing = customer.Interactions.FirstOrDefault(i => i.InteractionId == interactionId);
        if (existing is null) return NotFound();

        var deleted = await _deleteInteractionHandler.HandleAsync(new DeleteInteractionCommand { InteractionId = interactionId }, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    private static InteractionResponse MapInteraction(CustomerManagementSystem.Domain.Entities.Interaction i) =>
        new()
        {
            InteractionId = i.InteractionId,
            CustomerId = i.CustomerId,
            InteractionDate = i.InteractionDate,
            Type = i.Type,
            Subject = string.IsNullOrWhiteSpace(i.Subject) ? i.Summary : i.Subject,
            Details = i.Details ?? i.Notes,
            PerformedBy = i.PerformedBy
        };
}

