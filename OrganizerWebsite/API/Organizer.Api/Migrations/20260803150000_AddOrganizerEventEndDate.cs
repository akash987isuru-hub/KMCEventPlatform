using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Organizer.Api.Data;

#nullable disable

namespace Organizer.Api.Migrations;

[DbContext(typeof(OrganizerDbContext))]
[Migration("20260803150000_AddOrganizerEventEndDate")]
public partial class AddOrganizerEventEndDate : Migration
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "EndDate", table: "Events");
    }
}
