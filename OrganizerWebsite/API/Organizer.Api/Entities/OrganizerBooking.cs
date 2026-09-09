using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Organizer.Api.Entities;

public class OrganizerBooking
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string BookingReference { get; set; } = string.Empty;

    public int EventId { get; set; }
    public OrganizerEvent Event { get; set; } = null!;

    public int TicketTierId { get; set; }
    public OrganizerTicketTier TicketTier { get; set; } = null!;

    [Required, MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(30)]
    public string BookingSource { get; set; } = "Sparkling Events Kandy";

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OrganizerPayment Payment { get; set; } = null!;
}
