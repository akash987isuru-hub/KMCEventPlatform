using KMC.Api.Data;
using KMC.Api.DTOs.Registrations;
using KMC.Api.Entities;
using KMC.Api.Infrastructure;
using KMC.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KMC.Api.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _context;

    public RegistrationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegistrationOperationResult> RegisterForEventAsync(
        int eventId,
        int participantId,
        PurchaseTicketRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var participant = await _context.Users
            .FirstOrDefaultAsync(
                user =>
                    user.Id == participantId &&
                    user.Role == UserRole.Participant,
                cancellationToken);

        if (participant is null)
        {
            return new RegistrationOperationResult
            {
                Status = RegistrationOperationStatus.Forbidden,
                Message = "A valid public user account is required."
            };
        }

        var eventEntity = await _context.Events
            .Include(eventItem => eventItem.TicketTiers)
                .ThenInclude(ticketTier => ticketTier.Registrations)
            .Include(eventItem => eventItem.Registrations)
                .ThenInclude(registration => registration.Payment)
            .FirstOrDefaultAsync(
                eventItem => eventItem.Id == eventId,
                cancellationToken);

        if (eventEntity is null)
        {
            return new RegistrationOperationResult
            {
                Status = RegistrationOperationStatus.NotFound,
                Message = "Event not found."
            };
        }

        if (eventEntity.Status != EventStatus.Published)
        {
            return Invalid(
                "Ticket purchase is available only for published events.");
        }

        var eventEndDateTime =
            eventEntity.EndDate.Date.Add(eventEntity.EndTime);

        if (eventEndDateTime <= KmcTime.Now)
        {
            return Invalid(
                "Ticket purchase is not available because this event has ended.");
        }

        var ticketTier = eventEntity.TicketTiers.FirstOrDefault(
            item => item.Id == request.TicketTierId);

        if (ticketTier is null)
        {
            return Invalid(
                "Select a valid ticket category for this event.");
        }

        if (request.Quantity is < 1 or > 10)
        {
            return Invalid("Choose between 1 and 10 tickets.");
        }

        var existingRegistration =
            eventEntity.Registrations.FirstOrDefault(
                registration =>
                    registration.ParticipantId == participantId);

        if (existingRegistration?.Status == RegistrationStatus.Confirmed)
        {
            return new RegistrationOperationResult
            {
                Status = RegistrationOperationStatus.Conflict,
                Message =
                    "You already have an active ticket order for this event."
            };
        }

        var soldForTier = ticketTier.Registrations
            .Where(registration =>
                registration.Status == RegistrationStatus.Confirmed)
            .Sum(registration => registration.Quantity);

        var remainingForTier =
            Math.Max(0, ticketTier.Capacity - soldForTier);

        if (remainingForTier <= 0)
        {
            return Invalid(
                $"The {ticketTier.Name} ticket category is sold out.");
        }

        if (request.Quantity > remainingForTier)
        {
            return Invalid(
                $"Only {remainingForTier} {ticketTier.Name} ticket(s) are available.");
        }

        var confirmedEventCount = eventEntity.Registrations
            .Where(registration =>
                registration.Status == RegistrationStatus.Confirmed)
            .Sum(registration => registration.Quantity);

        var remainingForEvent =
            Math.Max(0, eventEntity.Capacity - confirmedEventCount);

        if (request.Quantity > remainingForEvent)
        {
            return Invalid(
                $"Only {remainingForEvent} ticket(s) remain for this event.");
        }

        var paymentValidation = ValidatePayment(request);

        if (paymentValidation is not null)
        {
            return Invalid(paymentValidation);
        }

        var normalizedCardNumber = NormalizeCardNumber(
            request.CardNumber);
        var now = DateTime.UtcNow;
        var transactionReference = CreateTransactionReference();
        var cardBrand = DetectCardBrand(normalizedCardNumber);
        var cardLastFour = normalizedCardNumber[^4..];
        var totalAmount = decimal.Round(
            ticketTier.Price * request.Quantity,
            2,
            MidpointRounding.AwayFromZero);

        Registration registration;

        if (existingRegistration is not null)
        {
            registration = existingRegistration;
            registration.TicketTierId = ticketTier.Id;
            registration.TicketTier = ticketTier;
            registration.Quantity = request.Quantity;
            registration.Status = RegistrationStatus.Confirmed;
            registration.RegisteredAt = now;

            if (registration.Payment is null)
            {
                registration.Payment = new Payment();
            }

            ApplyPaymentValues(
                registration.Payment,
                totalAmount,
                request.CardHolderName,
                cardBrand,
                cardLastFour,
                transactionReference,
                now);
        }
        else
        {
            registration = new Registration
            {
                EventId = eventEntity.Id,
                Event = eventEntity,
                ParticipantId = participant.Id,
                Participant = participant,
                TicketTierId = ticketTier.Id,
                TicketTier = ticketTier,
                Quantity = request.Quantity,
                RegisteredAt = now,
                Status = RegistrationStatus.Confirmed,
                Payment = new Payment()
            };

            ApplyPaymentValues(
                registration.Payment,
                totalAmount,
                request.CardHolderName,
                cardBrand,
                cardLastFour,
                transactionReference,
                now);

            _context.Registrations.Add(registration);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RegistrationOperationResult
        {
            Status = RegistrationOperationStatus.Success,
            Message = existingRegistration is null
                ? $"{request.Quantity} ticket(s) purchased successfully."
                : $"{request.Quantity} ticket(s) restored successfully.",
            Data = CreateRegistrationResponse(
                registration,
                eventEntity,
                participant,
                ticketTier,
                registration.Payment)
        };
    }

    public async Task<IReadOnlyList<RegistrationResponseDto>>
        GetMyRegistrationsAsync(
            int participantId,
            CancellationToken cancellationToken = default)
    {
        var registrations = await _context.Registrations
            .AsNoTracking()
            .Include(registration => registration.Event)
            .Include(registration => registration.Participant)
            .Include(registration => registration.TicketTier)
            .Include(registration => registration.Payment)
            .Where(registration =>
                registration.ParticipantId == participantId)
            .OrderByDescending(registration =>
                registration.RegisteredAt)
            .ToListAsync(cancellationToken);

        return registrations
            .Select(registration => CreateRegistrationResponse(
                registration,
                registration.Event,
                registration.Participant,
                registration.TicketTier,
                registration.Payment))
            .ToList();
    }

    public async Task<ParticipantListResult>
        GetEventParticipantsAsync(
            int eventId,
            int organizerId,
            CancellationToken cancellationToken = default)
    {
        var eventEntity = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(
                eventItem => eventItem.Id == eventId,
                cancellationToken);

        if (eventEntity is null)
        {
            return new ParticipantListResult
            {
                Status = RegistrationOperationStatus.NotFound,
                Message = "Event not found."
            };
        }

        if (eventEntity.OrganizerId != organizerId)
        {
            return new ParticipantListResult
            {
                Status = RegistrationOperationStatus.Forbidden,
                Message =
                    "Only the event creator can view its ticket holders."
            };
        }

        var participants = await _context.Registrations
            .AsNoTracking()
            .Include(registration => registration.Participant)
            .Include(registration => registration.TicketTier)
            .Include(registration => registration.Payment)
            .Where(registration =>
                registration.EventId == eventId)
            .OrderByDescending(registration =>
                registration.RegisteredAt)
            .Select(registration =>
                new ParticipantResponseDto
                {
                    RegistrationId = registration.Id,
                    ParticipantId = registration.ParticipantId,
                    FullName = registration.Participant.FullName,
                    Email = registration.Participant.Email,
                    TicketTierName = registration.TicketTier.Name,
                    Quantity = registration.Quantity,
                    TicketPrice = registration.TicketTier.Price,
                    TotalAmount = registration.Payment == null
                        ? registration.TicketTier.Price *
                          registration.Quantity
                        : registration.Payment.Amount,
                    PaymentStatus = registration.Payment == null
                        ? "Legacy"
                        : registration.Payment.Status.ToString(),
                    TransactionReference = registration.Payment == null
                        ? string.Empty
                        : registration.Payment.TransactionReference,
                    RegisteredAt = registration.RegisteredAt,
                    Status = registration.Status.ToString()
                })
            .ToListAsync(cancellationToken);

        return new ParticipantListResult
        {
            Status = RegistrationOperationStatus.Success,
            Message = "Ticket holders retrieved successfully.",
            Participants = participants
        };
    }

    public async Task<RegistrationOperationResult>
        CancelRegistrationAsync(
            int registrationId,
            int participantId,
            CancellationToken cancellationToken = default)
    {
        var registration = await _context.Registrations
            .Include(item => item.Event)
            .Include(item => item.Participant)
            .Include(item => item.TicketTier)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(
                item =>
                    item.Id == registrationId &&
                    item.ParticipantId == participantId,
                cancellationToken);

        if (registration is null)
        {
            return new RegistrationOperationResult
            {
                Status = RegistrationOperationStatus.NotFound,
                Message = "Ticket order not found."
            };
        }

        if (registration.Status == RegistrationStatus.Cancelled)
        {
            return Invalid(
                "This ticket order is already cancelled.");
        }

        var eventStartDateTime =
            registration.Event.EventDate.Date.Add(
                registration.Event.StartTime);

        if (eventStartDateTime <= KmcTime.Now)
        {
            return Invalid(
                "A ticket order cannot be cancelled after the event has started.");
        }

        registration.Status = RegistrationStatus.Cancelled;

        if (registration.Payment is not null &&
            registration.Payment.Status == PaymentStatus.Approved)
        {
            registration.Payment.Status = PaymentStatus.Refunded;
            registration.Payment.RefundedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RegistrationOperationResult
        {
            Status = RegistrationOperationStatus.Success,
            Message =
                "Ticket order cancelled successfully. The full payment has been marked as refunded.",
            Data = CreateRegistrationResponse(
                registration,
                registration.Event,
                registration.Participant,
                registration.TicketTier,
                registration.Payment)
        };
    }

    private static string? ValidatePayment(
        PurchaseTicketRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CardHolderName) ||
            request.CardHolderName.Trim().Length < 2)
        {
            return "Enter the card holder name exactly as shown on the card.";
        }

        if (string.IsNullOrWhiteSpace(request.CardNumber) ||
            request.CardNumber.Any(character =>
                !char.IsDigit(character) &&
                character != ' ' && character != '-'))
        {
            return "Card number may contain only digits, spaces or hyphens.";
        }

        var cardNumber = NormalizeCardNumber(request.CardNumber);

        if (cardNumber.Length != 16)
        {
            return "Enter a card number containing exactly 16 digits.";
        }

        var currentMonth = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            1);

        DateTime expiryMonth;

        try
        {
            expiryMonth = new DateTime(
                request.ExpiryYear,
                request.ExpiryMonth,
                1);
        }
        catch (ArgumentOutOfRangeException)
        {
            return "Enter a valid card expiry month and year.";
        }

        if (expiryMonth < currentMonth)
        {
            return "The card expiry date must be in the future.";
        }

        if (request.Cvv.Length != 3 ||
            request.Cvv.Any(character => !char.IsDigit(character)))
        {
            return "CVV must contain exactly 3 digits.";
        }

        return null;
    }

    private static string NormalizeCardNumber(string cardNumber)
    {
        return new string(cardNumber
            .Where(char.IsDigit)
            .ToArray());
    }

    private static string DetectCardBrand(string cardNumber)
    {
        if (cardNumber.StartsWith('4'))
        {
            return "Visa";
        }

        if (cardNumber.Length >= 2 &&
            int.TryParse(cardNumber[..2], out var firstTwo) &&
            firstTwo is >= 51 and <= 55)
        {
            return "Mastercard";
        }

        if (cardNumber.StartsWith("34") ||
            cardNumber.StartsWith("37"))
        {
            return "American Express";
        }

        return "Bank Card";
    }

    private static string CreateTransactionReference()
    {
        var reference =
            $"KMC-PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";

        return reference[..Math.Min(44, reference.Length)]
            .ToUpperInvariant();
    }

    private static void ApplyPaymentValues(
        Payment payment,
        decimal amount,
        string cardHolderName,
        string cardBrand,
        string cardLastFour,
        string transactionReference,
        DateTime paidAt)
    {
        payment.Amount = amount;
        payment.Currency = "LKR";
        payment.CardHolderName = cardHolderName.Trim();
        payment.CardBrand = cardBrand;
        payment.CardLastFour = cardLastFour;
        payment.TransactionReference = transactionReference;
        payment.Status = PaymentStatus.Approved;
        payment.PaidAt = paidAt;
        payment.RefundedAt = null;
    }

    private static RegistrationResponseDto CreateRegistrationResponse(
        Registration registration,
        Event eventEntity,
        User participant,
        TicketTier ticketTier,
        Payment? payment)
    {
        return new RegistrationResponseDto
        {
            Id = registration.Id,
            EventId = eventEntity.Id,
            EventTitle = eventEntity.Title,
            EventDate = eventEntity.EventDate,
            EndDate = eventEntity.EndDate,
            StartTime = eventEntity.StartTime,
            EndTime = eventEntity.EndTime,
            EventStatus = eventEntity.Status.ToString(),
            ParticipantId = participant.Id,
            ParticipantName = participant.FullName,
            ParticipantEmail = participant.Email,
            TicketTierId = ticketTier.Id,
            TicketTierName = ticketTier.Name,
            Quantity = registration.Quantity,
            TicketPrice = ticketTier.Price,
            TotalAmount = payment?.Amount ??
                ticketTier.Price * registration.Quantity,
            PaymentStatus = payment?.Status.ToString() ?? "Legacy",
            TransactionReference =
                payment?.TransactionReference ?? string.Empty,
            CardDisplay = payment is null
                ? string.Empty
                : $"{payment.CardBrand} ending {payment.CardLastFour}",
            PaidAt = payment?.PaidAt,
            RegisteredAt = registration.RegisteredAt,
            Status = registration.Status.ToString()
        };
    }

    private static RegistrationOperationResult Invalid(string message)
    {
        return new RegistrationOperationResult
        {
            Status = RegistrationOperationStatus.Invalid,
            Message = message
        };
    }
}
