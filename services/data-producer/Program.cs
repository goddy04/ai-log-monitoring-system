namespace data_producer
{
    public class Program
    {
        public static void Main()
        {
            LogSimulator.Generate(10);

            Preprocessor.Clean();

            FeatureExtractor.Extract();
        }
    }
}