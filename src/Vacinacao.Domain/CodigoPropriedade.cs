using System.Text.RegularExpressions;

namespace Vacinacao.Domain;

/// <summary>
/// Código estadual da propriedade rural: sigla da UF + 6 dígitos (ex.: GO000123).
/// Mesmo formato usado pelo serviço de GTA, para que os dois sistemas conversem.
/// </summary>
public sealed partial record CodigoPropriedade
{
    public string Valor { get; }

    private CodigoPropriedade(string valor) => Valor = valor;

    public static CodigoPropriedade De(string valor)
    {
        var normalizado = (valor ?? string.Empty).Trim().ToUpperInvariant();

        if (!Formato().IsMatch(normalizado))
        {
            throw new RegraDeNegocioException(
                $"Código de propriedade inválido: \"{valor}\". Use a UF seguida de 6 dígitos (ex.: GO000123).");
        }

        return new CodigoPropriedade(normalizado);
    }

    public static bool EhValido(string? valor) =>
        valor is not null && Formato().IsMatch(valor.Trim().ToUpperInvariant());

    public string Uf => Valor[..2];

    public override string ToString() => Valor;

    [GeneratedRegex("^[A-Z]{2}[0-9]{6}$")]
    private static partial Regex Formato();
}
