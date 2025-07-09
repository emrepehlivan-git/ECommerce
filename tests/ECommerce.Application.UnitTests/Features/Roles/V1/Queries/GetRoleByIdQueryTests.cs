using ECommerce.Application.Behaviors;

namespace ECommerce.Application.UnitTests.Features.Roles.V1.Queries;

public sealed class GetRoleByIdQueryTests : RoleTestBase
{
    private readonly GetRoleByIdQueryHandler Handler;
    private readonly GetRoleByIdQuery Query;
    private readonly GetRoleByIdQueryValidator Validator;
    private readonly Guid RoleId;

    public GetRoleByIdQueryTests()
    {
        RoleId = Guid.NewGuid();
        Query = new GetRoleByIdQuery(RoleId);

        Handler = new GetRoleByIdQueryHandler(
            RoleServiceMock.Object,
            LazyServiceProviderMock.Object);

        Validator = new GetRoleByIdQueryValidator(LocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRole_ShouldReturnRole()
    {
        // Arrange
        var role = DefaultRole;
        SetupRoleServiceFindByIdAsync(role);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(role.Name);

        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(RoleId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentRole_ShouldReturnError()
    {
        // Arrange
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(LocalizerMock.Object[RoleConsts.RoleNotFound]);

        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(RoleId), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange & Act & Assert
        Query.Should().BeAssignableTo<ICacheableRequest>();
        Query.CacheKey.Should().Be($"roles:id:{RoleId}");
        Query.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task Handle_ShouldMapRoleToDto()
    {
        // Arrange
        var role = Role.Create("TestRoleMapping");
        SetupRoleServiceFindByIdAsync(role);

        // Act
        var result = await Handler.Handle(Query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<RoleDto>();
        result.Value.Name.Should().Be("TestRoleMapping");
        result.Value.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task Validate_WithEmptyId_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetRoleByIdQuery(Guid.Empty);

        // Act
        var validationResult = await Validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == LocalizerMock.Object[RoleConsts.RoleNotFound]);
    }

    [Fact]
    public async Task Validate_WithValidId_ShouldPassValidation()
    {
        // Arrange & Act
        var validationResult = await Validator.ValidateAsync(Query);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }
} 