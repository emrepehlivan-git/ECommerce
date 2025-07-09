using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Repositories;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.V1.Commands;

public sealed record SetDefaultUserAddressCommand(
    Guid Id,
    Guid UserId) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class SetDefaultUserAddressCommandValidator : AbstractValidator<SetDefaultUserAddressCommand>
{
    public SetDefaultUserAddressCommandValidator(
        IUserAddressRepository userAddressRepository,
        ILocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (command, id, ct) =>
                await userAddressRepository.AnyAsync(x => x.Id == id && x.UserId == command.UserId && x.IsActive, ct))
            .WithMessage(localizer[UserAddressConsts.NotFound]);
    }
}

public sealed class SetDefaultUserAddressCommandHandler(
    IUserAddressRepository userAddressRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<SetDefaultUserAddressCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(SetDefaultUserAddressCommand command, CancellationToken cancellationToken)
    {
        var userAddress = await userAddressRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (userAddress is null || userAddress.UserId != command.UserId || !userAddress.IsActive)
            return Result.NotFound(Localizer[UserAddressConsts.NotFound]);

        if (userAddress.IsDefault)
            return Result.Error(Localizer[UserAddressConsts.AddressAlreadyDefault]);

        await userAddressRepository.SetDefaultAddressAsync(command.UserId, command.Id, cancellationToken);

        return Result.Success();
    }
} 