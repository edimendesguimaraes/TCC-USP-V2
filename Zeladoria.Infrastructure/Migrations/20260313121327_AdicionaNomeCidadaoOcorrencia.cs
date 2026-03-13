using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zeladoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaNomeCidadaoOcorrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeCidadao",
                table: "Ocorrencias",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeCidadao",
                table: "Ocorrencias");
        }
    }
}
