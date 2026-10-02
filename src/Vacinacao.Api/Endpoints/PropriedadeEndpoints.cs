using Microsoft.AspNetCore.Http.HttpResults;
using Vacinacao.Application;
using Vacinacao.Domain;

namespace Vacinacao.Api.Endpoints;

internal static class PropriedadeEndpoints
{
    public static void MapPropriedadeEndpoints(this IEndpointRouteBuilder app)
    {
        var propriedades = app.MapGroup("/propriedades/{codigo}").WithTags("Propriedades");

        propriedades.MapGet("/vacinacoes", async Task<Results<Ok<IReadOnlyList<RegistroVacinacaoResponse>>, ValidationProblem>> (
                string codigo, string? especie, VacinacaoService servico, CancellationToken ct) =>
            {
                if (Parametros.Validar(codigo, especie, especieObrigatoria: false, out var filtro) is { } problema)
                {
                    return problema;
                }

                return TypedResults.Ok(await servico.ListarAsync(codigo, filtro, ct));
            })
            .WithSummary("Histórico de vacinações da propriedade");

        propriedades.MapGet("/aptidao", async Task<Results<Ok<AptidaoResponse>, ValidationProblem>> (
                string codigo, string? especie, AptidaoService servico, CancellationToken ct) =>
            {
                if (Parametros.Validar(codigo, especie, especieObrigatoria: true, out var filtro) is { } problema)
                {
                    return problema;
                }

                return TypedResults.Ok(await servico.AvaliarAsync(codigo, filtro!.Value, ct));
            })
            .WithSummary("Informa se o rebanho está apto para transporte (emissão de GTA)")
            .WithDescription("Exige ?especie=. A propriedade fica apta quando não há pendência de vacinação obrigatória com prazo encerrado.");
    }
}

/// <summary>Validação dos parâmetros de rota e de query, no mesmo formato de erro dos corpos de requisição.</summary>
internal static class Parametros
{
    private static readonly string EspeciesAceitas =
        string.Join(", ", Enum.GetNames<Especie>().Select(n => n.ToLowerInvariant()));

    public static ValidationProblem? Validar(string? codigo, string? especie, bool especieObrigatoria, out Especie? filtro)
    {
        var erros = new Dictionary<string, string[]>();

        if (codigo is not null && !CodigoPropriedade.EhValido(codigo))
        {
            erros["codigo"] = ["Use a UF seguida de 6 dígitos (ex.: GO000123)."];
        }

        filtro = null;
        if (string.IsNullOrWhiteSpace(especie))
        {
            if (especieObrigatoria)
            {
                erros["especie"] = [$"Informe a espécie: {EspeciesAceitas}."];
            }
        }
        else if (Enum.TryParse<Especie>(especie, ignoreCase: true, out var convertida) && Enum.IsDefined(convertida))
        {
            filtro = convertida;
        }
        else
        {
            erros["especie"] = [$"Espécie inválida. Valores aceitos: {EspeciesAceitas}."];
        }

        return erros.Count == 0 ? null : TypedResults.ValidationProblem(erros, title: "Um ou mais parâmetros são inválidos.");
    }
}
