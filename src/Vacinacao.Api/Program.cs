using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Vacinacao.Api;
using Vacinacao.Api.Endpoints;
using Vacinacao.Application;
using Vacinacao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeExcecoes>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "Vacinação de Rebanhos API";
    document.Info.Description =
        "Campanhas de vacinação, registros por propriedade e aptidão do rebanho para transporte (GTA).";
    return Task.CompletedTask;
}));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Vacinação de Rebanhos API"));
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapHealthChecks("/health");
app.MapCampanhaEndpoints();
app.MapPropriedadeEndpoints();

await PrepararBancoAsync(app);

app.Run();

// Em produção, as migrations rodariam no pipeline de deploy; aqui rodam na subida para facilitar a demonstração.
static async Task PrepararBancoAsync(WebApplication app)
{
    if (!app.Configuration.GetValue("Banco:AplicarMigrationsNaSubida", true))
    {
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<VacinacaoDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue("Banco:DadosDemonstracao", false))
    {
        await DadosDemonstracao.PopularAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>());
    }
}

public partial class Program;
