using System;
using KMC.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMC.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260803150000_AddEventDateRangesAndTicketQuantity")]
public partial class AddEventDateRangesAndTicketQuantity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "EndDate",
            table: "Events",
            type: "datetime2",
            nullable: true);

        migrationBuilder.Sql("UPDATE [Events] SET [EndDate] = [EventDate] WHERE [EndDate] IS NULL");

        migrationBuilder.AlterColumn<DateTime>(
            name: "EndDate",
            table: "Events",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "EndDate",
            table: "PartnerEvents",
            type: "datetime2",
            nullable: true);

        migrationBuilder.Sql("UPDATE [PartnerEvents] SET [EndDate] = [EventDate] WHERE [EndDate] IS NULL");

        migrationBuilder.AlterColumn<DateTime>(
            name: "EndDate",
            table: "PartnerEvents",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Quantity",
            table: "Registrations",
            type: "int",
            nullable: false,
            defaultValue: 1);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "EndDate", table: "Events");
        migrationBuilder.DropColumn(name: "EndDate", table: "PartnerEvents");
        migrationBuilder.DropColumn(name: "Quantity", table: "Registrations");
    }
}
