using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.Common.CQRS;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Repositories;
using ECommerce.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.V1.Commands;

public sealed record UpdateUserAddressCommand(
    Guid Id,
    Guid UserId,
    string Label,
    string Street,
    string City,
    string ZipCode,
    string Country) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateUserAddressCommandValidator : AbstractValidator<UpdateUserAddressCommand>
{
    public UpdateUserAddressCommandValidator(
        IUserAddressRepository userAddressRepository,
        ILocalizationHelper localizer)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (command, id, ct) =>
                await userAddressRepository.AnyAsync(x => x.Id == id && x.UserId == command.UserId && x.IsActive, ct))
            .WithMessage(localizer[UserAddressConsts.NotFound]);

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

public sealed class UpdateUserAddressCommandHandler(
    IUserAddressRepository userAddressRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<UpdateUserAddressCommand, Result>(lazyServiceProvider)
{
    public override async Task<Result> Handle(UpdateUserAddressCommand command, CancellationToken cancellationToken)
    {
        var userAddress = await userAddressRepository.GetByIdAsync(command.Id, cancellationToken: cancellationToken);

        if (userAddress is null || userAddress.UserId != command.UserId || !userAddress.IsActive)
            return Result.NotFound(Localizer[UserAddressConsts.NotFound]);

        var address = new Address(
            command.Street,
            command.City,
            command.ZipCode,
            command.Country);

        userAddress.Update(command.Label, address);

        userAddressRepository.Update(userAddress);

        return Result.Success();
    }
} 