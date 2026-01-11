using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace data_producer;

public static class Preprocessor
{
    private static readonly string RawLogPath =
        Path.Combine("..", "..", "data", "raw_logs", "logs.txt");

    private static readonly string CleanLogPath =
        Path.Combine("..", "..", "data", "processed", "clean_logs.json");

    public static void Clean()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CleanLogPath)!);

        var structuredLogs = new List<Dictionary<string, object>>();

        foreach (var line in File.ReadLines(RawLogPath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var log = ParseLogLine(line);
                structuredLogs.Add(log);
            }
            catch
            {
                // Ignore malformed lines (or log them separately)
            }
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(structuredLogs, options);
        File.WriteAllText(CleanLogPath, json);
    }

    private static Dictionary<string, object> ParseLogLine(string line)
    {
        // Example:
        // [DB] INFO 2026-01-11T07:09:47 Query executed 7ms

        var parts = line.Split(' ');

        string service = parts[0].Trim('[', ']');
        string level = parts[1];
        string timestamp = parts[2];

        // Last part = "7ms"
        string responsePart = parts[^1];
        int responseTime = int.Parse(responsePart.Replace("ms", ""));

        // Message = everything between timestamp and response time
        var messageParts = parts[3..^1];
        string message = string.Join(" ", messageParts);

        return new Dictionary<string, object>
        {
            { "service", service },
            { "level", level },
            { "timestamp", timestamp },
            { "response_time", responseTime }
        };
    }
}
