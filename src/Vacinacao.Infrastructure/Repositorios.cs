using Microsoft.EntityFrameworkCore;
using Vacinacao.Application;
using Vacinacao.Domain;

namespace Vacinacao.Infrastructure;

internal sealed class CampanhaRepository(VacinacaoDbContext db) : ICampanhaRepository
{
    public void Adicionar(Campanha campanha) => db.Campanhas.Add(campanha);

    public Task<Campanha?> ObterAsync(Guid id, CancellationToken ct) =>
        db.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Campanha>> ListarAsync(Especie? especie, CancellationToken ct) =>
        await db.Campanhas.AsNoTracking()
            .Where(c => especie == null || c.Especie == especie)
            .OrderByDescending(c => c.Inicio)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Campanha>> ListarObrigatoriasAsync(Especie especie, CancellationToken ct) =>
        await db.Campanhas.AsNoTracking()
            .Where(c => c.Obrigatoria && c.Especie == especie)
            .ToListAsync(ct);
}

internal sealed class RegistroVacinacaoRepository(VacinacaoDbContext db) : IRegistroVacinacaoRepository
{
    public void Adicionar(RegistroVacinacao registro) => db.RegistrosVacinacao.Add(registro);

    public async Task<IReadOnlyList<RegistroVacinacao>> ListarPorPropriedadeAsync(
        CodigoPropriedade propriedade, Especie? especie, CancellationToken ct) =>
        await db.RegistrosVacinacao.AsNoTracking()
            .Where(r => r.Propriedade == propriedade && (especie == null || r.Especie == especie))
            .OrderByDescending(r => r.DataAplicacao)
            .ToListAsync(ct);
}
