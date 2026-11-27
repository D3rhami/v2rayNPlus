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
    [Column("YtStatus")]
    public int YouTubeStatus { get; set; }
    [Column("NfStatus")]
    public int NetflixStatus { get; set; }
    [Column("GemStatus")]
    public int GeminiStatus { get; set; }
    [Column("ChatStatus")]
    public int ChatGptStatus { get; set; }
    public string? EvidenceJson { get; set; }
    public bool PredictedLockGemini { get; set; }
    public bool PredictedLockChatGpt { get; set; }
    public string? TestTime { get; set; }
}
