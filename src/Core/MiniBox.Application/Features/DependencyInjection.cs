using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MiniBox.Application.Common.Behaviors;
using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Application.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        return services;
    }
}
