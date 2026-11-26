namespace ServiceLib.Services.Unblock;

public enum EUnblockStatus
{
    Unknown = 0,
    Ok = 1,
    Lock = 2,
    Risk = 3,
    Fail = 4,
}

public class UnblockCheckEvidence
{
    public int HttpStatusCode { get; set; }
    public string? FinalUrl { get; set; }
    public int ElapsedMs { get; set; }
    public string? MatchedMarker { get; set; }
    public string? Reason { get; set; }
}

public class UnblockServiceResult
{
    public string Service { get; set; } = "";
    public EUnblockStatus Status { get; set; } = EUnblockStatus.Unknown;
    public UnblockCheckEvidence? Evidence { get; set; }
}

public class UnblockNodeResult
{
    public string? ExitIp { get; set; }
    public string? ExitCountry { get; set; }
    public int LatencyMedianMs { get; set; }
    public int AttemptsOk { get; set; }
    public int AttemptsTotal { get; set; }
    public List<UnblockServiceResult> Services { get; set; } = new();
    public bool PredictedLockGemini { get; set; }
    public bool PredictedLockChatGpt { get; set; }
}
