using ECommerce.Application.Common.Constants;
using ECommerce.Application.Features.Roles.V1.Commands;
using ECommerce.Application.Features.Roles.V1.DTOs;
using ECommerce.Application.Features.Roles.V1.Queries;
using ECommerce.Application.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ardalis.Result.AspNetCore;
using System.Security.Claims;
using Ardalis.Result;

namespace ECommerce.WebAPI.Controllers.V1;

public sealed class RoleController : BaseApiV1Controller
{
    [HttpGet]
    [Authorize(Policy = PermissionConstants.Roles.Read)]
    [ProducesResponseType(typeof(PagedResult<List<RoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles([FromQuery] PageableRequestParams requestParams, [FromQuery] bool includePermissions = false, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetAllRolesQuery(requestParams, includePermissions), cancellationToken);
        return Ok(result);
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
    public async Task<ActionResult> AddUserToRole(Guid userId, [FromBody] RoleIdRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new AddUserToRoleCommand(userId, request.RoleId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("user/{userId}/remove-role")]
    [Authorize(Policy = PermissionConstants.Roles.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveUserFromRole(Guid userId, [FromBody] RoleIdRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new RemoveUserFromRoleCommand(userId, request.RoleId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("delete-many")]
    [Authorize(Policy = PermissionConstants.Roles.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteRoles([FromBody] List<Guid> ids, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteRolesCommand(ids), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record RoleIdRequest(Guid RoleId); 