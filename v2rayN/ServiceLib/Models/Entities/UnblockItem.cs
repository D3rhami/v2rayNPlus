namespace ServiceLib.Models.Entities;

public class UnblockItem
{
    [PrimaryKey]
    public string IndexId { get; set; } = "";

    public string? ExitIp { get; set; }
    public string? ExitCountry { get; set; }
    public int LatencyMedianMs { get; set; }
    public int AttemptsOk { get; set; }
    public int AttemptsTotal { get; set; }
    public int YtStatus { get; set; }
    public int NfStatus { get; set; }
    public int GemStatus { get; set; }
    public int ChatStatus { get; set; }
    public string? EvidenceJson { get; set; }
    public bool PredictedLockGemini { get; set; }
    public bool PredictedLockChatGpt { get; set; }
    public string? TestTime { get; set; }
}
