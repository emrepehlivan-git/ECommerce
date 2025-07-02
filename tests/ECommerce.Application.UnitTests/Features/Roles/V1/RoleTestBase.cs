using ECommerce.Application.Helpers;
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
    protected Mock<ILocalizationService> LocalizationServiceMock;
    protected Mock<ICacheManager> CacheManagerMock;

    protected LocalizationHelper Localizer;

    protected RoleTestBase()
    {
        RoleServiceMock = new Mock<IRoleService>();
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();
        CacheManagerMock = new Mock<ICacheManager>();
        
        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.NameIsRequired))
            .Returns("Role name is required.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.NameExists))
            .Returns("Role name already exists.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.NameMustBeAtLeastCharacters, RoleConsts.NameMinLength.ToString()))
            .Returns("Role name must be at least {0} characters long.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.NameMustBeLessThanCharacters, RoleConsts.NameMaxLength.ToString()))
            .Returns("Role name must be less than {0} characters long.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.RoleNotFound))
            .Returns("Role not found.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.UserNotFound))
            .Returns("User not found.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.UserAlreadyInRole))
            .Returns("User already has this role.");

        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(RoleConsts.UserNotInRole))
            .Returns("User does not have this role.");
        
        // UserConsts için de setup ekle
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserConsts.NotFound))
            .Returns("User not found.");
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
                It.IsAny<string>(),
                It.IsAny<bool>()
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