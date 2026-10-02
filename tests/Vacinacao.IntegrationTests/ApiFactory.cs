using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace Vacinacao.IntegrationTests;

/// <summary>
/// Sobe a API completa contra um PostgreSQL real. Por padrão o banco é criado pelo Testcontainers
/// (exige Docker). Se a variável TEST_CONNECTION_STRING existir, usa esse banco.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly DateTimeOffset Agora = new(2026, 3, 10, 15, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlContainer? _container =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING") is null
            ? new PostgreSqlBuilder("postgres:17-alpine").Build()
            : null;

    private string ConnectionString =>
        _container?.GetConnectionString() ?? Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")!;

    public async Task InitializeAsync()
    {
        if (_container is not null)
        {
            await _container.StartAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Vacinacao", ConnectionString);
        builder.UseSetting("Banco:DadosDemonstracao", "false");

        builder.ConfigureServices(services =>
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(new FakeTimeProvider(Agora))));
    }
}

[CollectionDefinition(Nome)]
public sealed class ColecaoApi : ICollectionFixture<ApiFactory>
{
    public const string Nome = "API com PostgreSQL";
}
