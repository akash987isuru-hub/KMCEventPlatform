using System.ComponentModel.DataAnnotations;

namespace Organizer.Web.Models;

public class TicketTierFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(180)]
    public string? Description { get; set; }

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(1, 5)]
    public int SortOrder { get; set; }
}
