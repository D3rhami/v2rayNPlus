namespace ServiceLib.Services.Unblock;

public static class UnblockChecker
{
    private const int RequestTimeoutMs = 8000;
    private const int MaxBodyChars = 200000;
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

    public static async Task<UnblockNodeResult> CheckServicesAsync(HttpClient client, IEnumerable<string> services, CancellationToken ct)
    {
        var nodeResult = new UnblockNodeResult();

        foreach (var service in services)
        {
            nodeResult.Services.Add(await CheckOneServiceAsync(client, service, ct));

            if (ct.IsCancellationRequested)
            {
                break;
            }
        }

        return nodeResult;
    }

    private static async Task<UnblockServiceResult> CheckOneServiceAsync(HttpClient client, string service, CancellationToken ct)
    {
        var item = UnblockCheckRegistry.DefaultChecks
            .FirstOrDefault(check => check.Service.Equals(service, StringComparison.OrdinalIgnoreCase));
        if (item == null)
        {
            return new UnblockServiceResult
            {
                Service = service,
                Status = EUnblockStatus.Unknown,
                Evidence = new UnblockCheckEvidence { Reason = "no check defined" },
            };
        }

        var evidence = new UnblockCheckEvidence();
        var stopwatch = Stopwatch.StartNew();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(RequestTimeoutMs);
        try
        {
            var result = await ExecuteCheckAsync(client, service, item, evidence, linkedCts.Token);
            stopwatch.Stop();
            evidence.ElapsedMs = (int)stopwatch.ElapsedMilliseconds;
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            evidence.ElapsedMs = (int)stopwatch.ElapsedMilliseconds;
            evidence.Reason = DescribeException(ex, ct);
            return new UnblockServiceResult { Service = service, Status = EUnblockStatus.Fail, Evidence = evidence };
        }
    }

    private static async Task<UnblockServiceResult> ExecuteCheckAsync(HttpClient client, string service, ServiceCheckItem item,
        UnblockCheckEvidence evidence, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, item.Url);
        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        evidence.HttpStatusCode = (int)response.StatusCode;
        evidence.FinalUrl = response.RequestMessage?.RequestUri?.ToString();
        var body = await ReadBodyCappedAsync(response.Content, ct);

        var result = new UnblockServiceResult { Service = service, Evidence = evidence };
        switch (item.Service)
        {
            case "YouTube":
                ClassifyYouTube(result, evidence, body);
                break;

            case "Netflix":
                ClassifyNetflix(result, evidence, body);
                break;

            case "Gemini":
                ClassifyGemini(result, evidence, body);
                break;

            case "ChatGPT":
                ClassifyChatGpt(result, evidence, body, response);
                break;

            default:
                evidence.Reason = "no check defined";
                break;
        }

        return result;
    }

    private static async Task<string> ReadBodyCappedAsync(HttpContent content, CancellationToken ct)
    {
        var stream = await content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var sb = new StringBuilder();
        var buffer = new char[8192];
        while (sb.Length < MaxBodyChars)
        {
            var read = await reader.ReadAsync(buffer, ct);
            if (read == 0)
            {
                break;
            }

            sb.Append(buffer, 0, read);
        }

        if (sb.Length > MaxBodyChars)
        {
            sb.Length = MaxBodyChars;
        }

        return sb.ToString();
    }

    private static string DescribeException(Exception ex, CancellationToken ct)
    {
        if (ex is OperationCanceledException)
        {
            return ct.IsCancellationRequested ? "cancelled" : "timeout";
        }

        var root = ex.GetBaseException();
        if (root is SocketException socketException && socketException.SocketErrorCode == SocketError.ConnectionRefused)
        {
            return "connection refused";
        }

        return root.GetType().Name;
    }

    private static string? FindMarker(string body, params string[] markers)
    {
        return markers.FirstOrDefault(marker => body.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static void ClassifyYouTube(UnblockServiceResult result, UnblockCheckEvidence evidence, string body)
    {
        if (evidence.FinalUrl != null && evidence.FinalUrl.Contains("consent.youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            result.Status = EUnblockStatus.Ok;
            evidence.MatchedMarker = "consent redirect";
            evidence.Reason = "redirected to consent page";
            return;
        }

        switch (evidence.HttpStatusCode)
        {
            case 200:
                result.Status = EUnblockStatus.Ok;
                evidence.Reason = "reachable";
                break;

            case 403:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "403 without marker";
                break;

            case 429:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "429 rate limited";
                break;

            case 451:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "451 legal block possible";
                break;

            default:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "unexpected status";
                break;
        }
    }

    private static void ClassifyNetflix(UnblockServiceResult result, UnblockCheckEvidence evidence, string body)
    {
        var lockMarker = FindMarker(body, "Sorry, this page isn't available", "not available in your region");
        if (lockMarker != null)
        {
            result.Status = EUnblockStatus.Lock;
            evidence.MatchedMarker = lockMarker;
            evidence.Reason = "region restriction page";
            return;
        }

        if (evidence.FinalUrl != null && evidence.FinalUrl.Contains("/login", StringComparison.OrdinalIgnoreCase))
        {
            result.Status = EUnblockStatus.Ok;
            evidence.MatchedMarker = "login wall";
            evidence.Reason = "redirected to login page";
            return;
        }

        switch (evidence.HttpStatusCode)
        {
            case 200 when evidence.FinalUrl != null
                && evidence.FinalUrl.Contains("/title/", StringComparison.OrdinalIgnoreCase)
                && !body.Contains("not available", StringComparison.OrdinalIgnoreCase)
                && !body.Contains("Sorry, this page isn't available", StringComparison.OrdinalIgnoreCase):
                result.Status = EUnblockStatus.Ok;
                evidence.Reason = "reachable";
                break;

            case 403:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "403 access denied";
                break;

            case 429:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "429 rate limited";
                break;

            case 451:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "451 legal block possible";
                break;

            case >= 400:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "unexpected status";
                break;

            default:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "unexpected response";
                break;
        }
    }

    private static void ClassifyGemini(UnblockServiceResult result, UnblockCheckEvidence evidence, string body)
    {
        var lockMarker = FindMarker(body, "not available in your country", "isn't available in your country");
        if (lockMarker != null)
        {
            result.Status = EUnblockStatus.Lock;
            evidence.MatchedMarker = lockMarker;
            evidence.Reason = "region restriction page";
            return;
        }

        if (evidence.FinalUrl != null && evidence.FinalUrl.Contains("google.com/sorry", StringComparison.OrdinalIgnoreCase))
        {
            result.Status = EUnblockStatus.Risk;
            evidence.MatchedMarker = "sorry redirect";
            evidence.Reason = "redirected to google sorry page";
            return;
        }

        if (evidence.FinalUrl != null && evidence.FinalUrl.Contains("accounts.google.com", StringComparison.OrdinalIgnoreCase))
        {
            result.Status = EUnblockStatus.Ok;
            evidence.MatchedMarker = "login wall";
            evidence.Reason = "redirected to login page";
            return;
        }

        switch (evidence.HttpStatusCode)
        {
            case 200:
                result.Status = EUnblockStatus.Ok;
                evidence.Reason = "reachable";
                break;

            case 403:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "403 access denied";
                break;

            case 429:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "429 rate limited";
                break;

            case 451:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "451 legal block possible";
                break;

            default:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "unexpected status";
                break;
        }
    }

    private static void ClassifyChatGpt(UnblockServiceResult result, UnblockCheckEvidence evidence, string body, HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("cf-mitigated", out var mitigatedValues)
            && mitigatedValues.Any(value => value.Contains("challenge", StringComparison.OrdinalIgnoreCase)))
        {
            result.Status = EUnblockStatus.Risk;
            evidence.MatchedMarker = "cf-mitigated: challenge";
            evidence.Reason = "cloudflare challenge header";
            return;
        }

        var challengeMarker = FindMarker(body, "Just a moment", "Verify you are human", "cf-challenge", "unusual activity", "unusual traffic");
        if (challengeMarker != null)
        {
            result.Status = EUnblockStatus.Risk;
            evidence.MatchedMarker = challengeMarker;
            evidence.Reason = "challenge marker in page";
            return;
        }

        var lockMarker = FindMarker(body, "not available in your country", "unsupported region", "not available in your region");
        if (lockMarker != null)
        {
            result.Status = EUnblockStatus.Lock;
            evidence.MatchedMarker = lockMarker;
            evidence.Reason = "region restriction page";
            return;
        }

        switch (evidence.HttpStatusCode)
        {
            case 200:
                result.Status = EUnblockStatus.Ok;
                evidence.Reason = "reachable";
                break;

            case 403:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "403 access denied";
                break;

            case 429:
                result.Status = EUnblockStatus.Risk;
                evidence.Reason = "429 rate limited";
                break;

            case 451:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "451 legal block possible";
                break;

            default:
                result.Status = EUnblockStatus.Unknown;
                evidence.Reason = "unexpected status";
                break;
        }
    }
}
