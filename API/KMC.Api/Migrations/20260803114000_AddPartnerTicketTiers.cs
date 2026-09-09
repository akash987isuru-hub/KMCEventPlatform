using KMC.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMC.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260803114000_AddPartnerTicketTiers")]
public partial class AddPartnerTicketTiers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PartnerEventTicketTiers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PartnerEventId = table.Column<int>(type: "int", nullable: false),
                ExternalTicketTierId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Description = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                SoldCount = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PartnerEventTicketTiers", x => x.Id);
                table.ForeignKey(
                    name: "FK_PartnerEventTicketTiers_PartnerEvents_PartnerEventId",
                    column: x => x.PartnerEventId,
                    principalTable: "PartnerEvents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PartnerEventTicketTiers_PartnerEventId_ExternalTicketTierId",
            table: "PartnerEventTicketTiers",
            columns: new[] { "PartnerEventId", "ExternalTicketTierId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PartnerEventTicketTiers");
    }
}
