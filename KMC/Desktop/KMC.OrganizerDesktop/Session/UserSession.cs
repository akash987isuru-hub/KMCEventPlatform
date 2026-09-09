namespace KMC.OrganizerDesktop.Session;

public static class UserSession
{
    public static int UserId { get; set; }

    public static string FullName { get; set; } = string.Empty;

    public static string Email { get; set; } = string.Empty;

    public static string Role { get; set; } = string.Empty;

    public static string Token { get; set; } = string.Empty;

    public static DateTime TokenExpiration { get; set; }

    public static bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(Token);

    public static void Clear()
    {
        UserId = 0;
        FullName = string.Empty;
        Email = string.Empty;
        Role = string.Empty;
        Token = string.Empty;
        TokenExpiration = default;
    }
}