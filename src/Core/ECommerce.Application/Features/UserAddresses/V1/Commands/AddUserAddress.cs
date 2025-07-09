using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel.DependencyInjection;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.V1.Commands;

public sealed record AddUserAddressCommand(
    Guid UserId,
    string Label,
    string Street,
    string City,
    string ZipCode,
    string Country,
    bool IsDefault = false) : IRequest<Result<Guid>>, IValidatableRequest, ITransactionalRequest;

public sealed class AddUserAddressCommandValidator : AbstractValidator<AddUserAddressCommand>
{
    public AddUserAddressCommandValidator(
        IUserService userService,
        ILocalizationHelper localizer)  
    {
        RuleFor(x => x.UserId)
            .MustAsync(async (id, ct) =>
                await userService.FindByIdAsync(id) != null)
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
            .WithMessage(localizer[UserAddressConsts.StreetRequired])
            .MaximumLength(UserAddressConsts.StreetMaxLengthValue)
            .WithMessage(string.Format(localizer[UserAddressConsts.StreetMaxLength], UserAddressConsts.StreetMaxLengthValue));

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage(localizer[UserAddressConsts.CityRequired])
            .MaximumLength(UserAddressConsts.CityMaxLengthValue)
            .WithMessage(string.Format(localizer[UserAddressConsts.CityMaxLength], UserAddressConsts.CityMaxLengthValue));

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage(localizer[UserAddressConsts.ZipCodeRequired])
            .MaximumLength(UserAddressConsts.ZipCodeMaxLengthValue)
            .WithMessage(string.Format(localizer[UserAddressConsts.ZipCodeMaxLength], UserAddressConsts.ZipCodeMaxLengthValue));

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage(localizer[UserAddressConsts.CountryRequired])
            .MaximumLength(UserAddressConsts.CountryMaxLengthValue)
            .WithMessage(string.Format(localizer[UserAddressConsts.CountryMaxLength], UserAddressConsts.CountryMaxLengthValue));
    }
}

public sealed class AddUserAddressCommandHandler(
    IUserAddressRepository userAddressRepository,
    IUserService userService,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<AddUserAddressCommand, Result<Guid>>(lazyServiceProvider)
{
    public override async Task<Result<Guid>> Handle(AddUserAddressCommand command, CancellationToken cancellationToken)
    {
        if (await userService.FindByIdAsync(command.UserId) is null)
            return Result.Error(Localizer[UserAddressConsts.UserNotFound]);

        var address = new Address(
            command.Street,
            command.City,
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