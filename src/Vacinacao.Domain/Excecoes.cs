namespace Vacinacao.Domain;

/// <summary>
/// Violação de regra de negócio. A API traduz em 422 (Problem Details), sem expor detalhes internos.
/// </summary>
public class RegraDeNegocioException(string message) : Exception(message);

/// <summary>Recurso inexistente. A API traduz em 404.</summary>
public sealed class NaoEncontradoException(string message) : Exception(message);
