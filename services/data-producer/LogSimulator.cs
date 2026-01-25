using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace data_producer;
public static class LogSimulator
{
    private static readonly string LogPath = Path.Combine("..", "..", "data", "raw_logs", "logs.txt");
    private static readonly Random Rand = new Random();

    private static readonly string[] Services = { "WEB", "API", "AUTH", "DB" };

    public static void Generate(int cycles = 50)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);

        using var writer = new StreamWriter(LogPath, append: true);

        for(int i = 0; i< cycles; i++)
        {
            if (Rand.NextDouble() < 0.8)
                GenerateNormalLogs(writer);
            else
                GenerateRandomFailure(writer);

            writer.Flush();

            writer.WriteLine();
            Thread.Sleep(500);
        }
        // while (true)
        // {
        //     if (Rand.NextDouble() < 0.8)
        //         GenerateNormalLogs(writer);
        //     else
        //         GenerateRandomFailure(writer);

        //     writer.Flush();

        //     writer.WriteLine();
        //     Thread.Sleep(500);
        // }
    }

    // ---------------- NORMAL TRAFFIC ----------------
    private static void GenerateNormalLogs(StreamWriter writer)
    {
        
        int db = RandomLatency(5, 25);

        int auth = db + RandomLatency(10, 25);

        int api = auth + RandomLatency(20, 40);

        int web = api + RandomLatency(30, 60);

        WriteLog(writer, "DB", "INFO", "Query executed", db);
        WriteLog(writer, "AUTH", "INFO", "Token validated", auth);
        WriteLog(writer, "API", "INFO", "Data returned", api);
        WriteLog(writer, "WEB", "INFO", "Request success", web);

    }

    // ---------------- FAILURE CONTROLLER ----------------
    private static void GenerateRandomFailure(StreamWriter writer)
    {
        int failureType = Rand.Next(4); // 0 = DB, 1 = AUTH, 2 = API

        switch (failureType)
        {
            case 0:
                GenerateDbFailure(writer);
                break;
            case 1:
                GenerateAuthFailure(writer);
                break;
            case 2:
                GenerateApiFailure(writer);
                break;

            case 3:
                GenerateWebFailure(writer);
                break;
        }
    }

    // ---------------- DB FAILURE ----------------
    private static void GenerateDbFailure(StreamWriter writer)
    {
        int db1 = RandomLatency(40, 80);
        int db2 = RandomLatency(80, 140);
        int db3 = RandomLatency(140, 220);

        WriteLog(writer, "DB", "INFO", "Query executed", db1);
        WriteLog(writer, "DB", "WARNING", "Query execution slow", db2);
        WriteLog(writer, "DB", "WARNING", "High query latency detected", db3);

        int dbCrash = RandomLatency(400, 800);
        WriteLog(writer, "DB", "ERROR", "Connection timeout", dbCrash);

        int auth = dbCrash + RandomLatency(20, 100);
        int api = auth + RandomLatency(30, 120);
        int web = api + RandomLatency(40, 150);

        WriteLog(writer, "AUTH", "ERROR", "Token validation failed (DB error)", auth);
        WriteLog(writer, "API", "ERROR", "API call failed (Auth error)", api);
        WriteLog(writer, "WEB", "ERROR", "500 Internal Server Error", web);
    }


    // ---------------- AUTH FAILURE ----------------
    private static void GenerateAuthFailure(StreamWriter writer)
    {
        int auth1 = RandomLatency(40, 90);
        int auth2 = RandomLatency(90, 160);
        int authCrash = RandomLatency(200, 400);

        WriteLog(writer, "AUTH", "INFO", "Token validated", auth1);
        WriteLog(writer, "AUTH", "WARNING", "Token validation slow", auth2);
        WriteLog(writer, "AUTH", "ERROR", "Invalid token", authCrash);

        int api = authCrash + RandomLatency(20, 100);
        int web = api + RandomLatency(30, 120);

        WriteLog(writer, "API", "ERROR", "Authentication failed", api);
        WriteLog(writer, "WEB", "ERROR", "Access denied", web);
    }



    // ---------------- API FAILURE ----------------
    private static void GenerateApiFailure(StreamWriter writer)
    {
        int api1 = RandomLatency(90, 150);
        int api2 = RandomLatency(150, 250);
        int apiCrash = RandomLatency(300, 500);

        WriteLog(writer, "API", "INFO", "Service responding", api1);
        WriteLog(writer, "API", "WARNING", "Service response slow", api2);
        WriteLog(writer, "API", "ERROR", "Service unavailable", apiCrash);

        int web = apiCrash + RandomLatency(30, 120);
        WriteLog(writer, "WEB", "ERROR", "Request failed", web);
    }


    private static void GenerateWebFailure(StreamWriter writer)
    {
    int webCrash = RandomLatency(200, 500);
    WriteLog(writer, "WEB", "ERROR", "Frontend rendering failed", webCrash);
    }

    // ---------------- CORE LOGGER ----------------
    private static void WriteLog(StreamWriter writer, string service, string level, string message, int latency)
    {
        var line = FormatLog(service, level, message, latency);
        writer.WriteLine(line);
    }
    private static string FormatLog(string service, string level, string message, int latency)
    {
        return $"[{service}] {level} {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss} {latency}ms";
    }


    private static int RandomLatency(int min, int max)
    {
        return Rand.Next(min, max);
    }

    public static void GenerateRealtime(BlockingCollection<string> output, CancellationToken token)
    {
         while (!token.IsCancellationRequested)
        {
            var logs = Rand.NextDouble() < 0.8
                ? GenerateNormalLogsRealtime()
                : BuildRandomFailure();

            foreach (var log in logs)
                output.Add(log);

            Thread.Sleep(500);
        }
    }

    private static IEnumerable<string> GenerateNormalLogsRealtime()
    {
        return BuildNormalLogs();
    }

    private static List<string> BuildNormalLogs()
    {
        int db = RandomLatency(5, 25);
        int auth = db + RandomLatency(10, 25);
        int api = auth + RandomLatency(20, 40);
        int web = api + RandomLatency(30, 60);

        return new List<string>
        {
            FormatLog("DB", "INFO", "Query executed", db),
            FormatLog("AUTH", "INFO", "Token validated", auth),
            FormatLog("API", "INFO", "Data returned", api),
            FormatLog("WEB", "INFO", "Request success", web)
        };
    }

    private static List<string> BuildRandomFailure()
    {
        int failureType = Rand.Next(4);

        return failureType switch
        {
            0 => BuildDbFailure(),
            1 => BuildAuthFailure(),
            2 => BuildApiFailure(),
            3 => BuildWebFailure(),
            _ => new List<string>()
        };
    }

    private static List<string> BuildDbFailure()
    {
        var logs = new List<string>();

        int db1 = RandomLatency(40, 80);
        int db2 = RandomLatency(80, 140);
        int db3 = RandomLatency(140, 220);

        logs.Add(FormatLog("DB", "INFO", "Query executed", db1));
        logs.Add(FormatLog("DB", "WARNING", "Query execution slow", db2));
        logs.Add(FormatLog("DB", "WARNING", "High query latency detected", db3));

        int dbCrash = RandomLatency(400, 800);
        logs.Add(FormatLog("DB", "ERROR", "Connection timeout", dbCrash));

        int auth = dbCrash + RandomLatency(20, 100);
        int api = auth + RandomLatency(30, 120);
        int web = api + RandomLatency(40, 150);

        logs.Add(FormatLog("AUTH", "ERROR", "Token validation failed (DB error)", auth));
        logs.Add(FormatLog("API", "ERROR", "API call failed (Auth error)", api));
        logs.Add(FormatLog("WEB", "ERROR", "500 Internal Server Error", web));

        return logs;
    }

    private static List<string> BuildAuthFailure()
    {
        var logs = new List<string>();

        int auth1 = RandomLatency(40, 90);
        int auth2 = RandomLatency(90, 160);
        int authCrash = RandomLatency(200, 400);

        logs.Add(FormatLog("AUTH", "INFO", "Token validated", auth1));
        logs.Add(FormatLog("AUTH", "WARNING", "Token validation slow", auth2));
        logs.Add(FormatLog("AUTH", "ERROR", "Invalid token", authCrash));

        int api = authCrash + RandomLatency(20, 100);
        int web = api + RandomLatency(30, 120);

        logs.Add(FormatLog("API", "ERROR", "Authentication failed", api));
        logs.Add(FormatLog("WEB", "ERROR", "Access denied", web));

        return logs;
    }

    private static List<string> BuildApiFailure()
    {
        var logs = new List<string>();

        int api1 = RandomLatency(90, 150);
        int api2 = RandomLatency(150, 250);
        int apiCrash = RandomLatency(300, 500);

        logs.Add(FormatLog("API", "INFO", "Service responding", api1));
        logs.Add(FormatLog("API", "WARNING", "Service response slow", api2));
        logs.Add(FormatLog("API", "ERROR", "Service unavailable", apiCrash));

        int web = apiCrash + RandomLatency(30, 120);
        logs.Add(FormatLog("WEB", "ERROR", "Request failed", web));

        return logs;
    }

    private static List<string> BuildWebFailure()
    {
        var logs = new List<string>();

        int webCrash = RandomLatency(200, 500);
        logs.Add(FormatLog("WEB", "ERROR", "Frontend rendering failed", webCrash));

        return logs;
    }

}
