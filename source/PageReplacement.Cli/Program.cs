using PageReplacement.Core;
using PageReplacement.Cli;

try
{
    if (args.Length == 0 || args[0] is "-h" or "--help")
    {
        Console.WriteLine("Commands:\n  simulate INPUT [--output DIR] [--algorithm ALL|FIFO|LRU|OPT|Second-Chance] [--no-trace] [--curve 1..32]\n  evidence [PROJECT_ROOT]\n  benchmark [OUTPUT_DIR]");
        return 0;
    }
    if (args[0] == "benchmark") { Experiments.Benchmark(args.Length > 1 ? args[1] : "output"); return 0; }
    if (args[0] == "evidence") { Experiments.Evidence(args.Length > 1 ? args[1] : "."); return 0; }
    if (args[0] != "simulate" || args.Length < 2) throw new ArgumentException("Cần lệnh simulate và đường dẫn CSV. Dùng --help.");
    string output = "output/run", algorithm = "ALL"; bool trace = true; int curveMax = 0;
    for (int i = 2; i < args.Length; i++)
    {
        string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Thiếu giá trị tùy chọn.");
        switch (args[i])
        {
            case "--output": output = Value(); break;
            case "--algorithm": algorithm = Value(); break;
            case "--curve": curveMax = CsvFiles.ParseInteger(Value(), "Frame tối đa"); if (curveMax is < 1 or > 32) throw new ArgumentException("Curve phải từ 1 đến 32."); break;
            case "--no-trace": trace = false; break;
            default: throw new ArgumentException("Tùy chọn không hợp lệ: " + args[i]);
        }
    }
    var input = CsvFiles.ReadInput(args[1]);
    var policies = algorithm.Equals("ALL", StringComparison.OrdinalIgnoreCase) ? Policies.All : new[] { Policies.Parse(algorithm) };
    var results = policies.Select(p => Simulator.Run(input.Pages, input.Frames, p, trace)).ToArray();
    CsvFiles.WriteSummary(Path.Combine(output, "summary.csv"), results);
    CsvFiles.WriteInput(Path.Combine(output, "input.csv"), input);
    foreach (var r in results)
    {
        if (trace) CsvFiles.WriteTrace(Path.Combine(output, r.Policy.Label() + "_trace.csv"), r);
        Console.WriteLine($"{r.Policy.Label(),-14} faults={r.Faults,6} hits={r.Hits,6} rate={r.FaultRate:P2}");
    }
    if (curveMax > 0) CsvFiles.WriteCurve(Path.Combine(output, "curve.csv"), Simulator.Curve(input.Pages, curveMax));
    return 0;
}
catch (Exception ex) when (ex is ArgumentException or FormatException or IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine("Lỗi: " + ex.Message); return 2;
}
