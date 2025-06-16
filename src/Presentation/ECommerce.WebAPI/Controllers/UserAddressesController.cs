using Ardalis.Result.AspNetCore;
using ECommerce.Application.Features.UserAddresses.Commands;
using ECommerce.Application.Features.UserAddresses.DTOs;
using ECommerce.Application.Features.UserAddresses.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[Route("api/[controller]")]
public sealed class UserAddressesController : BaseApiController
{
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<UserAddressDto>>> GetUserAddresses(Guid userId, bool activeOnly = true)
    {
        var result = await Mediator.Send(new GetUserAddressesQuery(userId, activeOnly));
        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> AddUserAddress([FromBody] AddUserAddressCommand command)
    {
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateUserAddress(Guid id, [FromBody] UpdateUserAddressRequest request)
    {
        var command = new UpdateUserAddressCommand(
            id,
            request.UserId,
            request.Label,
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country);

        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/set-default")]
    public async Task<ActionResult> SetDefaultAddress(Guid id, [FromBody] SetDefaultRequest request)
    {
        var command = new SetDefaultUserAddressCommand(id, request.UserId);
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUserAddress(Guid id, [FromBody] DeleteUserAddressRequest request)
    {
        var command = new DeleteUserAddressCommand(id, request.UserId);
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }
}

public sealed record UpdateUserAddressRequest(
    Guid UserId,
    string Label,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);

public sealed record SetDefaultRequest(Guid UserId);

public sealed record DeleteUserAddressRequest(Guid UserId); 