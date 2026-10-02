namespace Vacinacao.Domain;

/// <summary>
/// Campanha de vacinação definida pelo órgão de defesa agropecuária: qual doença, para qual espécie,
/// em que período e por quanto tempo a vacina aplicada nela continua válida.
/// </summary>
public sealed class Campanha
{
    public const int ValidadeMaximaMeses = 60;

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Doenca Doenca { get; private set; }
    public Especie Especie { get; private set; }
    public DateOnly Inicio { get; private set; }
    public DateOnly Fim { get; private set; }
    public int ValidadeMeses { get; private set; }
    public bool Obrigatoria { get; private set; }

    private Campanha() { } // EF Core

    public static Campanha Criar(
        string nome,
        Doenca doenca,
        Especie especie,
        DateOnly inicio,
        DateOnly fim,
        int validadeMeses,
        bool obrigatoria)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException("Nome da campanha é obrigatório.");
        }

        if (fim < inicio)
        {
            throw new RegraDeNegocioException("Fim da campanha não pode ser anterior ao início.");
        }

        if (validadeMeses is < 1 or > ValidadeMaximaMeses)
        {
            throw new RegraDeNegocioException(
                $"Validade da vacina deve ficar entre 1 e {ValidadeMaximaMeses} meses.");
        }

        return new Campanha
        {
            Id = Guid.CreateVersion7(),
            Nome = nome.Trim(),
            Doenca = doenca,
            Especie = especie,
            Inicio = inicio,
            Fim = fim,
            ValidadeMeses = validadeMeses,
            Obrigatoria = obrigatoria,
        };
    }

    /// <summary>O prazo da campanha acabou: a partir daqui, quem não vacinou fica irregular.</summary>
    public bool PrazoEncerrado(DateOnly hoje) => hoje > Fim;

    public bool EmAndamento(DateOnly hoje) => hoje >= Inicio && hoje <= Fim;

    public RegistroVacinacao RegistrarVacinacao(
        CodigoPropriedade propriedade,
        int quantidadeAnimais,
        DateOnly dataAplicacao,
        string loteVacina,
        string crmvVeterinario,
        DateOnly hoje)
    {
        if (dataAplicacao > hoje)
        {
            throw new RegraDeNegocioException("Data de aplicação não pode estar no futuro.");
        }

        if (dataAplicacao < Inicio || dataAplicacao > Fim)
        {
            throw new RegraDeNegocioException(
                $"Data de aplicação fora do período da campanha ({Inicio:dd/MM/yyyy} a {Fim:dd/MM/yyyy}).");
        }

        if (quantidadeAnimais <= 0)
        {
            throw new RegraDeNegocioException("Quantidade de animais vacinados deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(loteVacina) || string.IsNullOrWhiteSpace(crmvVeterinario))
        {
            throw new RegraDeNegocioException("Lote da vacina e CRMV do veterinário são obrigatórios.");
        }

        return RegistroVacinacao.Criar(this, propriedade, quantidadeAnimais, dataAplicacao, loteVacina, crmvVeterinario);
    }
}
