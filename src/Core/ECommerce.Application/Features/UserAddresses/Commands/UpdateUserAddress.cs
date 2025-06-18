using Ardalis.Result;
using ECommerce.Application.Behaviors;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Helpers;
using ECommerce.Application.Repositories;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.Commands;

public sealed record UpdateUserAddressCommand(
    Guid Id,
    Guid UserId,
    string Label,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country) : IRequest<Result>, IValidatableRequest, ITransactionalRequest;

public sealed class UpdateUserAddressCommandValidator : AbstractValidator<UpdateUserAddressCommand>
{
    public UpdateUserAddressCommandValidator(
        IUserAddressRepository userAddressRepository,
        LocalizationHelper localizer)
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
            command.State,
            command.ZipCode,
            command.Country);

        userAddress.Update(command.Label, address);

        userAddressRepository.Update(userAddress);

        return Result.Success();
    }
} 