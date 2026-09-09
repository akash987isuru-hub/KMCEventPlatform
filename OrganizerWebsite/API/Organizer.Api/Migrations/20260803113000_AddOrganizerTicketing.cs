using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Organizer.Api.Data;

#nullable disable

namespace Organizer.Api.Migrations;

[DbContext(typeof(OrganizerDbContext))]
[Migration("20260803113000_AddOrganizerTicketing")]
public partial class AddOrganizerTicketing : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TicketTiers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EventId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Description = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TicketTiers", x => x.Id);
                table.ForeignKey(
                    name: "FK_TicketTiers_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Bookings",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BookingReference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                EventId = table.Column<int>(type: "int", nullable: false),
                TicketTierId = table.Column<int>(type: "int", nullable: false),
                CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CustomerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                BookingSource = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bookings", x => x.Id);
                table.ForeignKey(
                    name: "FK_Bookings_Events_EventId",
                    column: x => x.EventId,
                    principalTable: "Events",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Bookings_TicketTiers_TicketTierId",
                    column: x => x.TicketTierId,
                    principalTable: "TicketTiers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BookingId = table.Column<int>(type: "int", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                CardHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CardBrand = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CardLastFour = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                TransactionReference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payments_Bookings_BookingId",
                    column: x => x.BookingId,
                    principalTable: "Bookings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_TicketTiers_EventId_Name", table: "TicketTiers", columns: new[] { "EventId", "Name" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Bookings_BookingReference", table: "Bookings", column: "BookingReference", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Bookings_EventId", table: "Bookings", column: "EventId");
        migrationBuilder.CreateIndex(name: "IX_Bookings_TicketTierId", table: "Bookings", column: "TicketTierId");
        migrationBuilder.CreateIndex(name: "IX_Payments_BookingId", table: "Payments", column: "BookingId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Payments_TransactionReference", table: "Payments", column: "TransactionReference", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Payments");
        migrationBuilder.DropTable(name: "Bookings");
        migrationBuilder.DropTable(name: "TicketTiers");
    }
}
