using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KMC.Api.Entities;

public class PartnerEventTicketTier
{
    public int Id { get; set; }
    public int PartnerEventId { get; set; }
    public PartnerEvent PartnerEvent { get; set; } = null!;
    public int ExternalTicketTierId { get; set; }

    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Capacity { get; set; }
    public int SoldCount { get; set; }
    public int SortOrder { get; set; }
}
