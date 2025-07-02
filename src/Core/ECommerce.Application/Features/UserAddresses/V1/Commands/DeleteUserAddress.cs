using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.V1.Commands;

public sealed record DeleteUserAddressCommand(
    Guid Id,
    Guid UserId) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class DeleteUserAddressCommandValidator : AbstractValidator<DeleteUserAddressCommand>
{
    public DeleteUserAddressCommandValidator(
        IUserAddressRepository userAddressRepository,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (command, id, ct) =>
                await userAddressRepository.AnyAsync(x => x.Id == id && x.UserId == command.UserId && x.IsActive, ct))
            .WithMessage(localizer[UserAddressConsts.NotFound]);

        RuleFor(x => x.Id)
            .MustAsync(async (command, id, ct) =>
            {
                var address = await userAddressRepository.GetByIdAsync(id, cancellationToken: ct);
                return address is null || !address.IsDefault;
            })
            .WithMessage(localizer[UserAddressConsts.DefaultAddressCannotBeDeleted]);
    }
}

public sealed class DeleteUserAddressCommandHandler(
    IUserAddressRepository userAddressRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<DeleteUserAddressCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(DeleteUserAddressCommand command, CancellationToken cancellationToken)
    {
        var userAddress = await userAddressRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (userAddress is null || userAddress.UserId != command.UserId || !userAddress.IsActive)
            return Result.NotFound(Localizer[UserAddressConsts.NotFound]);

        if (userAddress.IsDefault)
            return Result.Error(Localizer[UserAddressConsts.DefaultAddressCannotBeDeleted]);

        userAddress.Deactivate();
        userAddressRepository.Update(userAddress);

        return Result.Success();
    }
} 