using Ardalis.Result.AspNetCore;
using ECommerce.Application.Features.Carts.V1.Commands;
using ECommerce.Application.Features.Carts.V1.DTOs;
using ECommerce.Application.Features.Carts.V1.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers.V1;

public sealed class CartController : BaseApiV1Controller
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
        if (result.Status == Ardalis.Result.ResultStatus.Ok)
            return NoContent();
        
        return result.ToActionResult(this);
    }
} 