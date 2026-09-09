using KMC.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMC.Api.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260803180000_RemoveUnwantedDemoEvents")]
public partial class RemoveUnwantedDemoEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE payment
            FROM [Payments] AS payment
            INNER JOIN [Registrations] AS registration
                ON payment.[RegistrationId] = registration.[Id]
            INNER JOIN [Events] AS eventItem
                ON registration.[EventId] = eventItem.[Id]
            WHERE eventItem.[Title] IN (
                N'Kandy Cultural Festival',
                N'Kandy Cultural Night 2026',
                N'Kandy Food and Cultural Festival 2026',
                N'sdvvd'
            );

            DELETE registration
            FROM [Registrations] AS registration
            INNER JOIN [Events] AS eventItem
                ON registration.[EventId] = eventItem.[Id]
            WHERE eventItem.[Title] IN (
                N'Kandy Cultural Festival',
                N'Kandy Cultural Night 2026',
                N'Kandy Food and Cultural Festival 2026',
                N'sdvvd'
            );

            DELETE ticketTier
            FROM [TicketTiers] AS ticketTier
            INNER JOIN [Events] AS eventItem
                ON ticketTier.[EventId] = eventItem.[Id]
            WHERE eventItem.[Title] IN (
                N'Kandy Cultural Festival',
                N'Kandy Cultural Night 2026',
                N'Kandy Food and Cultural Festival 2026',
                N'sdvvd'
            );

            DELETE FROM [Events]
            WHERE [Title] IN (
                N'Kandy Cultural Festival',
                N'Kandy Cultural Night 2026',
                N'Kandy Food and Cultural Festival 2026',
                N'sdvvd'
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Deleted demo records are intentionally not recreated.
    }
}
