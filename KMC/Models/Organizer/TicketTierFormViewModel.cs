using System.ComponentModel.DataAnnotations;

namespace KMC.Web.Models.Organizer;

public class TicketTierFormViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(60)]
    [Display(Name = "Ticket category")]
    public string Name { get; set; } = string.Empty;

    [Range(
        0,
        1000000,
        ErrorMessage = "Ticket price cannot be negative.")]
    [Display(Name = "Price (LKR)")]
    public decimal Price { get; set; }

    [Range(
        1,
        100000,
        ErrorMessage = "Ticket capacity must be greater than zero.")]
    public int Capacity { get; set; }

    public int SortOrder { get; set; }
}
