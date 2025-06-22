using ECommerce.Application.Constants;
using ECommerce.Application.Features.Roles.Commands;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Features.Roles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;

namespace ECommerce.WebAPI.Controllers;

public sealed class RoleController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = PermissionConstants.Roles.Read)]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<RoleDto>>> GetRoles(CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetAllRolesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionConstants.Roles.Read)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetRoleById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetRoleByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = PermissionConstants.Roles.Read)]
    [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserRoleDto>> GetUserRoles(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetUserRolesQuery(userId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.Roles.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> CreateRole(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionConstants.Roles.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateRole(Guid id, UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command with { Id = id }, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionConstants.Roles.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("user/{userId}/add-role")]
    [Authorize(Policy = PermissionConstants.Roles.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddUserToRole(Guid userId, [FromBody] string roleName, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AddUserToRoleCommand(userId, roleName), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("user/{userId}/remove-role")]
    [Authorize(Policy = PermissionConstants.Roles.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveUserFromRole(Guid userId, [FromBody] string roleName, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RemoveUserFromRoleCommand(userId, roleName), cancellationToken);
        return result.ToActionResult(this);
    }
} 