namespace NetworkMonitor;

public sealed record SpeedTestResult(DateTime TimestampUtc, double DownloadMbps, long? LatencyMs, string? ErrorMessage);
