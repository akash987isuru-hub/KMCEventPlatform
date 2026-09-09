using KMC.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMC.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260803100000_AddPartnerEvents")]
public partial class AddPartnerEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PartnerEvents",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ExternalEventCode = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false),
                SourceEventId = table.Column<int>(
                    type: "int",
                    nullable: false),
                SourceSystem = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false),
                Title = table.Column<string>(
                    type: "nvarchar(150)",
                    maxLength: 150,
                    nullable: false),
                Description = table.Column<string>(
                    type: "nvarchar(1000)",
                    maxLength: 1000,
                    nullable: true),
                EventType = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false),
                EventDate = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: false),
                StartTime = table.Column<TimeSpan>(
                    type: "time",
                    nullable: false),
                EndTime = table.Column<TimeSpan>(
                    type: "time",
                    nullable: false),
                Venue = table.Column<string>(
                    type: "nvarchar(150)",
                    maxLength: 150,
                    nullable: false),
                Location = table.Column<string>(
                    type: "nvarchar(200)",
                    maxLength: 200,
                    nullable: false),
                OrganizerName = table.Column<string>(
                    type: "nvarchar(150)",
                    maxLength: 150,
                    nullable: false),
                OrganizerEventUrl = table.Column<string>(
                    type: "nvarchar(500)",
                    maxLength: 500,
                    nullable: false),
                IsPublished = table.Column<bool>(
                    type: "bit",
                    nullable: false),
                CreatedAt = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: false),
                UpdatedAt = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: true),
                LastSyncedAt = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PartnerEvents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PartnerEvents_ExternalEventCode",
            table: "PartnerEvents",
            column: "ExternalEventCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PartnerEvents");
    }
}
