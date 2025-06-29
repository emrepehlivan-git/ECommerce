using Ardalis.Result.AspNetCore;
using ECommerce.Application.Features.Carts.Commands;
using ECommerce.Application.Features.Carts.DTOs;
using ECommerce.Application.Features.Carts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[Authorize]
public sealed class CartController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var query = new GetCartQuery();
        var result = await Mediator.Send(query);
        return result.ToActionResult(this);
    }

    [HttpPost("add")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CartSummaryDto>> AddToCart([FromBody] AddToCartCommand command)
    {
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpDelete("remove/{productId:guid}")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartSummaryDto>> RemoveFromCart(Guid productId)
    {
        var command = new RemoveFromCartCommand(productId);
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpPut("update-quantity")]
    [ProducesResponseType(typeof(CartSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartSummaryDto>> UpdateQuantity([FromBody] UpdateCartItemQuantityCommand command)
    {
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ClearCart()
    {
        var result = await Mediator.Send(new ClearCartCommand());
        return result.ToActionResult(this);
    }
} 