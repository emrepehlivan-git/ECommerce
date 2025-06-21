using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Services;

public interface IAuthenticationService
{
    Task<SignInResult> PasswordSignInAsync(string email, string password, bool isPersistent, bool lockoutOnFailure);
    Task SignOutAsync();
    Task<bool> CheckPasswordAsync(User user, string password);
} 