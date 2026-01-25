using System.Collections.Concurrent;
using System.Threading;
using data_producer;

public static class RealtimePipeline
{
    public static void Start()
    {

        var cts = new CancellationTokenSource();

        var rawLogs = new BlockingCollection<string>(boundedCapacity: 1000);
        var cleanedLogs = new BlockingCollection<string>(boundedCapacity: 1000);
        var features = new BlockingCollection<FeatureVector>(boundedCapacity: 1000);

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            rawLogs.CompleteAdding();
            cleanedLogs.CompleteAdding();
            features.CompleteAdding();
        };

        Task.Run(() => LogSimulator.GenerateRealtime(rawLogs, cts.Token));
        Task.Run(() => Preprocessor.RunRealtime(rawLogs, cleanedLogs, cts.Token));
        Task.Run(() => FeatureExtractor.RunRealtime(cleanedLogs, features, cts.Token));
        Task.Run(() => PythonBridge.SendFeatures(features, cts.Token));
        
        Console.WriteLine("Real-time pipeline running. Press Ctrl+C to stop.");
        Thread.Sleep(Timeout.Infinite);
    }
}