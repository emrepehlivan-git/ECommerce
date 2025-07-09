using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;

namespace ECommerce.Application.UnitTests.Features.Users.V1.Commands;

public class UserCommandsTestBase
{
    protected Guid UserId = Guid.Parse("e64db34c-7455-41da-b255-a9a7a46ace54");
    protected static User DefaultUser => User.Create("test@example.com", "Test User", "Password123!");

    protected Mock<IUserService> UserServiceMock;
    protected Mock<ILazyServiceProvider> LazyServiceProviderMock;
    protected Mock<ILocalizationHelper> LocalizerMock;

    protected UserCommandsTestBase()
    {
        UserServiceMock = new Mock<IUserService>();
        LazyServiceProviderMock = new Mock<ILazyServiceProvider>();
        LocalizerMock = new Mock<ILocalizationHelper>();

        LazyServiceProviderMock
            .Setup(x => x.LazyGetRequiredService<ILocalizationHelper>())
            .Returns(LocalizerMock.Object);

        SetupDefaultLocalizationMessages();
    }

    protected void SetupDefaultLocalizationMessages()
    {
        LocalizerMock
            .Setup(x => x[UserConsts.NotFound])
            .Returns("User not found");
    }

    protected void SetupUserExists(bool exists = true)
    {
        UserServiceMock
            .Setup(x => x.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(exists ? DefaultUser : null);
    }

    protected void SetupLocalizedMessage(string message)
    {
        LocalizerMock
            .Setup(x => x[message])
            .Returns(message);
    }
}