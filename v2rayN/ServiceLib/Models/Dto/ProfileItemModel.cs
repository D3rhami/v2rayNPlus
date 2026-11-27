namespace ServiceLib.Models.Dto;

public partial class ProfileItemModel : ReactiveObject
{
    public bool IsActive { get; set; }
    public string IndexId { get; set; }
    public EConfigType ConfigType { get; set; }
    public string Remarks { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public string Network { get; set; }
    public string StreamSecurity { get; set; }
    public string Subid { get; set; }
    public string SubRemarks { get; set; }
    public int Sort { get; set; }

    [Reactive]
    public partial int Delay { get; set; }

    public decimal Speed { get; set; }

    [Reactive]
    public partial string DelayVal { get; set; }

    [Reactive]
    public partial string SpeedVal { get; set; }

    [Reactive]
    public partial string IpInfo { get; set; }

    [Reactive]
    public partial string YouTubeText { get; set; } = "—";

    [Reactive]
    public partial string NetflixText { get; set; } = "—";

    [Reactive]
    public partial string GeminiText { get; set; } = "—";

    [Reactive]
    public partial string ChatGptText { get; set; } = "—";

    [Reactive]
    public partial int YouTubeStatus { get; set; }

    [Reactive]
    public partial int NetflixStatus { get; set; }

    [Reactive]
    public partial int GeminiStatus { get; set; }

    [Reactive]
    public partial int ChatGptStatus { get; set; }

    public string? ExitIpU { get; set; }

    public string? ExitCountryU { get; set; }

    [Reactive]
    public partial string TodayUp { get; set; }

    [Reactive]
    public partial string TodayDown { get; set; }

    [Reactive]
    public partial string TotalUp { get; set; }

    [Reactive]
    public partial string TotalDown { get; set; }

    public string GetSummary()
    {
        var summary = $"[{ConfigType}] {Remarks}";
        if (!ConfigType.IsComplexType())
        {
            summary += $"({Address}:{Port})";
        }

        return summary;
    }
}
