using System.Security.Claims;
using Ardalis.Result;
using ECommerce.Application.Features.Users;
using ECommerce.Application.Helpers;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.SharedKernel.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Services;

public sealed class UserService : IUserService, IScopedDependency
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILocalizationService _localizationService;

    public UserService(UserManager<User> userManager, SignInManager<User> signInManager, ILocalizationService localizationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _localizationService = localizationService;
    }

    public IQueryable<User> Users => _userManager.Users;

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> FindByIdAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<User?> GetUserByPrincipalAsync(ClaimsPrincipal principal)
    {
        return await _userManager.GetUserAsync(principal);
    }

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        return await _userManager.UpdateAsync(user);
    }

    public async Task<bool> CanSignInAsync(User user)
    {
        return await _signInManager.CanSignInAsync(user);
    }

    public async Task<Result> UpdateBirthdayAsync(Guid userId, DateTime? birthday)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Error(_localizationService.GetLocalizedString(UserConsts.NotFound));
        user.UpdateBirthday(birthday);
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            return Result.Success();
        return Result.Error(result.Errors.Select(e => e.Description).ToArray());
    }
} 