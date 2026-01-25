namespace data_producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run train | realtime");
                return;
            }

            var mode = args[0].ToLower();

            if (mode == "train")
            {
                RunTrainingMode();
            }
            else if (mode == "realtime")
            {
                RunRealtimeMode();
            }
            else
            {
                Console.WriteLine("Invalid mode. Use train or realtime.");
            }
            
        }
        public static void RunTrainingMode()
        {
            LogSimulator.Generate(100);

            Preprocessor.Clean();

            FeatureExtractor.Extract();
        }

        public static void RunRealtimeMode()
        {
            Console.WriteLine("Running .NET in REALTIME mode...");
            RealtimePipeline.Start();
        }
    }
}