using Ardalis.Result;
using ECommerce.Application.Common.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Application.Features.UserAddresses.V1.Specifications;
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
        var spec = new UserAddressActiveSpecification(query.AddressId, query.UserId);
        var addresses = await userAddressRepository.ListAsync(spec, cancellationToken);
        var address = addresses.FirstOrDefault();

        if (address == null)
            return Result.NotFound("User address not found or not accessible");

        return Result.Success(address.Adapt<UserAddressDto>());
    }
}