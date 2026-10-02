using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vacinacao.Application;

namespace Vacinacao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Vacinacao")
            ?? throw new InvalidOperationException("Connection string \"Vacinacao\" não configurada.");

        services.AddDbContext<VacinacaoDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnidadeDeTrabalho>(sp => sp.GetRequiredService<VacinacaoDbContext>());
        services.AddScoped<ICampanhaRepository, CampanhaRepository>();
        services.AddScoped<IRegistroVacinacaoRepository, RegistroVacinacaoRepository>();

        services.AddHealthChecks().AddDbContextCheck<VacinacaoDbContext>("postgresql");

        return services;
    }
}
