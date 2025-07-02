using ECommerce.Application.Helpers;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Users.V1.Queries;

public abstract class UserQueriesTestBase
{
    protected readonly Mock<IUserService> UserServiceMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly Mock<ILocalizationService> LocalizationServiceMock;
    protected readonly User DefaultUser;
    protected readonly LocalizationHelper Localizer;
    protected UserQueriesTestBase()
    {
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizationServiceMock = new Mock<ILocalizationService>();

        Localizer = new LocalizationHelper(LocalizationServiceMock.Object);

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<LocalizationHelper>())
            .Returns(Localizer);

        DefaultUser = User.Create("test@example.com", "Test User", "Password123!");
    }

    protected void SetupUserExists(bool exists = true)
    {
        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(exists ? DefaultUser : null);
    }

    protected void SetupUsersQuery(IEnumerable<User> users)
    {
        var queryable = users.AsQueryable();
        UserServiceMock
            .Setup(x => x.Users)
            .Returns(queryable);
    }
}