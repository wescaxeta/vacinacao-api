using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vacinacao.IntegrationTests;

[Collection(ColecaoApi.Nome)]
public class ApiTests(ApiFactory factory)
{
    private readonly HttpClient _http = factory.CreateClient();

    // Cada teste usa um código de propriedade próprio, para não depender da ordem de execução.
    private static string NovaPropriedade() => $"GO{Random.Shared.Next(100_000, 999_999)}";

    [Fact]
    public async Task Health_check_responde_com_o_banco_no_ar()
    {
        var resposta = await _http.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Healthy", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Fluxo_completo_campanha_vacinacao_e_aptidao()
    {
        var propriedade = NovaPropriedade();
        var encerrada = await CriarCampanhaAsync("Brucelose 2025", "brucelose", "2025-09-01", "2025-11-30");

        var antes = await AptidaoAsync(propriedade);
        Assert.False(antes["apta"]!.GetValue<bool>());

        var registro = await _http.PostAsJsonAsync($"/campanhas/{encerrada}/vacinacoes", new
        {
            codigoPropriedade = propriedade.ToLowerInvariant(),
            quantidadeAnimais = 120,
            dataAplicacao = "2025-10-15",
            loteVacina = "BR-77",
            crmvVeterinario = "GO-12345",
        });
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

        var depois = await AptidaoAsync(propriedade);
        Assert.True(depois["apta"]!.GetValue<bool>());
        Assert.Equal("2026-10-15", depois["coberturas"]![0]!["validaAte"]!.GetValue<string>());

        var historico = await _http.GetFromJsonAsync<JsonArray>($"/propriedades/{propriedade}/vacinacoes");
        Assert.Equal(propriedade, Assert.Single(historico!)!["codigoPropriedade"]!.GetValue<string>());
    }

    [Fact]
    public async Task Vacinacao_fora_do_periodo_da_campanha_retorna_422()
    {
        var campanha = await CriarCampanhaAsync("Raiva 2026", "raiva", "2026-03-01", "2026-04-30");

        var resposta = await _http.PostAsJsonAsync($"/campanhas/{campanha}/vacinacoes", new
        {
            codigoPropriedade = NovaPropriedade(),
            quantidadeAnimais = 10,
            dataAplicacao = "2026-02-15",
            loteVacina = "RV-1",
            crmvVeterinario = "GO-12345",
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("fora do período", (await LerJsonAsync(resposta))["detail"]!.GetValue<string>());
    }

    [Fact]
    public async Task Campos_invalidos_retornam_400_com_erros_por_campo()
    {
        var campanha = await CriarCampanhaAsync("Raiva 2026", "raiva", "2026-03-01", "2026-04-30");

        var resposta = await _http.PostAsJsonAsync($"/campanhas/{campanha}/vacinacoes", new
        {
            codigoPropriedade = "GO1",
            quantidadeAnimais = 0,
            dataAplicacao = "2026-03-05",
            loteVacina = "",
            crmvVeterinario = "x",
        });
        var erros = (await LerJsonAsync(resposta))["errors"]!.AsObject();

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(
            ["codigoPropriedade", "crmvVeterinario", "loteVacina", "quantidadeAnimais"],
            erros.Select(e => e.Key).Order());
    }

    [Fact]
    public async Task Campanha_inexistente_retorna_404()
    {
        var resposta = await _http.PostAsJsonAsync($"/campanhas/{Guid.CreateVersion7()}/vacinacoes", new
        {
            codigoPropriedade = NovaPropriedade(),
            quantidadeAnimais = 10,
            dataAplicacao = "2026-03-05",
            loteVacina = "RV-1",
            crmvVeterinario = "GO-12345",
        });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Theory]
    [InlineData("/propriedades/GO000001/aptidao", "especie")]
    [InlineData("/propriedades/GO000001/aptidao?especie=gato", "especie")]
    [InlineData("/propriedades/GO1/aptidao?especie=bovino", "codigo")]
    public async Task Parametros_invalidos_retornam_400(string url, string campo)
    {
        var resposta = await _http.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.True((await LerJsonAsync(resposta))["errors"]!.AsObject().ContainsKey(campo));
    }

    [Fact]
    public async Task Documentacao_openapi_esta_publicada()
    {
        var documento = await _http.GetFromJsonAsync<JsonObject>("/openapi/v1.json");

        Assert.Equal("Vacinação de Rebanhos API", documento!["info"]!["title"]!.GetValue<string>());
        Assert.True(documento["paths"]!.AsObject().ContainsKey("/propriedades/{codigo}/aptidao"));
    }

    private async Task<Guid> CriarCampanhaAsync(string nome, string doenca, string inicio, string fim)
    {
        var resposta = await _http.PostAsJsonAsync("/campanhas", new
        {
            nome,
            doenca,
            especie = "bovino",
            inicio,
            fim,
            validadeMeses = 12,
            obrigatoria = true,
        });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await LerJsonAsync(resposta))["id"]!.GetValue<Guid>();
    }

    private async Task<JsonObject> AptidaoAsync(string propriedade) =>
        (await _http.GetFromJsonAsync<JsonObject>($"/propriedades/{propriedade}/aptidao?especie=bovino"))!;

    private static async Task<JsonObject> LerJsonAsync(HttpResponseMessage resposta) =>
        JsonNode.Parse(await resposta.Content.ReadAsStringAsync(), documentOptions: new JsonDocumentOptions())!.AsObject();
}
