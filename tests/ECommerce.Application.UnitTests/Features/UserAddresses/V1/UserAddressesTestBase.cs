using ECommerce.Application.Features.UserAddresses.V1;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using Mapster;

namespace ECommerce.Application.UnitTests.Features.UserAddresses.V1;

public abstract class UserAddressesTestBase
{
    protected static readonly Guid UserId = Guid.Parse("e64db34c-7455-41da-b255-a9a7a46ace54");
    protected static readonly Guid AddressId = Guid.Parse("f64db34c-7455-41da-b255-a9a7a46ace64");
    protected static readonly Address DefaultAddress = new("123 Main St", "New York", "10001", "USA");
    protected UserAddress DefaultUserAddress => UserAddress.Create(UserId, "Home", DefaultAddress);

    protected Mock<IUserAddressRepository> UserAddressRepositoryMock;
    protected Mock<IUserService> UserServiceMock;
    protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ILocalizationService> LocalizationServiceMock;

    protected LocalizationHelper Localizer;

    protected UserAddressesTestBase()
    {
        UserAddressRepositoryMock = new Mock<IUserAddressRepository>();
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();

        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);

        SetupDefaultLocalizationMessages();
        ConfigureMapster();
    }

    private void ConfigureMapster()
    {
        TypeAdapterConfig.GlobalSettings.ForType<UserAddress, UserAddressDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Label, src => src.Label)
            .Map(dest => dest.Street, src => src.Address.Street)
            .Map(dest => dest.City, src => src.Address.City)
            .Map(dest => dest.ZipCode, src => src.Address.ZipCode)
            .Map(dest => dest.Country, src => src.Address.Country)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.IsActive, src => src.IsActive);
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.NotFound))
            .Returns("User address not found");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.UserNotFound))
            .Returns("User not found for this address");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.LabelRequired))
            .Returns("Address label is required");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.LabelMinLength))
            .Returns("Address label must be at least {0} characters long");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.LabelMaxLength))
            .Returns("Address label cannot be longer than {0} characters");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.AddressRequired))
            .Returns("Address information is required");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.DefaultAddressCannotBeDeleted))
            .Returns("Default address cannot be deleted");
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(UserAddressConsts.AddressAlreadyDefault))
            .Returns("This address is already set as default");
    }

    protected void SetupUserExists(bool exists = true)
    {
        var user = exists ? User.Create("test@example.com", "Test", "User") : null;
        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(user);
    }

    protected void SetupUserAddressExists(UserAddress? userAddress = null)
    {
        UserAddressRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<IQueryable<UserAddress>, IQueryable<UserAddress>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userAddress ?? DefaultUserAddress);
    }

    protected void SetupUserAddressAnyAsync(bool exists = true)
    {
        UserAddressRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<Expression<Func<UserAddress, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    protected void SetupHasDefaultAddress(bool hasDefault = false)
    {
        UserAddressRepositoryMock
            .Setup(x => x.HasDefaultAddressAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasDefault);
    }

    protected void SetupGetUserAddresses(List<UserAddress>? addresses = null)
    {
        UserAddressRepositoryMock
            .Setup(x => x.GetUserAddressesAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses ?? new List<UserAddress> { DefaultUserAddress });
    }

    protected void SetupLocalizedMessage(string message)
    {
        LocalizationServiceMock
            .Setup(x => x.GetLocalizedString(message))
            .Returns(message);
    }
} 