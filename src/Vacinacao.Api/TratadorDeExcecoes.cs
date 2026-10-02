using Microsoft.AspNetCore.Diagnostics;
using Vacinacao.Domain;

namespace Vacinacao.Api;

/// <summary>
/// Converte exceções de domínio em Problem Details (RFC 9457). Exceções inesperadas seguem para o
/// tratador padrão, que responde 500 sem expor stack trace.
/// </summary>
internal sealed class TratadorDeExcecoes(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, titulo) = exception switch
        {
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        context.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = { Status = status, Title = titulo, Detail = exception.Message },
        });
    }
}
