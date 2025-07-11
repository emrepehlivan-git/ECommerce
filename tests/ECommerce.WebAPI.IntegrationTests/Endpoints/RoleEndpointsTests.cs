using ECommerce.Application.Features.Roles.V1.Commands;
using ECommerce.Application.Features.Roles.V1.DTOs;
using ECommerce.WebAPI.IntegrationTests.Common;
using System.Text.Json;
using Xunit;

namespace ECommerce.WebAPI.IntegrationTests.Endpoints;

public class RoleEndpointsTests : BaseIntegrationTest
{
    public RoleEndpointsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        
        // Act
        var response = await unauthenticatedClient.GetAsync("/api/v1/Role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetRoleById_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        
        // Act
        var response = await unauthenticatedClient.GetAsync($"/api/v1/Role/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserRoles_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        
        // Act
        var response = await unauthenticatedClient.GetAsync($"/api/v1/Role/user/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        var command = new { Name = "TestRole" };

        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync("/api/v1/Role", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        var command = new { Name = "UpdatedRole" };

        // Act
        var response = await unauthenticatedClient.PutAsJsonAsync($"/api/v1/Role/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        
        // Act
        var response = await unauthenticatedClient.DeleteAsync($"/api/v1/Role/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserToRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        
        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync($"/api/v1/Role/user/{Guid.NewGuid()}/add-role", new { roleId = Guid.NewGuid()});

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveUserFromRole_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        
        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync($"/api/v1/Role/user/{Guid.NewGuid()}/remove-role", new { roleId = Guid.NewGuid()});

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteRoles_Many_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedClient = Factory.CreateUnauthenticatedClient();
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var response = await unauthenticatedClient.PostAsJsonAsync("/api/v1/Role/delete-many", ids);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRoles_WithAuth_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateRoleAndGetId("TestRole");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var response = await Client.GetAsync("/api/v1/Role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var pagedResult = await response.Content.ReadFromJsonAsync<object>(options);
        pagedResult.Should().NotBeNull();
    }
    
    [Fact]
    public async Task CreateRole_WithValidData_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var command = new CreateRoleCommand("Test Role");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Role", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roleId = await response.Content.ReadFromJsonAsync<Guid>();
        roleId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateRole_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var command = new CreateRoleCommand("");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Role", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleName = "Duplicate Role";
        await CreateRoleAndGetId(roleName);
        var command = new CreateRoleCommand(roleName);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Role", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetRoleById_WithExistingRole_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleId = await CreateRoleAndGetId("Viewer");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var response = await Client.GetAsync($"/api/v1/Role/{roleId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var role = await response.Content.ReadFromJsonAsync<RoleDto>(options);
        role.Should().NotBeNull();
        role?.Name.Should().Be("Viewer");
    }

    [Fact]
    public async Task GetRoleById_WithNonExistingRole_ReturnsUnprocessableEntity()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"/api/v1/Role/{Guid.NewGuid()}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateRole_WithValidData_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleId = await CreateRoleAndGetId("Editor");
        var command = new UpdateRoleCommand(roleId, "Super Editor");

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"/api/v1/Role/{roleId}", command);
        
        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRoleResponse = await Client.GetAsync($"/api/v1/Role/{roleId}");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var updatedRole = await updatedRoleResponse.Content.ReadFromJsonAsync<RoleDto>(options);
        updatedRole?.Name.Should().Be("Super Editor");
    }

    [Fact]
    public async Task UpdateRole_WithNonExistentRole_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentRoleId = Guid.NewGuid();
        var command = new UpdateRoleCommand(nonExistentRoleId, "Non Existent Role");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/Role/{nonExistentRoleId}", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteRole_WithExistingRole_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleId = await CreateRoleAndGetId("Deletable");
        
        // Act
        var response = await Client.DeleteAsync($"/api/v1/Role/{roleId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteRole_WithNonExistentRole_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();
        var nonExistentRoleId = Guid.NewGuid();
        
        // Act
        var response = await Client.DeleteAsync($"/api/v1/Role/{nonExistentRoleId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetUserRoles_WithValidUserId_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleId = await CreateRoleAndGetId("TestRole");
        var userId = Guid.Parse(TestAuthHandler.TestUserId);
        await Client.PostAsJsonAsync($"/api/v1/Role/user/{userId}/add-role", new { roleId });
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var response = await Client.GetAsync($"/api/v1/Role/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userRoles = await response.Content.ReadFromJsonAsync<UserRoleDto>(options);
        userRoles.Should().NotBeNull();
        userRoles?.Roles.Should().Contain("TestRole");
    }

    [Fact]
    public async Task AddAndRemoveUserFromRole_ShouldSucceed()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleId = await CreateRoleAndGetId("Member");
        var userId = Guid.Parse(TestAuthHandler.TestUserId);

        // Act: Add user to role
        var addResponse = await Client.PostAsJsonAsync($"/api/v1/Role/user/{userId}/add-role", new { roleId });
        
        // Assert: Add
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var userRoles = await Client.GetFromJsonAsync<UserRoleDto>($"/api/v1/Role/user/{userId}", options);
        userRoles.Should().NotBeNull();
        userRoles?.Roles.Should().Contain("Member");
        
        // Act: Remove user from role
        var removeResponse = await Client.PostAsJsonAsync($"/api/v1/Role/user/{userId}/remove-role", new { roleId });
        
        // Assert: Remove
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userRolesAfterRemove = await Client.GetFromJsonAsync<UserRoleDto>($"/api/v1/Role/user/{userId}", options);
        userRolesAfterRemove.Should().NotBeNull();
        userRolesAfterRemove?.Roles.Should().NotContain("Member");
    }

    [Fact]
    public async Task DeleteRoles_WithValidIds_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        var roleId1 = await CreateRoleAndGetId("Role1");
        var roleId2 = await CreateRoleAndGetId("Role2");
        var roleIds = new List<Guid> { roleId1, roleId2 };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/Role/delete-many", roleIds);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify roles are deleted
        var getRoleResponse1 = await Client.GetAsync($"/api/v1/Role/{roleId1}");
        var getRoleResponse2 = await Client.GetAsync($"/api/v1/Role/{roleId2}");
        getRoleResponse1.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        getRoleResponse2.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetRoles_WithIncludePermissions_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();
        await CreateRoleAndGetId("TestRoleWithPermissions");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var response = await Client.GetAsync("/api/v1/Role?includePermissions=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var pagedResult = await response.Content.ReadFromJsonAsync<object>(options);
        pagedResult.Should().NotBeNull();
    }
    
    private async Task<Guid> CreateRoleAndGetId(string roleName)
    {
        var command = new CreateRoleCommand(roleName);
        var response = await Client.PostAsJsonAsync("/api/v1/Role", command);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}

public class PagedResult<T>
{
    public T Data { get; set; } = default!;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
} 