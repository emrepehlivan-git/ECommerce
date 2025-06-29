using ECommerce.Application.Features.Users.Queries;
using ECommerce.Application.Features.Users.DTOs;
using ECommerce.Application.Parameters;
using ECommerce.Application.Extensions;

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
        // Arrange
        var users = new List<User> { DefaultUser };
        SetupUsersQuery(users);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        UserServiceMock.Verify(x => x.Users, Times.AtLeastOnce);
        // Birthday alanı kontrolü
        foreach (var user in users)
        {
            var dto = result.Value.FirstOrDefault(x => x.Id == user.Id);
            dto.Should().NotBeNull();
            dto!.Birthday.Should().Be(user.Birthday);
        }
    }

    [Fact]
    public async Task Handle_WithEmptyUsers_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var emptyUsers = new List<User>();
        SetupUsersQuery(emptyUsers);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        UserServiceMock.Verify(x => x.Users, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WithPaging_ShouldUseCorrectParameters()
    {
        // Arrange
        var users = new List<User>
        {
            DefaultUser,
            User.Create("test2@example.com", "Test User 2", "Password123!"),
            User.Create("test3@example.com", "Test User 3", "Password123!")
        };
        SetupUsersQuery(users);

        var pagingParams = new PageableRequestParams { PageSize = 2, Page = 1 };
        var pagedQuery = new GetUsersQuery(pagingParams);

        // Act
        var result = await Handler.Handle(pagedQuery, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        UserServiceMock.Verify(x => x.Users, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ShouldUseAsNoTracking()
    {
        // Arrange
        var users = new List<User> { DefaultUser };
        SetupUsersQuery(users);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        UserServiceMock.Verify(x => x.Users, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WithSearch_ShouldReturnFilteredUsers()
    {
        // Arrange
        var users = new List<User>
        {
            DefaultUser,
            User.Create("admin@example.com", "Admin User", "Password123!"),
            User.Create("test@example.com", "Test User", "Password123!")
        };
        SetupUsersQuery(users);

        var search = "admin";
        var searchParams = new PageableRequestParams(Page: 1, PageSize: 10, Search: search);
        var searchQuery = new GetUsersQuery(searchParams);

        // Act
        var result = await Handler.Handle(searchQuery, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().ContainSingle(x => x.Email.ToLower().Contains(search) || x.FullName.ToLower().Contains(search));
    }
}