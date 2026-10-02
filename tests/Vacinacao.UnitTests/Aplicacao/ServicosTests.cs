using Microsoft.Extensions.Time.Testing;
using Vacinacao.Application;
using Vacinacao.Domain;

namespace Vacinacao.UnitTests.Aplicacao;

public class ServicosTests
{
    // 10/03/2026 às 02h UTC ainda é 09/03 em Brasília: o serviço deve usar a data local.
    private readonly FakeTimeProvider _relogio = new(new DateTimeOffset(2026, 3, 10, 2, 0, 0, TimeSpan.Zero));
    private readonly RepositoriosEmMemoria _repos = new();

    [Fact]
    public void Hoje_usa_o_fuso_de_Brasilia()
    {
        Assert.Equal(new DateOnly(2026, 3, 9), _relogio.Hoje());
    }

    [Fact]
    public async Task Registra_vacinacao_e_persiste()
    {
        var campanha = await CriarCampanhaAsync();
        var servico = new VacinacaoService(_repos, _repos, _repos, _relogio);

        var registro = await servico.RegistrarAsync(
            campanha.Id, new RegistrarVacinacaoRequest("go000001", 50, new DateOnly(2026, 3, 1), "L-1", "GO-1234"), default);

        Assert.Equal("GO000001", registro.CodigoPropriedade);
        Assert.Single(_repos.Registros);
        Assert.Equal(2, _repos.Salvamentos);
    }

    [Fact]
    public async Task Campanha_inexistente_lanca_nao_encontrado()
    {
        var servico = new VacinacaoService(_repos, _repos, _repos, _relogio);

        await Assert.ThrowsAsync<NaoEncontradoException>(() => servico.RegistrarAsync(
            Guid.NewGuid(), new RegistrarVacinacaoRequest("GO000001", 1, new DateOnly(2026, 3, 1), "L-1", "GO-1234"), default));
    }

    [Fact]
    public async Task Aptidao_considera_as_vacinacoes_da_propriedade()
    {
        var campanha = await CriarCampanhaAsync();
        var vacinacao = new VacinacaoService(_repos, _repos, _repos, _relogio);
        var aptidao = new AptidaoService(_repos, _repos, _relogio);

        Assert.True((await aptidao.AvaliarAsync("GO000001", Especie.Bovino, default)).Avisos.Count == 1);

        await vacinacao.RegistrarAsync(
            campanha.Id, new RegistrarVacinacaoRequest("GO000001", 50, new DateOnly(2026, 3, 1), "L-1", "GO-1234"), default);
        var depois = await aptidao.AvaliarAsync("GO000001", Especie.Bovino, default);

        Assert.True(depois.Apta);
        Assert.Empty(depois.Avisos);
        Assert.Single(depois.Coberturas);
    }

    private Task<CampanhaResponse> CriarCampanhaAsync() =>
        new CampanhaService(_repos, _repos).CriarAsync(
            new CriarCampanhaRequest("Raiva 2026", Doenca.Raiva, Especie.Bovino,
                new DateOnly(2026, 2, 1), new DateOnly(2026, 4, 30), 12, true),
            default);

    private sealed class RepositoriosEmMemoria : ICampanhaRepository, IRegistroVacinacaoRepository, IUnidadeDeTrabalho
    {
        public List<Campanha> Campanhas { get; } = [];
        public List<RegistroVacinacao> Registros { get; } = [];
        public int Salvamentos { get; private set; }

        public void Adicionar(Campanha campanha) => Campanhas.Add(campanha);

        public void Adicionar(RegistroVacinacao registro) => Registros.Add(registro);

        public Task<Campanha?> ObterAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Campanhas.FirstOrDefault(c => c.Id == id));

        public Task<IReadOnlyList<Campanha>> ListarAsync(Especie? especie, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Campanha>>([.. Campanhas.Where(c => especie is null || c.Especie == especie)]);

        public Task<IReadOnlyList<Campanha>> ListarObrigatoriasAsync(Especie especie, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Campanha>>([.. Campanhas.Where(c => c.Obrigatoria && c.Especie == especie)]);

        public Task<IReadOnlyList<RegistroVacinacao>> ListarPorPropriedadeAsync(
            CodigoPropriedade propriedade, Especie? especie, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<RegistroVacinacao>>(
                [.. Registros.Where(r => r.Propriedade == propriedade && (especie is null || r.Especie == especie))]);

        public Task SalvarAsync(CancellationToken ct)
        {
            Salvamentos++;
            return Task.CompletedTask;
        }
    }
}
