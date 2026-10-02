using Vacinacao.Domain;

namespace Vacinacao.Application;

public sealed class CampanhaService(
    ICampanhaRepository campanhas,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<CampanhaResponse> CriarAsync(CriarCampanhaRequest request, CancellationToken ct)
    {
        var campanha = Campanha.Criar(
            request.Nome, request.Doenca, request.Especie, request.Inicio, request.Fim,
            request.ValidadeMeses, request.Obrigatoria);

        campanhas.Adicionar(campanha);
        await unidadeDeTrabalho.SalvarAsync(ct);

        return CampanhaResponse.De(campanha);
    }

    public async Task<CampanhaResponse> ObterAsync(Guid id, CancellationToken ct) =>
        CampanhaResponse.De(await campanhas.ObterAsync(id, ct)
            ?? throw new NaoEncontradoException($"Campanha {id} não encontrada."));

    public async Task<IReadOnlyList<CampanhaResponse>> ListarAsync(Especie? especie, CancellationToken ct) =>
        [.. (await campanhas.ListarAsync(especie, ct)).Select(CampanhaResponse.De)];
}

public sealed class VacinacaoService(
    ICampanhaRepository campanhas,
    IRegistroVacinacaoRepository registros,
    IUnidadeDeTrabalho unidadeDeTrabalho,
    TimeProvider relogio)
{
    public async Task<RegistroVacinacaoResponse> RegistrarAsync(
        Guid campanhaId, RegistrarVacinacaoRequest request, CancellationToken ct)
    {
        var campanha = await campanhas.ObterAsync(campanhaId, ct)
            ?? throw new NaoEncontradoException($"Campanha {campanhaId} não encontrada.");

        var registro = campanha.RegistrarVacinacao(
            CodigoPropriedade.De(request.CodigoPropriedade),
            request.QuantidadeAnimais,
            request.DataAplicacao,
            request.LoteVacina,
            request.CrmvVeterinario,
            relogio.Hoje());

        registros.Adicionar(registro);
        await unidadeDeTrabalho.SalvarAsync(ct);

        return RegistroVacinacaoResponse.De(registro);
    }

    public async Task<IReadOnlyList<RegistroVacinacaoResponse>> ListarAsync(
        string codigoPropriedade, Especie? especie, CancellationToken ct) =>
        [.. (await registros.ListarPorPropriedadeAsync(CodigoPropriedade.De(codigoPropriedade), especie, ct))
            .Select(RegistroVacinacaoResponse.De)];
}

public sealed class AptidaoService(
    ICampanhaRepository campanhas,
    IRegistroVacinacaoRepository registros,
    TimeProvider relogio)
{
    public async Task<AptidaoResponse> AvaliarAsync(string codigoPropriedade, Especie especie, CancellationToken ct)
    {
        var propriedade = CodigoPropriedade.De(codigoPropriedade);

        var aptidao = AvaliadorAptidao.Avaliar(
            propriedade,
            especie,
            await campanhas.ListarObrigatoriasAsync(especie, ct),
            await registros.ListarPorPropriedadeAsync(propriedade, especie, ct),
            relogio.Hoje());

        return AptidaoResponse.De(aptidao);
    }
}
