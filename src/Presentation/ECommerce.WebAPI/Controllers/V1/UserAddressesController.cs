using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using ECommerce.Application.Features.UserAddresses.V1.Commands;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Application.Features.UserAddresses.V1.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Application.Services;

namespace ECommerce.WebAPI.Controllers.V1;

public sealed class UserAddressesController : BaseApiV1Controller
{
    private readonly ICurrentUserService _currentUserService;

    public UserAddressesController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<UserAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserAddressDto>>> GetCurrentUserAddresses(bool activeOnly = true)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Unauthorized();

        var result = await Mediator.Send(new GetUserAddressesQuery(currentUserId, activeOnly));
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UserAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserAddressDto>> GetUserAddressById(Guid id)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Unauthorized();

        var result = await Mediator.Send(new GetUserAddressByIdQuery(id, currentUserId));
        return result.ToActionResult(this);
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(List<UserAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserAddressDto>>> GetUserAddresses(Guid userId, bool activeOnly = true)
    {
        var result = await Mediator.Send(new GetUserAddressesQuery(userId, activeOnly));
        return result.ToActionResult(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> AddUserAddress(AddUserAddressCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsSuccess)
            return Created("", result.Value);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateUserAddress(Guid id, UpdateUserAddressRequest request)
    {
        var command = new UpdateUserAddressCommand(
            id,
            request.UserId,
            request.Label,
            request.Street,
            request.City,
            request.ZipCode,
            request.Country);

        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id:guid}/set-default")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SetDefaultAddress(Guid id, SetDefaultRequest request)
    {
        var command = new SetDefaultUserAddressCommand(id, request.UserId);
        var result = await Mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteUserAddress(Guid id, DeleteUserAddressRequest request)
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
    string ZipCode,
    string Country);

public sealed record SetDefaultRequest(Guid UserId);

public sealed record DeleteUserAddressRequest(Guid UserId); 