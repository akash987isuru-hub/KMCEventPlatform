using KMC.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace KMC.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketTier> TicketTiers => Set<TicketTier>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PartnerEvent> PartnerEvents => Set<PartnerEvent>();
    public DbSet<PartnerEventTicketTier> PartnerEventTicketTiers => Set<PartnerEventTicketTier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<PartnerEvent>()
            .HasIndex(eventItem => eventItem.ExternalEventCode)
            .IsUnique();


        modelBuilder.Entity<PartnerEventTicketTier>()
            .Property(item => item.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PartnerEventTicketTier>()
            .HasOne(item => item.PartnerEvent)
            .WithMany(item => item.TicketTiers)
            .HasForeignKey(item => item.PartnerEventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PartnerEventTicketTier>()
            .HasIndex(item => new
            {
                item.PartnerEventId,
                item.ExternalTicketTierId
            })
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(user => user.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Event>()
            .Property(eventItem => eventItem.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Registration>()
            .Property(registration => registration.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(payment => payment.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TicketTier>()
            .Property(ticketTier => ticketTier.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(payment => payment.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Event>()
            .HasOne(eventItem => eventItem.Organizer)
            .WithMany(user => user.OrganizedEvents)
            .HasForeignKey(eventItem => eventItem.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketTier>()
            .HasOne(ticketTier => ticketTier.Event)
            .WithMany(eventItem => eventItem.TicketTiers)
            .HasForeignKey(ticketTier => ticketTier.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketTier>()
            .HasIndex(ticketTier => new
            {
                ticketTier.EventId,
                ticketTier.Name
            })
            .IsUnique();

        modelBuilder.Entity<Registration>()
            .HasOne(registration => registration.Event)
            .WithMany(eventItem => eventItem.Registrations)
            .HasForeignKey(registration => registration.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Registration>()
            .HasOne(registration => registration.Participant)
            .WithMany(user => user.Registrations)
            .HasForeignKey(registration => registration.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Registration>()
            .HasOne(registration => registration.TicketTier)
            .WithMany(ticketTier => ticketTier.Registrations)
            .HasForeignKey(registration => registration.TicketTierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Registration>()
            .HasIndex(registration => new
            {
                registration.EventId,
                registration.ParticipantId
            })
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasOne(payment => payment.Registration)
            .WithOne(registration => registration.Payment)
            .HasForeignKey<Payment>(payment => payment.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasIndex(payment => payment.RegistrationId)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(payment => payment.TransactionReference)
            .IsUnique();
    }
}
