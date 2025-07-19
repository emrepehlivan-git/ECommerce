using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("GlobalRateLimit")]
public abstract class BaseApiV1Controller : ControllerBase
{
    protected ILazyServiceProvider LazyServiceProvider => HttpContext.RequestServices.GetRequiredService<ILazyServiceProvider>();
    protected ISender Mediator => LazyServiceProvider.LazyGetRequiredService<ISender>();
    protected ILocalizationHelper Localizer => LazyServiceProvider.LazyGetRequiredService<ILocalizationHelper>();
} 