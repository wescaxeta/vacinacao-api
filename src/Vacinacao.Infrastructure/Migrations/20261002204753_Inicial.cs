using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vacinacao.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campanha",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    doenca = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    especie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fim = table.Column<DateOnly>(type: "date", nullable: false),
                    validade_meses = table.Column<int>(type: "integer", nullable: false),
                    obrigatoria = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_campanha", x => x.id);
                    table.CheckConstraint("ck_campanha_periodo", "fim >= inicio");
                    table.CheckConstraint("ck_campanha_validade", "validade_meses BETWEEN 1 AND 60");
                });

            migrationBuilder.CreateTable(
                name: "registro_vacinacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_propriedade = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    doenca = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    especie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantidade_animais = table.Column<int>(type: "integer", nullable: false),
                    data_aplicacao = table.Column<DateOnly>(type: "date", nullable: false),
                    valida_ate = table.Column<DateOnly>(type: "date", nullable: false),
                    lote_vacina = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    crmv_veterinario = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registro_vacinacao", x => x.id);
                    table.CheckConstraint("ck_registro_quantidade", "quantidade_animais > 0");
                    table.ForeignKey(
                        name: "fk_registro_vacinacao_campanha_campanha_id",
                        column: x => x.campanha_id,
                        principalTable: "campanha",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_campanha_especie_obrigatoria",
                table: "campanha",
                columns: new[] { "especie", "obrigatoria" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_vacinacao_campanha_id",
                table: "registro_vacinacao",
                column: "campanha_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_vacinacao_codigo_propriedade_especie",
                table: "registro_vacinacao",
                columns: new[] { "codigo_propriedade", "especie" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registro_vacinacao");

            migrationBuilder.DropTable(
                name: "campanha");
        }
    }
}
