namespace KMC.Api.Infrastructure;

public static class KmcTime
{
    private static readonly TimeZoneInfo SriLankaTimeZone =
        ResolveSriLankaTimeZone();

    public static DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            SriLankaTimeZone);

    private static TimeZoneInfo ResolveSriLankaTimeZone()
    {
        var timeZoneId = OperatingSystem.IsWindows()
            ? "Sri Lanka Standard Time"
            : "Asia/Colombo";

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return CreateFallbackTimeZone();
        }
        catch (InvalidTimeZoneException)
        {
            return CreateFallbackTimeZone();
        }
    }

    private static TimeZoneInfo CreateFallbackTimeZone()
    {
        return TimeZoneInfo.CreateCustomTimeZone(
            "KMC Sri Lanka Time",
            TimeSpan.FromHours(5.5),
            "Sri Lanka Standard Time",
            "Sri Lanka Standard Time");
    }
}
