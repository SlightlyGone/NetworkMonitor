using System.Globalization;
using System.Text;

namespace NetworkMonitor;

public sealed class CsvResultWriter
{
    private const string Header = "timestamp_utc,download_mbps,latency_ms,error_message";
    private readonly string _filePath;

    public CsvResultWriter(string filePath)
    {
        _filePath = filePath;
    }

    public string FilePath => _filePath;

    public async Task AppendAsync(SpeedTestResult result, CancellationToken cancellationToken)
    {
        var newFile = !File.Exists(_filePath);
        await using var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);

        if (newFile)
        {
            await writer.WriteLineAsync(Header.AsMemory(), cancellationToken);
        }

        var line = string.Join(",",
            result.TimestampUtc.ToString("O", CultureInfo.InvariantCulture),
            result.DownloadMbps.ToString("F2", CultureInfo.InvariantCulture),
            result.LatencyMs?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            EscapeForCsv(result.ErrorMessage));

        await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
    }

    private static string EscapeForCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var escaped = value.Replace('"', '\'');
        if (escaped.Contains(',') || escaped.Contains('"'))
        {
            return $"\"{escaped}\"";
        }

        return escaped;
    }
}
