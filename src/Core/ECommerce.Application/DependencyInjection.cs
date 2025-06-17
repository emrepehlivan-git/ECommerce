using ECommerce.Application.Behaviors;
using ECommerce.Application.Features.Categories.Commands;
using ECommerce.Application.Features.Categories;
using ECommerce.Application.Helpers;
using ECommerce.Application.Mappings;
using ECommerce.SharedKernel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddDependencies(assembly);

        services.AddValidatorsFromAssemblyContaining<CreateCategoryCommandValidator>(includeInternalTypes: true);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionalRequestBehavior<,>));
        });

        services.AddMapsterConfiguration();

        return services;
    }
}