using Ardalis.Result;
using ECommerce.Application.CQRS;
using ECommerce.SharedKernel.DependencyInjection;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Application.Repositories;
using ECommerce.SharedKernel;
using Mapster;
using MediatR;

namespace ECommerce.Application.Features.UserAddresses.V1.Queries;

public sealed record GetUserAddressesQuery(Guid UserId, bool ActiveOnly = true) : IRequest<Result<List<UserAddressDto>>>;

public sealed class GetUserAddressesQueryHandler(
    IUserAddressRepository userAddressRepository,
    ILazyServiceProvider lazyServiceProvider) : BaseHandler<GetUserAddressesQuery, Result<List<UserAddressDto>>>(lazyServiceProvider)
{
    public override async Task<Result<List<UserAddressDto>>> Handle(GetUserAddressesQuery query, CancellationToken cancellationToken)
    {
        var addresses = await userAddressRepository.GetUserAddressesAsync(query.UserId, query.ActiveOnly, cancellationToken);

        return Result.Success(addresses.Adapt<List<UserAddressDto>>());
    }
} 