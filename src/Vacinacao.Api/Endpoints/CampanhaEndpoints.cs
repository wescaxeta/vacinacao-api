using Microsoft.AspNetCore.Http.HttpResults;
using Vacinacao.Application;

namespace Vacinacao.Api.Endpoints;

internal static class CampanhaEndpoints
{
    public static void MapCampanhaEndpoints(this IEndpointRouteBuilder app)
    {
        var campanhas = app.MapGroup("/campanhas").WithTags("Campanhas");

        campanhas.MapPost("/", async (CriarCampanhaRequest request, CampanhaService servico, CancellationToken ct) =>
            {
                var campanha = await servico.CriarAsync(request, ct);
                return TypedResults.Created($"/campanhas/{campanha.Id}", campanha);
            })
            .ComValidacao<CriarCampanhaRequest>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Cria uma campanha de vacinação");

        campanhas.MapGet("/", async Task<Results<Ok<IReadOnlyList<CampanhaResponse>>, ValidationProblem>> (
                string? especie, CampanhaService servico, CancellationToken ct) =>
            {
                if (Parametros.Validar(codigo: null, especie, especieObrigatoria: false, out var filtro) is { } problema)
                {
                    return problema;
                }

                return TypedResults.Ok(await servico.ListarAsync(filtro, ct));
            })
            .WithSummary("Lista as campanhas, opcionalmente filtradas por espécie");

        campanhas.MapGet("/{id:guid}", async (Guid id, CampanhaService servico, CancellationToken ct) =>
                TypedResults.Ok(await servico.ObterAsync(id, ct)))
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Consulta uma campanha");

        campanhas.MapPost("/{id:guid}/vacinacoes", async (
                Guid id, RegistrarVacinacaoRequest request, VacinacaoService servico, CancellationToken ct) =>
            {
                var registro = await servico.RegistrarAsync(id, request, ct);
                return TypedResults.Created($"/propriedades/{registro.CodigoPropriedade}/vacinacoes", registro);
            })
            .ComValidacao<RegistrarVacinacaoRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Registra a vacinação do rebanho de uma propriedade na campanha");
    }
}
