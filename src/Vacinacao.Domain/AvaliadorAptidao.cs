namespace Vacinacao.Domain;

public sealed record CoberturaVacinal(Doenca Doenca, DateOnly ValidaAte);

public sealed record Aptidao(
    CodigoPropriedade Propriedade,
    Especie Especie,
    DateOnly AvaliadaEm,
    IReadOnlyList<CoberturaVacinal> Coberturas,
    IReadOnlyList<string> Pendencias,
    IReadOnlyList<string> Avisos)
{
    /// <summary>Apta para transporte (emissão de GTA) quando não há nenhuma pendência.</summary>
    public bool Apta => Pendencias.Count == 0;
}

/// <summary>
/// Decide se o rebanho de uma propriedade está apto para transporte, doença a doença:
/// <list type="bullet">
/// <item>com vacinação válida para a doença, a doença está coberta;</item>
/// <item>sem vacinação válida e com o prazo de alguma campanha obrigatória já encerrado, há pendência e o transporte fica bloqueado;</item>
/// <item>sem vacinação válida e com a campanha ainda em andamento, há apenas um aviso, porque o produtor ainda está no prazo.</item>
/// </list>
/// </summary>
public static class AvaliadorAptidao
{
    public static Aptidao Avaliar(
        CodigoPropriedade propriedade,
        Especie especie,
        IEnumerable<Campanha> campanhasObrigatorias,
        IEnumerable<RegistroVacinacao> registros,
        DateOnly hoje)
    {
        var registrosDaEspecie = registros.Where(r => r.Especie == especie && r.Propriedade == propriedade).ToList();
        var coberturas = new List<CoberturaVacinal>();
        var pendencias = new List<string>();
        var avisos = new List<string>();

        var porDoenca = campanhasObrigatorias
            .Where(c => c.Obrigatoria && c.Especie == especie && c.Inicio <= hoje)
            .GroupBy(c => c.Doenca)
            .OrderBy(g => g.Key);

        foreach (var campanhasDaDoenca in porDoenca)
        {
            var doenca = campanhasDaDoenca.Key;
            var validadeMaisLonga = registrosDaEspecie
                .Where(r => r.Doenca == doenca && r.ValidaEm(hoje))
                .Select(r => (DateOnly?)r.ValidaAte)
                .Max();

            if (validadeMaisLonga is { } validaAte)
            {
                coberturas.Add(new CoberturaVacinal(doenca, validaAte));
                continue;
            }

            var prazoEncerrado = campanhasDaDoenca.Where(c => c.PrazoEncerrado(hoje)).MaxBy(c => c.Fim);
            if (prazoEncerrado is not null)
            {
                pendencias.Add(
                    $"Sem vacinação válida contra {doenca}: o prazo da campanha \"{prazoEncerrado.Nome}\" terminou em {prazoEncerrado.Fim:dd/MM/yyyy}.");
                continue;
            }

            var emAndamento = campanhasDaDoenca.Where(c => c.EmAndamento(hoje)).MinBy(c => c.Fim);
            if (emAndamento is not null)
            {
                avisos.Add(
                    $"Campanha \"{emAndamento.Nome}\" em andamento: vacine contra {doenca} até {emAndamento.Fim:dd/MM/yyyy}.");
            }
        }

        return new Aptidao(propriedade, especie, hoje, coberturas, pendencias, avisos);
    }
}
