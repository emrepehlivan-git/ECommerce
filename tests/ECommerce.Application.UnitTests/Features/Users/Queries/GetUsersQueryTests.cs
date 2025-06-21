using ECommerce.Application.Features.Users.Queries;
using ECommerce.Application.Parameters;

namespace ECommerce.Application.UnitTests.Features.Users.Queries;

public sealed class GetUsersQueryTests : UserQueriesTestBase
{
    private readonly GetUsersQueryHandler Handler;
    private readonly GetUsersQuery Query;

    public GetUsersQueryTests()
    {
        Query = new GetUsersQuery(new PageableRequestParams());

        Handler = new GetUsersQueryHandler(
            UserServiceMock.Object,
            LazyServiceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedUsers()
    {
        var users = new List<User> { DefaultUser };
        SetupUsersQuery(users);

        var result = await Handler.Handle(Query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(DefaultUser.Id);
        result.Value.First().Email.Should().Be(DefaultUser.Email);
        result.Value.First().FullName.Should().Be(DefaultUser.FullName.ToString());
        result.Value.First().IsActive.Should().Be(DefaultUser.IsActive);
    }

    [Fact]
    public async Task Handle_WithEmptyUsers_ShouldReturnEmptyList()
    {
        var emptyUsers = new List<User>();
        SetupUsersQuery(emptyUsers);

        var result = await Handler.Handle(Query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPaging_ShouldReturnPagedResults()
    {
        var users = new List<User>
        {
            DefaultUser,
            User.Create("test2@example.com", "Test User 2", "Password123!"),
            User.Create("test3@example.com", "Test User 3", "Password123!")
        };
        SetupUsersQuery(users);

        var pagingParams = new PageableRequestParams { PageSize = 2, Page = 1 };
        var pagedQuery = new GetUsersQuery(pagingParams);

        var result = await Handler.Handle(pagedQuery, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }
}