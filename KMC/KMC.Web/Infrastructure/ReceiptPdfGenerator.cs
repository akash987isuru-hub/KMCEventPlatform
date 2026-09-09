using System.Globalization;
using System.Text;
using KMC.Web.Models.Registrations;

namespace KMC.Web.Infrastructure;

public static class ReceiptPdfGenerator
{
    public static byte[] Generate(RegistrationViewModel registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F2 19 Tf");
        content.AppendLine("50 790 Td");
        content.AppendLine($"({Escape("KMC PAYMENT RECEIPT")}) Tj");

        AddLine(content, "Kandy Municipal Council Event Platform", 12, false, 26);
        AddLine(content, $"Receipt number: KMC-RCP-{registration.Id:000000}", 11, true, 34);
        AddLine(content, $"Public user: {registration.ParticipantName}", 11, false, 22);
        AddLine(content, $"Event: {registration.EventTitle}", 11, false, 22);
        AddLine(content, $"Ticket category: {registration.TicketTierName}", 11, false, 22);
        AddLine(content, $"Quantity: {registration.Quantity}", 11, false, 18);
        AddLine(content, $"Unit price: LKR {registration.TicketPrice:N2}", 11, false, 18);
        AddLine(content, $"Total paid: LKR {registration.TotalAmount:N2}", 11, true, 24);
        AddLine(content, $"Payment status: {registration.PaymentStatus}", 11, false, 22);
        AddLine(content, $"Card: {Fallback(registration.CardDisplay, "Not available")}", 11, false, 22);
        AddLine(content, $"Transaction reference: {Fallback(registration.TransactionReference, "Not available")}", 9, false, 22);
        AddLine(content, $"Payment date: {(registration.PaidAt?.ToString("dd MMM yyyy, hh:mm tt", CultureInfo.InvariantCulture) ?? "Not available")}", 10, false, 22);
        AddLine(content, "This receipt confirms a demo payment recorded by the KMC Event Platform.", 9, false, 34);
        content.AppendLine("ET");

        return BuildPdf(content.ToString());
    }

    private static void AddLine(StringBuilder content, string text, int fontSize, bool bold, int verticalOffset)
    {
        content.AppendLine($"0 -{verticalOffset} Td");
        content.AppendLine($"/F{(bold ? 2 : 1)} {fontSize} Tf");
        content.AppendLine($"({Escape(text)}) Tj");
    }

    private static string Fallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static string Escape(string value)
    {
        var safe = new string(value.Select(character => character is >= ' ' and <= '~' ? character : '?').ToArray());
        return safe.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private static byte[] BuildPdf(string contentStream)
    {
        using var stream = new MemoryStream();
        var offsets = new long[7];
        WriteAscii(stream, "%PDF-1.4\n");
        offsets[1] = stream.Position;
        WriteAscii(stream, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        offsets[2] = stream.Position;
        WriteAscii(stream, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        offsets[3] = stream.Position;
        WriteAscii(stream, "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>\nendobj\n");
        offsets[4] = stream.Position;
        WriteAscii(stream, "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
        offsets[5] = stream.Position;
        WriteAscii(stream, "5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n");
        var contentBytes = Encoding.ASCII.GetBytes(contentStream);
        offsets[6] = stream.Position;
        WriteAscii(stream, $"6 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        stream.Write(contentBytes, 0, contentBytes.Length);
        WriteAscii(stream, "\nendstream\nendobj\n");
        var crossReferenceOffset = stream.Position;
        WriteAscii(stream, "xref\n0 7\n0000000000 65535 f \n");
        for (var index = 1; index <= 6; index++)
        {
            WriteAscii(stream, $"{offsets[index]:D10} 00000 n \n");
        }
        WriteAscii(stream, $"trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n{crossReferenceOffset}\n%%EOF");
        return stream.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
