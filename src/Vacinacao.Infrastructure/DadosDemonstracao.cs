using Microsoft.EntityFrameworkCore;
using Vacinacao.Application;
using Vacinacao.Domain;

namespace Vacinacao.Infrastructure;

/// <summary>
/// Dados fictícios para demonstração, com datas relativas a hoje para que o cenário continue
/// válido em qualquer época. Usa os mesmos códigos de propriedade do mock SOAP do gta-integration-service.
/// </summary>
public static class DadosDemonstracao
{
    public static async Task PopularAsync(VacinacaoDbContext db, TimeProvider relogio, CancellationToken ct = default)
    {
        if (await db.Campanhas.AnyAsync(ct))
        {
            return;
        }

        var hoje = relogio.Hoje();

        var brucelose = Campanha.Criar(
            "Brucelose - etapa anterior", Doenca.Brucelose, Especie.Bovino,
            hoje.AddDays(-120), hoje.AddDays(-30), validadeMeses: 12, obrigatoria: true);
        var raiva = Campanha.Criar(
            "Raiva dos herbívoros - etapa atual", Doenca.Raiva, Especie.Bovino,
            hoje.AddDays(-15), hoje.AddDays(45), validadeMeses: 12, obrigatoria: true);

        db.Campanhas.AddRange(brucelose, raiva);

        // GO000001: vacinou as duas, apta. GO000003: não vacinou contra brucelose, bloqueada.
        // MT000010: vacinou contra brucelose e ainda está no prazo da raiva, apta com aviso.
        db.RegistrosVacinacao.AddRange(
            brucelose.RegistrarVacinacao(CodigoPropriedade.De("GO000001"), 480, hoje.AddDays(-60), "BR-2026-001", "GO-10234", hoje),
            raiva.RegistrarVacinacao(CodigoPropriedade.De("GO000001"), 480, hoje.AddDays(-5), "RV-2026-014", "GO-10234", hoje),
            brucelose.RegistrarVacinacao(CodigoPropriedade.De("MT000010"), 1150, hoje.AddDays(-45), "BR-2026-087", "MT-5521", hoje));

        await db.SaveChangesAsync(ct);
    }
}
