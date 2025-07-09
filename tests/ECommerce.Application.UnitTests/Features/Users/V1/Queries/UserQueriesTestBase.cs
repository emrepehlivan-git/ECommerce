using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Users.V1.Queries;

public abstract class UserQueriesTestBase
{
    protected readonly Mock<IUserService> UserServiceMock;
    protected readonly Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected readonly User DefaultUser;
    protected readonly Mock<ILocalizationHelper> LocalizerMock;

    protected UserQueriesTestBase()
    {
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

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