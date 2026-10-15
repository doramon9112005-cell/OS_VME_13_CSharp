namespace PageReplacement.Core;

public enum Policy { FIFO, LRU, OPT, SecondChance }

public static class Policies
{
    public static readonly Policy[] All = Enum.GetValues<Policy>();
    public static string Label(this Policy policy) => policy == Policy.SecondChance ? "Second-Chance" : policy.ToString();
    public static Policy Parse(string text) => text.Trim().ToUpperInvariant() switch
    {
        "FIFO" => Policy.FIFO, "LRU" => Policy.LRU, "OPT" => Policy.OPT,
        "SECOND-CHANCE" or "SECONDCHANCE" or "CLOCK" => Policy.SecondChance,
        _ => throw new ArgumentException("Thuật toán phải là FIFO, LRU, OPT hoặc Second-Chance.")
    };
}

public sealed record TraceStep(int Index, int Page, bool Hit, int Slot, int? Victim,
    IReadOnlyList<int?> Frames, IReadOnlyList<int> Bits, int? Hand,
    IReadOnlyList<int> ClearedSlots, IReadOnlyList<int> Order, int Faults);

public sealed record SimulationResult(Policy Policy, int Capacity, int Count, int Faults,
    int Replacements, IReadOnlyList<TraceStep> Steps)
{
    public int Hits => Count - Faults;
    public double FaultRate => Count == 0 ? 0 : (double)Faults / Count;
}

public sealed record CurvePoint(int Frames, IReadOnlyList<SimulationResult> Results);

public static class Simulator
{
    public const int MaxReferences = 200_000;
    public const int MaxFrames = 256;
    public const int MaxTraceCells = 1_000_000;

    public static void Validate(IReadOnlyList<int> pages, int capacity)
    {
        ArgumentNullException.ThrowIfNull(pages);
        if (capacity is < 1 or > MaxFrames) throw new ArgumentException($"Số frame phải từ 1 đến {MaxFrames}.");
        if (pages.Count is < 1 or > MaxReferences) throw new ArgumentException($"Cần từ 1 đến {MaxReferences} tham chiếu.");
        if (pages.Any(p => p < 0)) throw new ArgumentException("Page phải là số nguyên không âm, tối đa Int32.MaxValue.");
    }

    /// <summary>Cold start. All snapshots are after a reference. OPT tie: lowest slot.
    /// Clock: hit sets R without moving hand; insertion sets R and advances hand.</summary>
    public static SimulationResult Run(IReadOnlyList<int> pages, int capacity, Policy policy, bool trace = true)
    {
        Validate(pages, capacity);
        if (!Enum.IsDefined(policy)) throw new ArgumentException("Thuật toán không hợp lệ.");
        if (trace && (long)pages.Count * capacity > MaxTraceCells)
            throw new ArgumentException("Trace quá lớn. Giảm n/frame hoặc dùng --no-trace.");
        int?[] frames = new int?[capacity];
        int[] bits = new int[capacity];
        var resident = new Dictionary<int, int>();
        var order = new LinkedList<int>();
        var nodes = new Dictionary<int, LinkedListNode<int>>();
        var future = new Dictionary<int, Queue<int>>();
        if (policy == Policy.OPT)
            for (int i = 0; i < pages.Count; i++)
            {
                if (!future.TryGetValue(pages[i], out var positions)) future[pages[i]] = positions = new Queue<int>();
                positions.Enqueue(i);
            }
        int hand = 0, faults = 0, replacements = 0;
        var steps = new List<TraceStep>(trace ? pages.Count : 0);
        for (int i = 0; i < pages.Count; i++)
        {
            int page = pages[i];
            if (policy == Policy.OPT) future[page].Dequeue();
            bool hit = resident.TryGetValue(page, out int slot);
            int? victim = null;
            List<int>? cleared = trace ? new List<int>() : null;
            if (hit)
            {
                if (policy == Policy.LRU)
                {
                    order.Remove(nodes[page]); order.AddLast(nodes[page]);
                }
                else if (policy == Policy.SecondChance) bits[slot] = 1;
            }
            else
            {
                faults++;
                if (resident.Count < capacity) slot = resident.Count;
                else if (policy is Policy.FIFO or Policy.LRU)
                {
                    victim = order.First!.Value; slot = resident[victim.Value];
                    order.RemoveFirst(); nodes.Remove(victim.Value);
                }
                else if (policy == Policy.OPT)
                {
                    int farthest = -1; slot = 0;
                    for (int candidate = 0; candidate < capacity; candidate++)
                    {
                        var positions = future[frames[candidate]!.Value];
                                    int next = positions[candidate].Count == 0 ? int.MaxValue : positions[candidate].Peek();
                        if (next > farthest) { farthest = next; slot = candidate; }
                                    if (next == int.MaxValue) break;
                    }
                    victim = frames[slot];
                }
                else
                {
                    while (bits[hand] == 1)
                    {
                        bits[hand] = 0; cleared?.Add(hand); hand = (hand + 1) % capacity;
                    }
                    slot = hand; victim = frames[slot];
                }
                if (victim.HasValue) { resident.Remove(victim.Value); replacements++; }
                frames[slot] = page; resident[page] = slot;
                if (policy is Policy.FIFO or Policy.LRU) nodes[page] = order.AddLast(page);
                else if (policy == Policy.SecondChance)
                {
                    bits[slot] = 1; hand = (slot + 1) % capacity;
                }
            }
            if (trace)
                steps.Add(new TraceStep(i + 1, page, hit, slot, victim,
                    Array.AsReadOnly((int?[])frames.Clone()),
                    policy == Policy.SecondChance ? Array.AsReadOnly((int[])bits.Clone()) : Array.Empty<int>(),
                    policy == Policy.SecondChance ? hand : null,
                    cleared!.AsReadOnly(), Array.AsReadOnly(order.ToArray()), faults));
        }
        return new SimulationResult(policy, capacity, pages.Count, faults, replacements, steps.AsReadOnly());
    }

    public static IReadOnlyList<SimulationResult> Compare(IReadOnlyList<int> pages, int capacity) =>
        Policies.All.Select(a => Run(pages, capacity, a, false)).ToArray();

    public static IReadOnlyList<CurvePoint> Curve(IReadOnlyList<int> pages, int maxFrames)
    {
        if (maxFrames is < 1 or > 32) throw new ArgumentException("Frame tối đa của đường cong phải từ 1 đến 32.");
        return Enumerable.Range(1, maxFrames).Select(f => new CurvePoint(f, Compare(pages, f))).ToArray();
    }
}
