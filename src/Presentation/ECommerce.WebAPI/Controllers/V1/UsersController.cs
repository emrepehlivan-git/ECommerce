using ECommerce.Application.Features.Users.V1.Queries;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Features.Users.V1.Commands;
using ECommerce.Application.Parameters;
using ECommerce.Application.Features.Users.V1.DTOs;
using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Application.Services;
using Ardalis.Result;
namespace ECommerce.WebAPI.Controllers.V1;

public sealed class UsersController : BaseApiV1Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsers([FromQuery] PageableRequestParams pageableRequestParams, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUsersQuery(pageableRequestParams), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("activate/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ActivateUserCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("deactivate/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}/birthday")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateBirthday(Guid id, [FromBody] DateTime? birthday, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateUserBirthdayCommand(id, birthday), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("permissions")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<string>>> GetCurrentUserPermissions(
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IPermissionService permissionService)
    {
        var userIdString = currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var permissions = await permissionService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }

} 