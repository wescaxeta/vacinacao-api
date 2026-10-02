using Vacinacao.Domain;

namespace Vacinacao.Application;

public interface ICampanhaRepository
{
    void Adicionar(Campanha campanha);

    Task<Campanha?> ObterAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<Campanha>> ListarAsync(Especie? especie, CancellationToken ct);

    Task<IReadOnlyList<Campanha>> ListarObrigatoriasAsync(Especie especie, CancellationToken ct);
}

public interface IRegistroVacinacaoRepository
{
    void Adicionar(RegistroVacinacao registro);

    Task<IReadOnlyList<RegistroVacinacao>> ListarPorPropriedadeAsync(
        CodigoPropriedade propriedade, Especie? especie, CancellationToken ct);
}

public interface IUnidadeDeTrabalho
{
    Task SalvarAsync(CancellationToken ct);
}

public static class RelogioExtensions
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>Data de hoje no horário de Brasília, independente do fuso do servidor.</summary>
    public static DateOnly Hoje(this TimeProvider relogio) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(relogio.GetUtcNow(), Brasilia).DateTime);
}
