using FluentValidation;
using Vacinacao.Domain;

namespace Vacinacao.Application;

/// <summary>
/// Validação de formato da entrada (campos obrigatórios, tamanhos, enums). Regras que dependem
/// de estado, como a data dentro do período da campanha, ficam no domínio.
/// </summary>
public sealed class CriarCampanhaValidator : AbstractValidator<CriarCampanhaRequest>
{
    public CriarCampanhaValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Informe o nome da campanha.")
            .MaximumLength(120).WithMessage("O nome deve ter no máximo 120 caracteres.");
        RuleFor(x => x.Doenca).IsInEnum().WithMessage("Doença inválida.");
        RuleFor(x => x.Especie).IsInEnum().WithMessage("Espécie inválida.");
        RuleFor(x => x.Fim).GreaterThanOrEqualTo(x => x.Inicio)
            .WithMessage("O fim da campanha não pode ser anterior ao início.");
        RuleFor(x => x.ValidadeMeses).InclusiveBetween(1, Campanha.ValidadeMaximaMeses)
            .WithMessage($"A validade deve ficar entre 1 e {Campanha.ValidadeMaximaMeses} meses.");
    }
}

public sealed class RegistrarVacinacaoValidator : AbstractValidator<RegistrarVacinacaoRequest>
{
    public RegistrarVacinacaoValidator()
    {
        RuleFor(x => x.CodigoPropriedade).Must(CodigoPropriedade.EhValido)
            .WithMessage("Use a UF seguida de 6 dígitos (ex.: GO000123).");
        RuleFor(x => x.QuantidadeAnimais).InclusiveBetween(1, 100_000)
            .WithMessage("Informe entre 1 e 100.000 animais.");
        RuleFor(x => x.LoteVacina).NotEmpty().WithMessage("Informe o lote da vacina.")
            .MaximumLength(40).WithMessage("O lote deve ter no máximo 40 caracteres.");
        RuleFor(x => x.CrmvVeterinario).Matches("^[A-Za-z]{2}-?[0-9]{3,6}$")
            .WithMessage("Informe o CRMV no formato UF-número (ex.: GO-12345).");
    }
}
