using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Vacinacao.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CampanhaService>();
        services.AddScoped<VacinacaoService>();
        services.AddScoped<AptidaoService>();
        services.AddValidatorsFromAssemblyContaining<CriarCampanhaValidator>(ServiceLifetime.Singleton);
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
