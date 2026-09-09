using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KMC.Api.Entities;

public class TicketTier
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public Event Event { get; set; } = null!;

    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Capacity { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Registration> Registrations { get; set; }
        = new List<Registration>();
}
