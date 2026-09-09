using KMC.Web.Models.Events;
using KMC.Web.Models.Registrations;

namespace KMC.Web.Models.Participant;

public class ParticipantDashboardViewModel
{
    public IReadOnlyList<RegistrationViewModel> Registrations { get; set; }
        = Array.Empty<RegistrationViewModel>();

    public IReadOnlyList<EventViewModel> RecommendedEvents { get; set; }
        = Array.Empty<EventViewModel>();

    public int TotalTickets => Registrations.Sum(registration => Math.Max(1, registration.Quantity));

    public int ConfirmedTickets => Registrations
        .Where(registration => string.Equals(
            registration.Status,
            "Confirmed",
            StringComparison.OrdinalIgnoreCase))
        .Sum(registration => Math.Max(1, registration.Quantity));

    public int UpcomingEvents => Registrations.Count(registration =>
        string.Equals(
            registration.Status,
            "Confirmed",
            StringComparison.OrdinalIgnoreCase) &&
        registration.EventDate.Date.Add(registration.StartTime) > DateTime.Now);

    public decimal TotalPaid => Registrations
        .Where(registration =>
            string.Equals(
                registration.PaymentStatus,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
        .Sum(registration => registration.TotalAmount);

    public string? ErrorMessage { get; set; }
}
