using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;

namespace data_producer;
public static class FeatureExtractor
{
    private static readonly string CleanLogPath =
        Path.Combine("..", "..", "data", "processed", "clean_logs.json");

    private static readonly string FeaturePath =
        Path.Combine("..","..", "data", "features", "features.json");

    public static void Extract()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FeaturePath)!);

        string json = File.ReadAllText(CleanLogPath);

        var logs = JsonSerializer.Deserialize<List<LogEntry>>(json);

        if (logs == null || logs.Count == 0)
            return;

        var features = logs
            .GroupBy(l => l.Service)
            .Select(group => new FeatureVector
            {
                Service = group.Key,
                LogCount = group.Count(),
                ErrorCount = group.Count(l => l.Level == "ERROR"),
                WarningCount = group.Count(l => l.Level == "WARNING"),
                AvgResponseTime = (int)group.Average(l => l.ResponseTime)
            })
            .ToList();

        var options = new JsonSerializerOptions { WriteIndented = true };
        string output = JsonSerializer.Serialize(features, options);
        File.WriteAllText(FeaturePath, output);
    }

    public static void RunRealtime(
    BlockingCollection<string> input,
    BlockingCollection<FeatureVector> output,
    CancellationToken token)
    {
        // Running state per service
        var stats = new Dictionary<string, ServiceStats>();

        foreach (var json in input.GetConsumingEnumerable(token))
        {
            var log = JsonSerializer.Deserialize<LogEntry>(json);
            if (log == null) continue;

            if (!stats.ContainsKey(log.Service))
                stats[log.Service] = new ServiceStats();

            var s = stats[log.Service];
            s.LogCount++;
            s.TotalResponseTime += log.ResponseTime;

            if (log.Level == "ERROR") s.ErrorCount++;
            if (log.Level == "WARNING") s.WarningCount++;

            // Emit a feature vector every N logs (window)
            if (s.LogCount % 10 == 0)   // window size = 10
            {
                var feature = new FeatureVector
                {
                    Service = log.Service,
                    LogCount = s.LogCount,
                    ErrorCount = s.ErrorCount,
                    WarningCount = s.WarningCount,
                    AvgResponseTime = s.TotalResponseTime / s.LogCount
                };

                output.Add(feature);
            }
        }
    }

}

public class ServiceStats
{
    public int LogCount = 0;
    public int ErrorCount = 0;
    public int WarningCount = 0;
    public int TotalResponseTime = 0;
}



public class LogEntry
{
    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("response_time")]
    public int ResponseTime { get; set; }
}

public class FeatureVector
{
    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonPropertyName("log_count")]
    public int LogCount { get; set; }

    [JsonPropertyName("error_count")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("warning_count")]
    public int WarningCount { get; set; }

    [JsonPropertyName("avg_response_time")]
    public int AvgResponseTime { get; set; }
}


