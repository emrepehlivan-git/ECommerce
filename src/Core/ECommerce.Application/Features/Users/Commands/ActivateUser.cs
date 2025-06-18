using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;

namespace ECommerce.Application.Features.Users.Commands;

public record ActivateUserCommand(Guid UserId) : IRequest<Result>, ITransactionalRequest;

public sealed class ActivateUserCommandHandler(
    IIdentityService identityService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<ActivateUserCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(command.UserId);

        if (user is null)
            return Result.NotFound(Localizer[UserConsts.NotFound]);

        if (user.IsActive)
            return Result.Success();

        user.Activate();
        var result = await identityService.UpdateAsync(user);

        return result.Succeeded
            ? Result.Success()
            : Result.Invalid(result.Errors.Select(e => new ValidationError(e.Description)).ToArray());
    }
}