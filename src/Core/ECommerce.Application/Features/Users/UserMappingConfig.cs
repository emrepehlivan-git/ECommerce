using ECommerce.Application.Features.Users.DTOs;
using ECommerce.Domain.Entities;
using Mapster;

namespace ECommerce.Application.Features.Users;

public sealed class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.FullName, src => src.FullName.ToString())
            .Map(dest => dest.Birthday, src => src.Birthday);
    }
}