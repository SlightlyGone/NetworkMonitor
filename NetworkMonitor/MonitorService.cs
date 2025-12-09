namespace NetworkMonitor;

public sealed class MonitorService
{
    private readonly SpeedTester _tester;
    private readonly CsvResultWriter _writer;
    private readonly TimeSpan _interval;

    public MonitorService(SpeedTester tester, CsvResultWriter writer, TimeSpan interval)
    {
        _tester = tester;
        _writer = writer;
        _interval = interval;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await RunOnceAsync(cancellationToken);

        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await RunOnceAsync(cancellationToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var result = await _tester.RunAsync(cancellationToken);
        await _writer.AppendAsync(result, cancellationToken);

        var latencyText = result.LatencyMs?.ToString() ?? "n/a";
        var statusText = string.IsNullOrWhiteSpace(result.ErrorMessage)
            ? "completed"
            : $"error: {result.ErrorMessage}";

        Console.WriteLine(
            $"[{result.TimestampUtc:O}] Download {result.DownloadMbps:F2} Mbps, latency {latencyText} ms ({statusText})." );
    }
}
