using System.Text.Json;
using FluentValidation;

namespace Vacinacao.Api;

/// <summary>
/// Valida o corpo da requisição com o validador FluentValidation correspondente e responde
/// 400 com os erros agrupados por campo (formato ValidationProblemDetails).
/// </summary>
internal sealed class FiltroDeValidacao<T>(IValidator<T> validador) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var corpo = context.Arguments.OfType<T>().FirstOrDefault();
        if (corpo is null)
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Corpo da requisição ausente ou inválido.");
        }

        var resultado = await validador.ValidateAsync(corpo, context.HttpContext.RequestAborted);
        if (!resultado.IsValid)
        {
            // Chaves em camelCase, iguais aos nomes dos campos no JSON
            var erros = resultado.ToDictionary().ToDictionary(
                par => JsonNamingPolicy.CamelCase.ConvertName(par.Key),
                par => par.Value);

            return Results.ValidationProblem(erros, title: "Um ou mais campos são inválidos.");
        }

        return await next(context);
    }
}

internal static class FiltroDeValidacaoExtensions
{
    public static RouteHandlerBuilder ComValidacao<T>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<FiltroDeValidacao<T>>().ProducesValidationProblem();
}
