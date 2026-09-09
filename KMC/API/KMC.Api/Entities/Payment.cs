using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KMC.Api.Entities;

public class Payment
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }

    public Registration Registration { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "LKR";

    [Required]
    [MaxLength(100)]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CardBrand { get; set; } = string.Empty;

    [Required]
    [MaxLength(4)]
    public string CardLastFour { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string TransactionReference { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Approved;

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public DateTime? RefundedAt { get; set; }
}
