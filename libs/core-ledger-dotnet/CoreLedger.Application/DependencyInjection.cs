using System.Reflection;
using CoreLedger.Application.Behaviors;
using CoreLedger.Domain.Cadastros.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLedger.Application;

/// <summary>
///     Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg => { }, assembly);
        services.AddValidatorsFromAssembly(assembly);

        // Register Domain Services
        services.AddScoped<FundoDomainService>();

        return services;
    }
}