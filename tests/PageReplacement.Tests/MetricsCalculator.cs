public class MetricsCalculator
{
    public void CalculateMetrics(System.Collections.Generic.List<string> results)
    {
        int total = results.Count;
        int faults = 0;
        int hits = 0;

        foreach (var r in results) {
            if (r == "FAULT") faults++;
            else hits++;
        }

        double faultRate = ((double)faults / total) * 100;
        double hitRatio = ((double)hits / total) * 100;

        System.Console.WriteLine($"Fault Count: {faults}");
        System.Console.WriteLine($"Fault Rate: {faultRate:F2}%");
        System.Console.WriteLine($"Hit Ratio: {hitRatio:F2}%");
    }
}
