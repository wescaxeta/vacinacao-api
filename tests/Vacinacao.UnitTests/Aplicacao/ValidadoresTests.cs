using Vacinacao.Application;
using Vacinacao.Domain;

namespace Vacinacao.UnitTests.Aplicacao;

public class ValidadoresTests
{
    private readonly RegistrarVacinacaoValidator _registro = new();
    private readonly CriarCampanhaValidator _campanha = new();

    [Fact]
    public void Aceita_registro_valido()
    {
        var resultado = _registro.Validate(new RegistrarVacinacaoRequest("GO000001", 10, new DateOnly(2026, 3, 1), "L-1", "GO-12345"));

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Aponta_todos_os_campos_invalidos_do_registro()
    {
        var resultado = _registro.Validate(new RegistrarVacinacaoRequest("GO1", 0, new DateOnly(2026, 3, 1), "", "123"));

        Assert.Equal(
            ["CodigoPropriedade", "CrmvVeterinario", "LoteVacina", "QuantidadeAnimais"],
            resultado.Errors.Select(e => e.PropertyName).Distinct().Order());
    }

    [Fact]
    public void Rejeita_campanha_com_fim_antes_do_inicio()
    {
        var resultado = _campanha.Validate(new CriarCampanhaRequest(
            "Raiva", Doenca.Raiva, Especie.Bovino, new DateOnly(2026, 5, 1), new DateOnly(2026, 4, 1), 12, true));

        Assert.Equal("Fim", Assert.Single(resultado.Errors).PropertyName);
    }

    [Fact]
    public void Rejeita_enum_fora_do_dominio()
    {
        var resultado = _campanha.Validate(new CriarCampanhaRequest(
            "Raiva", (Doenca)99, Especie.Bovino, new DateOnly(2026, 4, 1), new DateOnly(2026, 5, 1), 12, true));

        Assert.Equal("Doenca", Assert.Single(resultado.Errors).PropertyName);
    }
}
