using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace NetworkMonitor;

public sealed class SpeedTester : IDisposable
{
    private static readonly Uri TestFileUri = new("https://speed.hetzner.de/1MB.bin");
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private bool _disposed;

    public async Task<SpeedTestResult> RunAsync(CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        var latency = await MeasureLatencyAsync(cancellationToken);
        var (downloadMbps, error) = await MeasureDownloadAsync(cancellationToken);

        return new SpeedTestResult(timestamp, downloadMbps, latency, error);
    }

    private async Task<long?> MeasureLatencyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("1.1.1.1", 5000);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<(double downloadMbps, string? error)> MeasureDownloadAsync(CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            using var response = await _httpClient.GetAsync(TestFileUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var buffer = new byte[81920];
            long bytesRead = 0;
            int read;
            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                bytesRead += read;
            }

            stopwatch.Stop();
            var seconds = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001);
            var megabits = bytesRead * 8 / 1_000_000d;
            return (megabits / seconds, null);
        }
        catch (Exception ex)
        {
            return (0, ex.Message);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
