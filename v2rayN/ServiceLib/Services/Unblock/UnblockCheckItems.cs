namespace ServiceLib.Services.Unblock;

public sealed class ServiceCheckItem
{
    public string Service { get; set; } = "";
    public string Url { get; set; } = "";
}

public static class UnblockCheckRegistry
{
    public static List<ServiceCheckItem> DefaultChecks =>
    [
        new ServiceCheckItem { Service = "YouTube", Url = "https://www.youtube.com/" },
        new ServiceCheckItem { Service = "Netflix", Url = "https://www.netflix.com/title/81280792" },
        new ServiceCheckItem { Service = "Gemini", Url = "https://gemini.google.com/" },
        new ServiceCheckItem { Service = "ChatGPT", Url = "https://chatgpt.com/" },
    ];
}
