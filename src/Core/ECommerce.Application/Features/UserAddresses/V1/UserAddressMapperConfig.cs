using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.UserAddresses.V1;

public static class UserAddressMapperConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<UserAddress, UserAddressDto>.NewConfig()
            .Map(dest => dest.Street, src => src.Address.Street)
            .Map(dest => dest.City, src => src.Address.City)
            .Map(dest => dest.ZipCode, src => src.Address.ZipCode)
            .Map(dest => dest.Country, src => src.Address.Country);
    }
} 