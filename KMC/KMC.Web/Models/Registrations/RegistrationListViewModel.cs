namespace KMC.Web.Models.Registrations;

public class RegistrationListViewModel
{
    public IReadOnlyList<RegistrationViewModel> Registrations
    { get; set; } = [];

    public string? ErrorMessage { get; set; }
}