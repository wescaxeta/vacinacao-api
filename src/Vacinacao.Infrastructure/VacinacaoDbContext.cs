using Microsoft.EntityFrameworkCore;
using Vacinacao.Application;
using Vacinacao.Domain;

namespace Vacinacao.Infrastructure;

public sealed class VacinacaoDbContext(DbContextOptions<VacinacaoDbContext> options)
    : DbContext(options), IUnidadeDeTrabalho
{
    public DbSet<Campanha> Campanhas => Set<Campanha>();
    public DbSet<RegistroVacinacao> RegistrosVacinacao => Set<RegistroVacinacao>();

    public Task SalvarAsync(CancellationToken ct) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campanha>(campanha =>
        {
            campanha.ToTable("campanha");
            campanha.HasKey(c => c.Id);
            campanha.Property(c => c.Nome).HasMaxLength(120).IsRequired();
            campanha.Property(c => c.Doenca).HasConversion<string>().HasMaxLength(20);
            campanha.Property(c => c.Especie).HasConversion<string>().HasMaxLength(20);
            campanha.HasIndex(c => new { c.Especie, c.Obrigatoria });
            campanha.ToTable(t =>
            {
                t.HasCheckConstraint("ck_campanha_periodo", "fim >= inicio");
                t.HasCheckConstraint("ck_campanha_validade", "validade_meses BETWEEN 1 AND 60");
            });
        });

        modelBuilder.Entity<RegistroVacinacao>(registro =>
        {
            registro.ToTable("registro_vacinacao");
            registro.HasKey(r => r.Id);
            registro.Property(r => r.Propriedade)
                .HasColumnName("codigo_propriedade")
                .HasMaxLength(8)
                .HasConversion(codigo => codigo.Valor, valor => CodigoPropriedade.De(valor));
            registro.Property(r => r.Doenca).HasConversion<string>().HasMaxLength(20);
            registro.Property(r => r.Especie).HasConversion<string>().HasMaxLength(20);
            registro.Property(r => r.LoteVacina).HasMaxLength(40).IsRequired();
            registro.Property(r => r.CrmvVeterinario).HasMaxLength(12).IsRequired();
            registro.HasOne<Campanha>().WithMany().HasForeignKey(r => r.CampanhaId).OnDelete(DeleteBehavior.Restrict);

            // Consulta principal: vacinações de uma propriedade por espécie
            registro.HasIndex(r => new { r.Propriedade, r.Especie });
            registro.ToTable(t => t.HasCheckConstraint("ck_registro_quantidade", "quantidade_animais > 0"));
        });
    }
}
