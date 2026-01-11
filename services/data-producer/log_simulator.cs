using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        string line = $"[{service}] {level} {timestamp} {latency}ms";
        writer.WriteLine(line);
    }

    private static int RandomLatency(int min, int max)
    {
        return Rand.Next(min, max);
    }
}
