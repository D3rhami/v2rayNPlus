namespace ServiceLib.Services.Unblock;

public static class CountryLockLists
{
    public static readonly HashSet<string> GeminiLockedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "CN", "CU", "HK", "IR", "KP", "RU", "SY", "MO", "BY", "VE",
    };

    public static readonly HashSet<string> ChatGptLockedCountries = new(StringComparer.OrdinalIgnoreCase)
    {
        "CN", "CU", "IR", "KP", "RU", "SY", "BY", "VE",
    };

    public static bool IsGeminiLocked(string? iso) => !string.IsNullOrEmpty(iso) && GeminiLockedCountries.Contains(iso);

    public static bool IsChatGptLocked(string? iso) => !string.IsNullOrEmpty(iso) && ChatGptLockedCountries.Contains(iso);
}
