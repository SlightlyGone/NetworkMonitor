namespace NetworkMonitor;

internal static class Program
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    private static async Task Main()
    {
        Console.WriteLine("Network monitor starting. Press Ctrl+C to exit.");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cts.Cancel();
        };

        var resultsPath = Path.Combine(AppContext.BaseDirectory, "speed_results.csv");

        using var tester = new SpeedTester();
        var writer = new CsvResultWriter(resultsPath);
        var service = new MonitorService(tester, writer, Interval);

        try
        {
            await service.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Shutdown requested. Exiting.");
        }
    }
}
