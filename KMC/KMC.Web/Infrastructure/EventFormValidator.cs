using KMC.Web.Models.Organizer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KMC.Web.Infrastructure;

public static class EventFormValidator
{
    public static void Validate(
        EventFormViewModel model,
        ModelStateDictionary modelState,
        string prefix = "Input")
    {
        string Key(string propertyName) =>
            string.IsNullOrWhiteSpace(prefix)
                ? propertyName
                : $"{prefix}.{propertyName}";
        EnsureThreeTicketTiers(model);

        var requiresFutureStart =
            model.Status is "Draft" or "Published";

        var eventStart = model.EventDate.Date.Add(model.StartTime);
        var eventEnd = model.EndDate.Date.Add(model.EndTime);

        if (requiresFutureStart && eventStart <= DateTime.Now)
        {
            modelState.AddModelError(
                Key(nameof(model.EventDate)),
                "Draft and published events must start in the future.");
        }

        if (eventEnd <= eventStart)
        {
            modelState.AddModelError(
                Key(nameof(model.EndDate)),
                "The event end date and time must be later than its start date and time.");
        }

        if (model.TicketTiers.Count != 3)
        {
            modelState.AddModelError(
                Key(nameof(model.TicketTiers)),
                "Exactly three ticket categories are required.");
            return;
        }

        var uniqueNames = model.TicketTiers
            .Select(ticketTier => ticketTier.Name?.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (uniqueNames != 3)
        {
            modelState.AddModelError(
                Key(nameof(model.TicketTiers)),
                "Ticket category names must be unique.");
        }

        for (var index = 0; index < model.TicketTiers.Count; index++)
        {
            model.TicketTiers[index].SortOrder = index + 1;
        }
    }

    public static void EnsureThreeTicketTiers(
        EventFormViewModel model)
    {
        model.TicketTiers ??= [];

        var defaults = EventFormViewModel.CreateDefaultTicketTiers();

        while (model.TicketTiers.Count < 3)
        {
            model.TicketTiers.Add(defaults[model.TicketTiers.Count]);
        }

        if (model.TicketTiers.Count > 3)
        {
            model.TicketTiers = model.TicketTiers.Take(3).ToList();
        }

        for (var index = 0; index < model.TicketTiers.Count; index++)
        {
            model.TicketTiers[index].SortOrder = index + 1;
        }
    }
}
