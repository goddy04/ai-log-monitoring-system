using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

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
