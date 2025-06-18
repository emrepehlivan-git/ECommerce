using ECommerce.Application.Helpers;
using ECommerce.SharedKernel.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ILazyServiceProvider LazyServiceProvider => HttpContext.RequestServices.GetRequiredService<ILazyServiceProvider>();
    protected ISender Mediator => LazyServiceProvider.LazyGetRequiredService<ISender>();
    protected LocalizationHelper Localizer => LazyServiceProvider.LazyGetRequiredService<LocalizationHelper>();
}

