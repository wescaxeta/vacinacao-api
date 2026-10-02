using Vacinacao.Domain;

namespace Vacinacao.UnitTests.Dominio;

public class AvaliadorAptidaoTests
{
    private static readonly DateOnly Hoje = new(2026, 3, 10);
    private static readonly CodigoPropriedade Propriedade = CodigoPropriedade.De("GO000001");

    // Prazo já encerrado (1º/09 a 30/11/2025)
    private static readonly Campanha BruceloseEncerrada = Campanha.Criar(
        "Brucelose 2025", Doenca.Brucelose, Especie.Bovino,
        new DateOnly(2025, 9, 1), new DateOnly(2025, 11, 30), validadeMeses: 12, obrigatoria: true);

    // Em andamento (1º/03 a 30/04/2026)
    private static readonly Campanha RaivaEmAndamento = Campanha.Criar(
        "Raiva 2026", Doenca.Raiva, Especie.Bovino,
        new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 30), validadeMeses: 12, obrigatoria: true);

    private static Aptidao Avaliar(params RegistroVacinacao[] registros) => AvaliadorAptidao.Avaliar(
        Propriedade, Especie.Bovino, [BruceloseEncerrada, RaivaEmAndamento], registros, Hoje);

    [Fact]
    public void Sem_vacinacao_e_com_prazo_encerrado_bloqueia_o_transporte()
    {
        var aptidao = Avaliar();

        Assert.False(aptidao.Apta);
        var pendencia = Assert.Single(aptidao.Pendencias);
        Assert.Contains("Brucelose", pendencia);
        Assert.Contains("30/11/2025", pendencia);
    }

    [Fact]
    public void Campanha_em_andamento_gera_apenas_aviso()
    {
        var aptidao = Avaliar(BruceloseEncerrada.RegistrarVacinacao(Propriedade, 100, new DateOnly(2025, 10, 1), "L-1", "GO-1234", Hoje));

        Assert.True(aptidao.Apta);
        Assert.Contains("Raiva", Assert.Single(aptidao.Avisos));
        Assert.Equal(new CoberturaVacinal(Doenca.Brucelose, new DateOnly(2026, 10, 1)), Assert.Single(aptidao.Coberturas));
    }

    [Fact]
    public void Com_todas_as_vacinas_em_dia_esta_apta_sem_avisos()
    {
        var aptidao = Avaliar(
            BruceloseEncerrada.RegistrarVacinacao(Propriedade, 100, new DateOnly(2025, 10, 1), "L-1", "GO-1234", Hoje),
            RaivaEmAndamento.RegistrarVacinacao(Propriedade, 100, new DateOnly(2026, 3, 5), "L-2", "GO-1234", Hoje));

        Assert.True(aptidao.Apta);
        Assert.Empty(aptidao.Avisos);
        Assert.Equal(2, aptidao.Coberturas.Count);
    }

    [Fact]
    public void Vacinacao_vencida_volta_a_bloquear()
    {
        var registro = BruceloseEncerrada.RegistrarVacinacao(Propriedade, 100, new DateOnly(2025, 10, 1), "L-1", "GO-1234", Hoje);

        var aptidao = AvaliadorAptidao.Avaliar(
            Propriedade, Especie.Bovino, [BruceloseEncerrada], [registro], hoje: new DateOnly(2026, 10, 2));

        Assert.False(aptidao.Apta);
    }

    [Fact]
    public void Ignora_vacinacao_de_outra_propriedade_ou_especie()
    {
        var outraPropriedade = BruceloseEncerrada.RegistrarVacinacao(
            CodigoPropriedade.De("GO000002"), 100, new DateOnly(2025, 10, 1), "L-1", "GO-1234", Hoje);

        Assert.False(Avaliar(outraPropriedade).Apta);
    }

    [Fact]
    public void Campanhas_opcionais_ou_futuras_nao_entram_na_avaliacao()
    {
        var opcional = Campanha.Criar("Clostridiose", Doenca.Clostridiose, Especie.Bovino,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1), 12, obrigatoria: false);
        var futura = Campanha.Criar("Aftosa", Doenca.Aftosa, Especie.Bovino,
            new DateOnly(2026, 5, 1), new DateOnly(2026, 6, 1), 6, obrigatoria: true);

        var aptidao = AvaliadorAptidao.Avaliar(Propriedade, Especie.Bovino, [opcional, futura], [], Hoje);

        Assert.True(aptidao.Apta);
        Assert.Empty(aptidao.Avisos);
    }
}
