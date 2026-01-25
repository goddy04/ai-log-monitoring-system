using System.Collections.Concurrent;
using System.Text.Json;
using data_producer;

public static class PythonBridge
{
    public static void SendFeatures(
        BlockingCollection<FeatureVector> input,
        CancellationToken token)
    {
        foreach (var feature in input.GetConsumingEnumerable(token))
        {
            string json = JsonSerializer.Serialize(feature);
            SendToPython(json);
        }
    }

    private static void SendToPython(string json)
    {
        // HTTP POST / socket / pipe
        Console.WriteLine($"Sent to Python: {json}");
    }
}
