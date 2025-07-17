using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Identity;
using Mapster;

namespace ECommerce.Application.UnitTests.Features.Roles.V1;

public abstract class RoleTestBase
{
    protected static Role DefaultRole => Role.Create("TestRole");
    protected static User DefaultUser => User.Create("testuser@test.com", "Test", "User");

    protected Mock<IRoleService> RoleServiceMock;
    protected Mock<IUserService> UserServiceMock;
    protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ICacheManager> CacheManagerMock;
    protected Mock<ILocalizationHelper> LocalizerMock;

    protected RoleTestBase()
    {
        RoleServiceMock = new Mock<IRoleService>();
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        CacheManagerMock = new Mock<ICacheManager>();
        LocalizerMock = new Mock<ILocalizationHelper>();
        
        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        var translations = new Dictionary<string, string>
        {
            [RoleConsts.NameIsRequired] = "Role name is required.",
            [RoleConsts.NameExists] = "Role name already exists.",
            [RoleConsts.NameMustBeAtLeastCharacters] = "Role name must be at least {0} characters long.",
            [RoleConsts.NameMustBeLessThanCharacters] = "Role name must be less than {0} characters long.",
            [RoleConsts.RoleNotFound] = "Role not found.",
            [RoleConsts.UserNotFound] = "User not found.",
            [RoleConsts.UserAlreadyInRole] = "User already has this role.",
            [RoleConsts.UserNotInRole] = "User does not have this role.",
            [UserConsts.NotFound] = "User not found."
        };
    
        LocalizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => translations.TryGetValue(key, out var res) ? res : key);
    
        LocalizerMock.Setup(x => x[It.IsAny<string>(), It.IsAny<string>()])
            .Returns((string key, string arg) =>
            {
                var value = translations.TryGetValue(key, out var res) ? res : key;
                try
                {
                    return string.Format(value, arg);
                }
                catch (FormatException)
                {
                    return value;
                }
            });
    }

    protected void SetupRoleServiceCreateAsync(IdentityResult? result = null)
    {
        RoleServiceMock
            .Setup(x => x.CreateRoleAsync(It.IsAny<Role>()))
            .Callback<Role>(role => { if (role.Id == Guid.Empty) role.Id = Guid.NewGuid(); })
            .ReturnsAsync(result ?? IdentityResult.Success);
    }

    protected void SetupRoleServiceUpdateAsync(IdentityResult? result = null)
    {
        RoleServiceMock
            .Setup(x => x.UpdateRoleAsync(It.IsAny<Role>()))
            .ReturnsAsync(result ?? IdentityResult.Success);
    }

    protected void SetupRoleServiceDeleteAsync(IdentityResult? result = null)
    {
        RoleServiceMock
            .Setup(x => x.DeleteRoleAsync(It.IsAny<Role>()))
            .ReturnsAsync(result ?? IdentityResult.Success);
    }

    protected void SetupRoleServiceFindByIdAsync(Role? role = null)
    {
        RoleServiceMock
            .Setup(x => x.FindRoleByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(role);
    }

    protected void SetupRoleServiceFindByNameAsync(Role? role = null)
    {
        RoleServiceMock
            .Setup(x => x.FindRoleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(role);
    }

    protected void SetupRoleServiceRoleExistsAsync(bool exists = false)
    {
        RoleServiceMock
            .Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(exists);
    }

    protected void SetupRoleServiceGetAllRolesAsync(IList<Role>? roles = null)
    {
        var roleDtos = (roles ?? new List<Role>()).Adapt<List<RoleDto>>();
        RoleServiceMock
            .Setup(x => x.GetAllRolesAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new PagedResult<List<RoleDto>>(new PagedInfo(1, 10, roleDtos.Count, 1), roleDtos));
    }

    protected void SetupRoleServiceGetUserRolesAsync(IList<string>? roles = null)
    {
        RoleServiceMock
            .Setup(x => x.GetUserRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(roles ?? new List<string>());
    }

    protected void SetupRoleServiceAddToRoleAsync(IdentityResult? result = null)
    {
        RoleServiceMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(result ?? IdentityResult.Success);
    }

    protected void SetupRoleServiceRemoveFromRoleAsync(IdentityResult? result = null)
    {
        RoleServiceMock
            .Setup(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(result ?? IdentityResult.Success);
    }

    protected void SetupUserServiceFindByIdAsync(User? user = null)
    {
        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);
    }
} 