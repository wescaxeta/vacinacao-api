using Vacinacao.Domain;

namespace Vacinacao.Application;

public sealed record CriarCampanhaRequest(
    string Nome,
    Doenca Doenca,
    Especie Especie,
    DateOnly Inicio,
    DateOnly Fim,
    int ValidadeMeses,
    bool Obrigatoria);

public sealed record CampanhaResponse(
    Guid Id,
    string Nome,
    Doenca Doenca,
    Especie Especie,
    DateOnly Inicio,
    DateOnly Fim,
    int ValidadeMeses,
    bool Obrigatoria)
{
    public static CampanhaResponse De(Campanha c) =>
        new(c.Id, c.Nome, c.Doenca, c.Especie, c.Inicio, c.Fim, c.ValidadeMeses, c.Obrigatoria);
}

public sealed record RegistrarVacinacaoRequest(
    string CodigoPropriedade,
    int QuantidadeAnimais,
    DateOnly DataAplicacao,
    string LoteVacina,
    string CrmvVeterinario);

public sealed record RegistroVacinacaoResponse(
    Guid Id,
    Guid CampanhaId,
    string CodigoPropriedade,
    Doenca Doenca,
    Especie Especie,
    int QuantidadeAnimais,
    DateOnly DataAplicacao,
    DateOnly ValidaAte,
    string LoteVacina,
    string CrmvVeterinario)
{
    public static RegistroVacinacaoResponse De(RegistroVacinacao r) =>
        new(r.Id, r.CampanhaId, r.Propriedade.Valor, r.Doenca, r.Especie, r.QuantidadeAnimais,
            r.DataAplicacao, r.ValidaAte, r.LoteVacina, r.CrmvVeterinario);
}

public sealed record AptidaoResponse(
    string CodigoPropriedade,
    Especie Especie,
    bool Apta,
    DateOnly AvaliadaEm,
    IReadOnlyList<CoberturaVacinal> Coberturas,
    IReadOnlyList<string> Pendencias,
    IReadOnlyList<string> Avisos)
{
    public static AptidaoResponse De(Aptidao a) =>
        new(a.Propriedade.Valor, a.Especie, a.Apta, a.AvaliadaEm, a.Coberturas, a.Pendencias, a.Avisos);
}
