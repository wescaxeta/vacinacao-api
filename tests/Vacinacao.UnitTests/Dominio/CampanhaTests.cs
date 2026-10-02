using Vacinacao.Domain;

namespace Vacinacao.UnitTests.Dominio;

public class CampanhaTests
{
    private static readonly DateOnly Hoje = new(2026, 3, 10);
    private static readonly CodigoPropriedade Propriedade = CodigoPropriedade.De("GO000001");

    private static Campanha CampanhaDeBrucelose(int validadeMeses = 12) => Campanha.Criar(
        "Brucelose 2026", Doenca.Brucelose, Especie.Bovino,
        new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), validadeMeses, obrigatoria: true);

    [Fact]
    public void Cria_campanha_valida()
    {
        var campanha = CampanhaDeBrucelose();

        Assert.NotEqual(Guid.Empty, campanha.Id);
        Assert.Equal(Doenca.Brucelose, campanha.Doenca);
        Assert.True(campanha.EmAndamento(Hoje));
        Assert.False(campanha.PrazoEncerrado(Hoje));
    }

    [Theory]
    [InlineData("", 12, "Nome")]
    [InlineData("Brucelose", 0, "Validade")]
    [InlineData("Brucelose", 61, "Validade")]
    public void Rejeita_campanha_invalida(string nome, int validade, string mensagem)
    {
        var erro = Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            nome, Doenca.Brucelose, Especie.Bovino, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), validade, true));

        Assert.Contains(mensagem, erro.Message);
    }

    [Fact]
    public void Rejeita_fim_antes_do_inicio()
    {
        Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            "Brucelose", Doenca.Brucelose, Especie.Bovino, new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 1), 12, true));
    }

    [Fact]
    public void Registro_herda_doenca_e_especie_e_calcula_validade()
    {
        var registro = CampanhaDeBrucelose(validadeMeses: 12)
            .RegistrarVacinacao(Propriedade, 150, new DateOnly(2026, 2, 15), " L-01 ", "go-1234", Hoje);

        Assert.Equal(Doenca.Brucelose, registro.Doenca);
        Assert.Equal(Especie.Bovino, registro.Especie);
        Assert.Equal(new DateOnly(2027, 2, 15), registro.ValidaAte);
        Assert.Equal("L-01", registro.LoteVacina);
        Assert.Equal("GO-1234", registro.CrmvVeterinario);
    }

    [Theory]
    [InlineData(2026, 3, 11, "futuro")]
    [InlineData(2025, 12, 31, "fora do período")]
    public void Rejeita_data_de_aplicacao_invalida(int ano, int mes, int dia, string mensagem)
    {
        var erro = Assert.Throws<RegraDeNegocioException>(() => CampanhaDeBrucelose()
            .RegistrarVacinacao(Propriedade, 10, new DateOnly(ano, mes, dia), "L-01", "GO-1234", Hoje));

        Assert.Contains(mensagem, erro.Message);
    }

    [Fact]
    public void Rejeita_quantidade_zero()
    {
        Assert.Throws<RegraDeNegocioException>(() => CampanhaDeBrucelose()
            .RegistrarVacinacao(Propriedade, 0, new DateOnly(2026, 2, 1), "L-01", "GO-1234", Hoje));
    }

    [Theory]
    [InlineData("GO000001", true)]
    [InlineData(" go000001 ", true)]
    [InlineData("GO1", false)]
    [InlineData("000001GO", false)]
    [InlineData("", false)]
    public void Valida_formato_do_codigo_de_propriedade(string codigo, bool valido)
    {
        Assert.Equal(valido, CodigoPropriedade.EhValido(codigo));
    }

    [Fact]
    public void Normaliza_codigo_de_propriedade()
    {
        var codigo = CodigoPropriedade.De(" mt000010 ");

        Assert.Equal("MT000010", codigo.Valor);
        Assert.Equal("MT", codigo.Uf);
        Assert.Equal(CodigoPropriedade.De("MT000010"), codigo);
    }
}
