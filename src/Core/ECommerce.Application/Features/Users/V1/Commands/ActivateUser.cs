using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Services;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Users.V1.Commands;

public record ActivateUserCommand(Guid UserId) : IRequest<Result>, ITransactionalRequest;

public sealed class ActivateUserCommandHandler(
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<ActivateUserCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(command.UserId);

        if (user is null)
            return Result.NotFound(Localizer[UserConsts.NotFound]);

        if (user.IsActive)
            return Result.Success();

        user.Activate();
        var result = await userService.UpdateAsync(user);

        return result.Succeeded
            ? Result.Success()
            : Result.Invalid(result.Errors.Select(e => new ValidationError(e.Description)).ToArray());
    }
}