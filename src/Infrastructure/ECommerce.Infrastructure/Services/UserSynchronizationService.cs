using Ardalis.Result;
using ECommerce.Application.Common.Logging;
using ECommerce.Application.Extensions;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ECommerce.Infrastructure.Services;

public class UserSynchronizationService(
    UserManager<User> userManager,
    IECommerceLogger<UserSynchronizationService> logger,
    IServiceProvider serviceProvider) : IUserSynchronizationService, IScopedDependency
{
    public async Task<Result<User>> SyncUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var subjectId = principal.GetUserId();
        if (subjectId == Guid.Empty)
        {
            return Result<User>.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(subjectId.ToString());

        if (user is not null)
        {
            return Result<User>.Success(user);
        }

        logger.LogInformation("User with ID {SubjectId} not found locally. Creating new user.", subjectId);

        var email = principal.GetEmail();
        if (string.IsNullOrEmpty(email))
        {
            logger.LogWarning("Email claim is missing for user {SubjectId}. Cannot create user.", subjectId);
            return Result<User>.Error("Email is required to create a user.");
        }
        
        var firstName = principal.GetFirstName();
        var lastName = principal.GetLastName();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            var name = principal.FindFirst("name")?.Value;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameParts = name.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = nameParts.Length > 0 ? nameParts[0]! : firstName;
                lastName = nameParts.Length > 1 ? nameParts[1]! : lastName;
            }
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            logger.LogWarning("First name or last name could not be determined from the token for user {SubjectId}. Cannot create user.", subjectId);
            return Result<User>.Error("First name and last name are required to create a user.");
        }
        
        var newUser = User.Create(email, firstName, lastName);
        newUser.Id = subjectId;
        newUser.UserName = email;
        newUser.EmailConfirmed = true;
        
        var createUserResult = await userManager.CreateAsync(newUser);
        
        if (!createUserResult.Succeeded)
        {
            logger.LogError("Failed to create user {Email}. Errors: {Errors}", email, string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
            return Result<User>.Error(createUserResult.Errors.Select(e => e.Description).ToArray());
        }

        var addToRoleResult = await userManager.AddToRoleAsync(newUser, "Customer");
        if (!addToRoleResult.Succeeded)
        {
            logger.LogWarning("Failed to add 'Customer' role to user {Email}", email);
        }
        
        logger.LogInformation("Successfully created and provisioned user {Email} with ID {UserId}", email, newUser.Id);

        await SyncUserPermissionsToKeycloakAsync(newUser.Id.ToString());
        
        return Result<User>.Success(newUser);
    }

    private async Task SyncUserPermissionsToKeycloakAsync(string userId)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var permissionSyncService = scope.ServiceProvider.GetService<KeycloakPermissionSyncService>();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            
            if (permissionSyncService == null)
            {
                logger.LogWarning("KeycloakPermissionSyncService bulunamadı, permission sync atlanıyor");
                return;
            }

            var userPermissions = await permissionService.GetUserPermissionsAsync(Guid.Parse(userId));
            
            await permissionSyncService.AssignPermissionsToKeycloakUserAsync(userId, userPermissions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "User {UserId} için permission sync hatası", userId);
        }
    }
} 