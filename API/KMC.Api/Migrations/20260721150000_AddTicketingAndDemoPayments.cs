using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMC.Api.Migrations;

public partial class AddTicketingAndDemoPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "TicketTierId",
            table: "Registrations",
            type: "int",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TicketTiers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EventId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(
                    type: "nvarchar(60)",
                    maxLength: 60,
                    nullable: false),
                Price = table.Column<decimal>(
                    type: "decimal(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false),
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

        migrationBuilder.Sql(
            """
            INSERT INTO TicketTiers (EventId, Name, Price, Capacity, SortOrder)
            SELECT Id, 'Standard', 1000.00, Capacity, 1
            FROM Events;

            INSERT INTO TicketTiers (EventId, Name, Price, Capacity, SortOrder)
            SELECT Id, 'Premium', 2500.00,
                   CASE WHEN Capacity / 3 < 1 THEN 1 ELSE Capacity / 3 END, 2
            FROM Events;

            INSERT INTO TicketTiers (EventId, Name, Price, Capacity, SortOrder)
            SELECT Id, 'VIP', 5000.00,
                   CASE WHEN Capacity / 5 < 1 THEN 1 ELSE Capacity / 5 END, 3
            FROM Events;

            UPDATE Events
            SET Capacity =
                Capacity +
                CASE WHEN Capacity / 3 < 1 THEN 1 ELSE Capacity / 3 END +
                CASE WHEN Capacity / 5 < 1 THEN 1 ELSE Capacity / 5 END;

            UPDATE registrations
            SET TicketTierId = ticket.Id
            FROM Registrations AS registrations
            INNER JOIN TicketTiers AS ticket
                ON ticket.EventId = registrations.EventId
               AND ticket.SortOrder = 1;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "TicketTierId",
            table: "Registrations",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Registrations_TicketTierId",
            table: "Registrations",
            column: "TicketTierId");

        migrationBuilder.CreateIndex(
            name: "IX_TicketTiers_EventId_Name",
            table: "TicketTiers",
            columns: new[] { "EventId", "Name" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Registrations_TicketTiers_TicketTierId",
            table: "Registrations",
            column: "TicketTierId",
            principalTable: "TicketTiers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RegistrationId = table.Column<int>(type: "int", nullable: false),
                Amount = table.Column<decimal>(
                    type: "decimal(18,2)",
                    precision: 18,
                    scale: 2,
                    nullable: false),
                Currency = table.Column<string>(
                    type: "nvarchar(3)",
                    maxLength: 3,
                    nullable: false),
                CardHolderName = table.Column<string>(
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: false),
                CardBrand = table.Column<string>(
                    type: "nvarchar(30)",
                    maxLength: 30,
                    nullable: false),
                CardLastFour = table.Column<string>(
                    type: "nvarchar(4)",
                    maxLength: 4,
                    nullable: false),
                TransactionReference = table.Column<string>(
                    type: "nvarchar(60)",
                    maxLength: 60,
                    nullable: false),
                Status = table.Column<string>(
                    type: "nvarchar(max)",
                    nullable: false),
                PaidAt = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: false),
                RefundedAt = table.Column<DateTime>(
                    type: "datetime2",
                    nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payments_Registrations_RegistrationId",
                    column: x => x.RegistrationId,
                    principalTable: "Registrations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Payments_RegistrationId",
            table: "Payments",
            column: "RegistrationId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Payments_TransactionReference",
            table: "Payments",
            column: "TransactionReference",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Payments");

        migrationBuilder.DropForeignKey(
            name: "FK_Registrations_TicketTiers_TicketTierId",
            table: "Registrations");

        migrationBuilder.DropTable(
            name: "TicketTiers");

        migrationBuilder.DropIndex(
            name: "IX_Registrations_TicketTierId",
            table: "Registrations");

        migrationBuilder.DropColumn(
            name: "TicketTierId",
            table: "Registrations");
    }
}
