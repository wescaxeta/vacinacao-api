namespace Vacinacao.Domain;

/// <summary>Vacinação aplicada no rebanho de uma propriedade, dentro de uma campanha.</summary>
public sealed class RegistroVacinacao
{
    public Guid Id { get; private set; }
    public Guid CampanhaId { get; private set; }
    public CodigoPropriedade Propriedade { get; private set; } = null!;
    public Doenca Doenca { get; private set; }
    public Especie Especie { get; private set; }
    public int QuantidadeAnimais { get; private set; }
    public DateOnly DataAplicacao { get; private set; }
    public DateOnly ValidaAte { get; private set; }
    public string LoteVacina { get; private set; } = string.Empty;
    public string CrmvVeterinario { get; private set; } = string.Empty;

    private RegistroVacinacao() { } // EF Core

    internal static RegistroVacinacao Criar(
        Campanha campanha,
        CodigoPropriedade propriedade,
        int quantidadeAnimais,
        DateOnly dataAplicacao,
        string loteVacina,
        string crmvVeterinario) => new()
        {
            Id = Guid.CreateVersion7(),
            CampanhaId = campanha.Id,
            Propriedade = propriedade,
            Doenca = campanha.Doenca,
            Especie = campanha.Especie,
            QuantidadeAnimais = quantidadeAnimais,
            DataAplicacao = dataAplicacao,
            ValidaAte = dataAplicacao.AddMonths(campanha.ValidadeMeses),
            LoteVacina = loteVacina.Trim(),
            CrmvVeterinario = crmvVeterinario.Trim().ToUpperInvariant(),
        };

    public bool ValidaEm(DateOnly data) => data <= ValidaAte;
}
