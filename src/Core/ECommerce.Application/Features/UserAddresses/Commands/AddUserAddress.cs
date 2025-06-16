using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.Commands;

public sealed record AddUserAddressCommand(
    Guid UserId,
    string Label,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool IsDefault = false) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class AddUserAddressCommandValidator : AbstractValidator<AddUserAddressCommand>
{
    public AddUserAddressCommandValidator(
        IIdentityService identityService,
        LocalizationHelper localizer)
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (id, ct) =>
                await identityService.FindByIdAsync(id) != null)
            .WithMessage(localizer[UserAddressConsts.UserNotFound]);

        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage(localizer[UserAddressConsts.LabelRequired])
            .MinimumLength(UserAddressConsts.LabelMinLengthValue)
            .WithMessage(string.Format(localizer[UserAddressConsts.LabelMinLength], UserAddressConsts.LabelMinLengthValue))
            .MaximumLength(UserAddressConsts.LabelMaxLengthValue)
            .WithMessage(string.Format(localizer[UserAddressConsts.LabelMaxLength], UserAddressConsts.LabelMaxLengthValue));

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required")
            .MaximumLength(200)
            .WithMessage("Street cannot be longer than 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City cannot be longer than 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(100)
            .WithMessage("State cannot be longer than 100 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("ZipCode is required")
            .MaximumLength(20)
            .WithMessage("ZipCode cannot be longer than 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country cannot be longer than 100 characters");
    }
}

public sealed class AddUserAddressCommandHandler(
    IUserAddressRepository userAddressRepository,
    IIdentityService identityService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<AddUserAddressCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(AddUserAddressCommand command, CancellationToken cancellationToken)
    {
        if (await identityService.FindByIdAsync(command.UserId) is null)
            return Result.Error(Localizer[UserAddressConsts.UserNotFound]);

        var address = new Address(
            command.Street,
            command.City,
            command.State,
            command.ZipCode,
            command.Country);

        var userAddress = UserAddress.Create(
            command.UserId,
            command.Label,
            address,
            command.IsDefault);

        // Eğer bu kullanıcının hiç varsayılan adresi yoksa, bu adresi varsayılan yap
        if (!await userAddressRepository.HasDefaultAddressAsync(command.UserId, cancellationToken))
        {
            userAddress.SetAsDefault();
        }
        else if (command.IsDefault)
        {
            // Eğer bu adres varsayılan olarak işaretlenmişse, diğer varsayılan adresleri kaldır
            await userAddressRepository.SetDefaultAddressAsync(command.UserId, userAddress.Id, cancellationToken);
        }

        userAddressRepository.Add(userAddress);

        return Result.Success(userAddress.Id);
    }
} 