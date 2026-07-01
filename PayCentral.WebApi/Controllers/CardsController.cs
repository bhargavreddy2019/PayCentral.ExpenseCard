using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCentral.Application.Cards;

namespace PayCentral.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetCards(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(
            new GetCardsQuery(searchTerm, status, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{cardId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetCard(Guid cardId)
    {
        var result = await _mediator.Send(new GetCardByIdQuery(cardId));
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "AdminOrCardholder")]
    public async Task<IActionResult> GetCardsByUser(Guid userId)
    {
        var result = await _mediator.Send(new GetCardsByUserQuery(userId));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{cardId}/activate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ActivateCard(Guid cardId)
    {
        var result = await _mediator.Send(new ActivateCardCommand(cardId));
        return Ok(result);
    }

    [HttpPut("{cardId}/block")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> BlockCard(Guid cardId, [FromBody] BlockCardRequest request)
    {
        var result = await _mediator.Send(new BlockCardCommand(cardId, request.Reason));
        return Ok(result);
    }

    [HttpPut("{cardId}/unblock")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UnblockCard(Guid cardId)
    {
        var result = await _mediator.Send(new UnblockCardCommand(cardId));
        return Ok(result);
    }

    [HttpPut("{cardId}/suspend")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SuspendCard(Guid cardId, [FromBody] SuspendCardRequest request)
    {
        var result = await _mediator.Send(new SuspendCardCommand(cardId, request.Reason));
        return Ok(result);
    }

    [HttpPut("{cardId}/close")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CloseCard(Guid cardId, [FromBody] CloseCardRequest request)
    {
        var result = await _mediator.Send(new CloseCardCommand(cardId, request.Reason));
        return Ok(result);
    }
}

public record BlockCardRequest(string Reason);
public record SuspendCardRequest(string Reason);
public record CloseCardRequest(string Reason);