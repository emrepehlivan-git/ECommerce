using ECommerce.Application.Behaviors;
using ECommerce.Application.Features.Roles;
using ECommerce.Application.Features.Roles.DTOs;
using ECommerce.Application.Features.Roles.Queries;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.UnitTests.Features.Roles.Queries;

public sealed class GetRoleByIdQueryTests : RoleTestBase
{
    private readonly GetRoleByIdQueryHandler _handler;
    private readonly GetRoleByIdQuery _query;
    private readonly GetRoleByIdQueryValidator _validator;
    private readonly Guid _roleId;

    public GetRoleByIdQueryTests()
    {
        _roleId = Guid.NewGuid();
        _query = new GetRoleByIdQuery(_roleId);

        _handler = new GetRoleByIdQueryHandler(
            RoleServiceMock.Object,
            LazyServiceProviderMock.Object);

        _validator = new GetRoleByIdQueryValidator(Localizer);
    }

    [Fact]
    public async Task Handle_WithExistingRole_ShouldReturnRole()
    {
        // Arrange
        var role = DefaultRole;
        SetupRoleServiceFindByIdAsync(role);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(role.Name);

        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(_roleId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentRole_ShouldReturnError()
    {
        // Arrange
        SetupRoleServiceFindByIdAsync(null);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(Localizer[RoleConsts.RoleNotFound]);

        RoleServiceMock.Verify(x => x.FindRoleByIdAsync(_roleId), Times.Once);
    }

    [Fact]
    public void Query_ShouldImplementICacheableRequest()
    {
        // Arrange & Act & Assert
        _query.Should().BeAssignableTo<ICacheableRequest>();
        _query.CacheKey.Should().Be($"roles:id:{_roleId}");
        _query.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task Handle_ShouldMapRoleToDto()
    {
        // Arrange
        var role = Role.Create("TestRoleMapping");
        SetupRoleServiceFindByIdAsync(role);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

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
        var validationResult = await _validator.ValidateAsync(query);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.ErrorMessage == Localizer[RoleConsts.RoleNotFound]);
    }

    [Fact]
    public async Task Validate_WithValidId_ShouldPassValidation()
    {
        // Arrange & Act
        var validationResult = await _validator.ValidateAsync(_query);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }
} 