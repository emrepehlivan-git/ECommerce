using ECommerce.Application.Features.Users.Commands;
using ECommerce.Application.Features.Users;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.UnitTests.Features.Users.Commands;

public sealed class UpdateUserBirthdayCommandTests : UserCommandsTestBase
{
    private readonly UpdateUserBirthdayCommandHandler _handler;

    public UpdateUserBirthdayCommandTests()
    {
        // UserConsts.NotFound için doğru mesajı döndür
        LocalizationServiceMock.Setup(x => x.GetLocalizedString(UserConsts.NotFound)).Returns("User not found");
        
        UserServiceMock.Setup(x => x.UpdateBirthdayAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>()))
            .ReturnsAsync((Guid userId, DateTime? birthday) => 
            {
                if (userId == Guid.Empty)
                    return Result.Error(LocalizationServiceMock.Object.GetLocalizedString(UserConsts.NotFound));
                return Result.Success();
            });

        _handler = new UpdateUserBirthdayCommandHandler(
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUpdateIsSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var birthday = new DateTime(1990, 1, 1);
        UserServiceMock.Setup(x => x.UpdateBirthdayAsync(userId, birthday))
            .ReturnsAsync(Result.Success());
        var command = new UpdateUserBirthdayCommand(userId, birthday);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        UserServiceMock.Verify(x => x.UpdateBirthdayAsync(userId, birthday), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenUpdateFails()
    {
        // Arrange
        var userId = Guid.Empty; // NotFound durumu için
        var birthday = new DateTime(1990, 1, 1);
        var command = new UpdateUserBirthdayCommand(userId, birthday);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("User not found"));
        UserServiceMock.Verify(x => x.UpdateBirthdayAsync(userId, birthday), Times.Once);
    }
} 