using ECommerce.Application.Features.UserAddresses.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.UserAddresses;

public static class UserAddressMapperConfig
{
    public static void Configure(TypeAdapterConfig config)
    {
        config.ForType<UserAddress, UserAddressDto>()
            .Map(dest => dest.Street, src => src.Address.Street)
            .Map(dest => dest.City, src => src.Address.City)
            .Map(dest => dest.State, src => src.Address.State)
            .Map(dest => dest.ZipCode, src => src.Address.ZipCode)
            .Map(dest => dest.Country, src => src.Address.Country);
    }
} 