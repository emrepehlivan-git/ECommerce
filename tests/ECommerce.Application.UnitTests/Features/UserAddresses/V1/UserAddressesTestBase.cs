using ECommerce.Application.Features.UserAddresses.V1;
using ECommerce.Application.Features.UserAddresses.V1.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.ValueObjects;
using ECommerce.SharedKernel.Specifications;
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
    protected Mock<ILocalizationHelper> LocalizerMock;

    protected UserAddressesTestBase()
    {
        UserAddressRepositoryMock = new Mock<IUserAddressRepository>();
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

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
        LocalizerMock
            .Setup(x => x[UserAddressConsts.NotFound])
            .Returns("User address not found");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.UserNotFound])
            .Returns("User not found for this address");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.LabelRequired])
            .Returns("Address label is required");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.LabelMinLength])
            .Returns("Address label must be at least {0} characters long");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.LabelMaxLength])
            .Returns("Address label cannot be longer than {0} characters");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.AddressRequired])
            .Returns("Address information is required");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.DefaultAddressCannotBeDeleted])
            .Returns("Default address cannot be deleted");
        LocalizerMock
            .Setup(x => x[UserAddressConsts.AddressAlreadyDefault])
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

        // Also setup the specification-based method
        UserAddressRepositoryMock
            .Setup(x => x.ListAsync(It.IsAny<ISpecification<UserAddress>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses ?? new List<UserAddress> { DefaultUserAddress });
    }

    protected void SetupLocalizedMessage(string message)
    {
        LocalizerMock
            .Setup(x => x[message])
            .Returns(message);
    }
} 