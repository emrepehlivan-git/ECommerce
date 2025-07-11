using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.V1.Queries;

public sealed record GetUserAddressByIdQuery(Guid AddressId, Guid UserId) : IRequest<Result<UserAddressDto>>;

public sealed class GetUserAddressByIdQueryHandler(
    IUserAddressRepository userAddressRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUserAddressByIdQuery, Result<UserAddressDto>>(lazyServiceProvider)
{
    public override async Task<Result<UserAddressDto>> Handle(GetUserAddressByIdQuery query, CancellationToken cancellationToken)
    {
        var address = await userAddressRepository.GetByIdAsync(query.AddressId, null, false, cancellationToken);

        if (address == null)
            return Result.NotFound("User address not found");

        // Security check: ensure the address belongs to the requesting user
        if (address.UserId != query.UserId)
            return Result.Forbidden();

        return Result.Success(address.Adapt<UserAddressDto>());
    }
}