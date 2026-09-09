using Microsoft.EntityFrameworkCore;
using Organizer.Api.Entities;

namespace Organizer.Api.Data;

public class OrganizerDbContext : DbContext
{
    public OrganizerDbContext(DbContextOptions<OrganizerDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrganizerEvent> Events => Set<OrganizerEvent>();
    public DbSet<OrganizerTicketTier> TicketTiers => Set<OrganizerTicketTier>();
    public DbSet<OrganizerBooking> Bookings => Set<OrganizerBooking>();
    public DbSet<OrganizerPayment> Payments => Set<OrganizerPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrganizerEvent>()
            .HasIndex(eventItem => eventItem.ExternalEventCode)
            .IsUnique();

        modelBuilder.Entity<OrganizerTicketTier>()
            .Property(item => item.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizerBooking>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizerBooking>()
            .Property(item => item.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizerPayment>()
            .Property(item => item.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrganizerTicketTier>()
            .HasOne(item => item.Event)
            .WithMany(item => item.TicketTiers)
            .HasForeignKey(item => item.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizerTicketTier>()
            .HasIndex(item => new { item.EventId, item.Name })
            .IsUnique();

        modelBuilder.Entity<OrganizerBooking>()
            .HasOne(item => item.Event)
            .WithMany(item => item.Bookings)
            .HasForeignKey(item => item.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizerBooking>()
            .HasOne(item => item.TicketTier)
            .WithMany(item => item.Bookings)
            .HasForeignKey(item => item.TicketTierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrganizerBooking>()
            .HasIndex(item => item.BookingReference)
            .IsUnique();

        modelBuilder.Entity<OrganizerPayment>()
            .HasOne(item => item.Booking)
            .WithOne(item => item.Payment)
            .HasForeignKey<OrganizerPayment>(item => item.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizerPayment>()
            .HasIndex(item => item.BookingId)
            .IsUnique();

        modelBuilder.Entity<OrganizerPayment>()
            .HasIndex(item => item.TransactionReference)
            .IsUnique();
    }
}
