using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PageReplacement.Core;

namespace PageReplacement.Cli;

public static class Experiments
{
    // Fixed portable generator, independent of future System.Random implementation changes.
    private sealed class Generator(uint seed)
    {
        private uint state = seed;
        public int Next(int bound) { state = unchecked(1664525u * state + 1013904223u); return (int)(((ulong)state * (uint)bound) >> 32); }
    }
    public static int[] Workload(int n, string kind)
    {
        var rng = new Generator(13); var pages = new int[n];
        for (int i = 0; i < n; i++)
            pages[i] = kind switch
            {
                "uniform" => rng.Next(64),
                "locality" => rng.Next(10) < 9 ? ((i / 1000) % 4) * 16 + rng.Next(12) : rng.Next(64),
                _ => i % 64
            };
        return pages;
    }
    public static void Evidence(string root)
    {
        var cases = new (string Name, Policy? Policy, int Expected, string Oracle)[]
        {
            ("textbook",Policy.FIFO,15,"ch10 slide 35"),("textbook",Policy.LRU,12,"ch10 slide 38"),("textbook",Policy.OPT,9,"ch10 slide 37"),
            ("belady_3",Policy.FIFO,9,"manual FIFO queue"),("belady_4",Policy.FIFO,10,"manual FIFO queue"),
            ("repeat",null,1,"manual first fault"),("single_frame",null,4,"manual adjacent runs"),
            ("sc_clock",null,5,"manual trace"),("opt_tie",Policy.OPT,4,"manual unique pages")
        };
        var rows = new List<object?[]>();
        foreach (var c in cases)
        {
            var input = CsvFiles.ReadInput(Path.Combine(root, "input", c.Name + ".csv"));
            foreach (var policy in c.Policy.HasValue ? new[] { c.Policy.Value } : Policies.All)
            {
                var r = Simulator.Run(input.Pages, input.Frames, policy);
                rows.Add([c.Name, policy.Label(), input.Frames, c.Expected, r.Faults, r.Faults == c.Expected ? "PASS" : "FAIL", c.Oracle]);
            }
        }
        CsvFiles.WriteRows(Path.Combine(root, "output", "expected_actual.csv"),
            ["case", "algorithm", "frames", "expected_faults", "actual_faults", "status", "oracle"], rows);
        foreach (string name in new[] { "textbook", "belady_3", "belady_4", "sc_clock" })
        {
            var input = CsvFiles.ReadInput(Path.Combine(root, "input", name + ".csv"));
            var results = Policies.All.Select(p => Simulator.Run(input.Pages, input.Frames, p)).ToArray();
            string output = Path.Combine(root, "output", name);
            foreach (var r in results) CsvFiles.WriteTrace(Path.Combine(output, r.Policy.Label() + "_trace.csv"), r);
            CsvFiles.WriteSummary(Path.Combine(output, "summary.csv"), results);
            CsvFiles.WriteCurve(Path.Combine(output, "curve.csv"), Simulator.Curve(input.Pages, 10));
        }
        if (rows.Any(r => (string?)r[5] == "FAIL")) throw new InvalidOperationException("Evidence did not match expected faults.");
        Console.WriteLine("Generated expected/actual and trace CSV from C#.");
    }

    public static void Benchmark(string output)
    {
        const int repeats = 5;
        var rows = new List<object?[]>();
        foreach (string kind in new[] { "uniform", "locality", "scan" })
        foreach (int n in new[] { 1000, 10000, 100000 })
        {
            int[] refs = Workload(n, kind);
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(",", refs)))).ToLowerInvariant();
            foreach (var policy in Policies.All)
            {
                for (int i = 0; i < 3; i++) Simulator.Run(refs, 16, policy, false);
                var ms = new double[repeats]; SimulationResult? r = null;
                for (int i = 0; i < repeats; i++)
                {
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                    long start = Stopwatch.GetTimestamp(); r = Simulator.Run(refs, 16, policy, false);
                    ms[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                }
                long before = GC.GetAllocatedBytesForCurrentThread(); Simulator.Run(refs, 16, policy, false);
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Array.Sort(ms);
                rows.Add([kind,n,16,policy.Label(),13,hash,repeats,ms[repeats/2],ms[0],ms[^1],allocated/1024d,r!.Faults,r.FaultRate]);
                Console.WriteLine($"{kind} {n} {policy.Label()} {ms[repeats / 2]:F3} ms");
            }
        }
        CsvFiles.WriteRows(Path.Combine(output, "performance.csv"),
            ["workload","n","frames","algorithm","seed","input_sha256","repeats","median_ms","min_ms","max_ms","allocated_KiB","faults","fault_rate"],rows);
        var memory = new List<object?[]>(); int[] sample = Workload(10000,"uniform");
        foreach (var policy in Policies.All)
        foreach (bool trace in new[] { false, true })
        {
            Simulator.Run(sample,16,policy,trace);
            long before=GC.GetAllocatedBytesForCurrentThread();var r=Simulator.Run(sample,16,policy,trace);
            memory.Add([policy.Label(),10000,16,trace,(GC.GetAllocatedBytesForCurrentThread()-before)/1024d,r.Steps.Count]);
        }
        CsvFiles.WriteRows(Path.Combine(output,"trace_memory.csv"),["algorithm","n","frames","trace","allocated_KiB","saved_steps"],memory);
        File.WriteAllText(Path.Combine(output,"benchmark_environment.json"),JsonSerializer.Serialize(new
        {
            utc=DateTimeOffset.UtcNow, runtime=RuntimeInformation.FrameworkDescription, os=RuntimeInformation.OSDescription,
            architecture=RuntimeInformation.ProcessArchitecture.ToString(),logical_cpus=Environment.ProcessorCount,
            clock_frequency=Stopwatch.Frequency,repeats,warmups=3,
            tieredCompilation=Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "runtime default",
            method="Release; trace=False; validation and OPT preprocessing included; input/CSV/GUI excluded; 3 warmups, 5 timing trials with GC before trial; allocation measured separately using GetAllocatedBytesForCurrentThread, cumulative bytes NOT peak/RSS; LCG32 seed13; tiered compilation setting recorded separately."
        },new JsonSerializerOptions { WriteIndented=true }));
    }
}
