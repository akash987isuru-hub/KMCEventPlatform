using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Organizer.Api.Entities;

public class OrganizerTicketTier
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public OrganizerEvent Event { get; set; } = null!;

    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Capacity { get; set; }
    public int SortOrder { get; set; }

    public ICollection<OrganizerBooking> Bookings { get; set; }
        = new List<OrganizerBooking>();
}
