using ECommerce.Application.Features.Users.Queries;

namespace ECommerce.Application.UnitTests.Features.Users.Queries;

public sealed class GetUserByIdQueryTests : UserQueriesTestBase
{
    private readonly GetUserByIdQueryHandler Handler;
    private readonly GetUserByIdQuery Query;

    public GetUserByIdQueryTests()
    {   
        Query = new GetUserByIdQuery(Guid.NewGuid());

        Handler = new GetUserByIdQueryHandler(
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnUser()
    {
        SetupUserExists(true);

        var result = await Handler.Handle(Query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(DefaultUser.Id);
        result.Value.Email.Should().Be(DefaultUser.Email);
        result.Value.FullName.Should().Be(DefaultUser.FullName.ToString());
        result.Value.IsActive.Should().Be(DefaultUser.IsActive);
    }

    [Fact]
    public async Task Handle_WithInvalidQuery_ShouldReturnNotFound()
    {
        SetupUserExists(false);

        var result = await Handler.Handle(Query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }
}