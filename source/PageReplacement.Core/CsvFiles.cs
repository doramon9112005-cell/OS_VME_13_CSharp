using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PageReplacement.Core;

public sealed record InputCase(int[] Pages, int Frames);

public static class CsvFiles
{
    private static readonly UTF8Encoding Utf8Strict = new(false, true);
    public static int ParseInteger(string? value, string label)
    {
        string s = value?.Trim() ?? "";
        if (s.Length == 0 || s.Any(c => c is < '0' or > '9') ||
            !int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out int result))
            throw new FormatException($"{label}: cần số nguyên từ 0 đến 2147483647.");
        return result;
    }

    public static int[] ParseReferences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new FormatException("Chuỗi tham chiếu không được rỗng.");
        if (text.Length > 2_400_000) throw new FormatException("Chuỗi nhập quá dài.");
        var groups = text.Trim().Split(',');
        if (groups.Any(string.IsNullOrWhiteSpace)) throw new FormatException("Có phần tử rỗng giữa các dấu phẩy.");
        return groups.SelectMany(g => Regex.Split(g.Trim(), @"\s+")).Select(s => ParseInteger(s, "Page")).ToArray();
    }

    // CSV record parser: quoted fields and escaped quotes supported; multiline fields
    // are deliberately outside the numeric input schema.
    public static string[] ParseRecord(string line)
    {
        var fields = new List<string>();
        int i = 0;
        while (true)
        {
            var field = new StringBuilder();
            if (i < line.Length && line[i] == '"')
            {
                i++; bool closed = false;
                while (i < line.Length)
                {
                    if (line[i] != '"') { field.Append(line[i++]); continue; }
                    i++;
                    if (i < line.Length && line[i] == '"') { field.Append('"'); i++; }
                    else { closed = true; break; }
                }
                if (!closed || (i < line.Length && line[i] != ',')) throw new FormatException("CSV có dấu nháy không hợp lệ.");
            }
            else
            {
                while (i < line.Length && line[i] != ',')
                {
                    if (line[i] == '"') throw new FormatException("CSV có dấu nháy không hợp lệ.");
                    field.Append(line[i++]);
                }
            }
            fields.Add(field.ToString());
            if (i == line.Length) break;
            i++;
        }
        return fields.ToArray();
    }

    public static InputCase ReadInput(string path)
    {
        using var stream = new StreamReader(path, Utf8Strict, detectEncodingFromByteOrderMarks: false);
        string? header = stream.ReadLine();
        header = header?.TrimStart('\uFEFF');
        if (header != "step,page,frames") throw new FormatException("Header CSV phải đúng: step,page,frames");
        var pages = new List<int>(); int? capacity = null; int lineNumber = 1;
        while (stream.ReadLine() is string line)
        {
            lineNumber++;
            if (line.Length > 256) throw new FormatException($"Dòng {lineNumber} quá dài.");
            string[] row = ParseRecord(line);
            if (row.Length != 3) throw new FormatException($"Dòng {lineNumber}: cần đúng 3 cột.");
            int step = ParseInteger(row[0], "Step"), page = ParseInteger(row[1], "Page"), frames = ParseInteger(row[2], "Frames");
            if (step != pages.Count + 1) throw new FormatException($"Dòng {lineNumber}: step phải liên tục từ 1.");
            capacity ??= frames;
            if (frames != capacity) throw new FormatException("Frames phải giống nhau ở mọi dòng.");
            pages.Add(page);
            if (pages.Count > Simulator.MaxReferences) throw new FormatException("Tối đa 200000 tham chiếu.");
        }
        if (capacity is null) throw new FormatException("CSV không có dữ liệu.");
        int[] refs = pages.ToArray(); Simulator.Validate(refs, capacity.Value);
        return new InputCase(refs, capacity.Value);
    }

    public static void WriteRows(string path, IEnumerable<string> header, IEnumerable<IEnumerable<object?>> rows)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        static string Escape(object? value)
        {
            string s = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            return s.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }
        writer.WriteLine(string.Join(",", header.Select(Escape)));
        foreach (var row in rows) writer.WriteLine(string.Join(",", row.Select(Escape)));
    }

    public static void WriteInput(string path, InputCase input)
    {
        Simulator.Validate(input.Pages, input.Frames);
        WriteRows(path, ["step", "page", "frames"], input.Pages.Select((p, i) => new object?[] { i + 1, p, input.Frames }));
    }

    public static void WriteTrace(string path, SimulationResult result)
    {
        if (result.Steps.Count == 0) throw new ArgumentException("Kết quả không chứa trace.");
        WriteRows(path, ["algorithm", "capacity", "step", "page", "status", "slot", "victim", "frames_json",
            "reference_bits_json", "hand_after", "cleared_slots_json", "order_pages_json", "faults_total"],
            result.Steps.Select(s => new object?[] { result.Policy.Label(), result.Capacity, s.Index, s.Page,
                s.Hit ? "HIT" : "FAULT", s.Slot, s.Victim, JsonSerializer.Serialize(s.Frames),
                JsonSerializer.Serialize(s.Bits), s.Hand, JsonSerializer.Serialize(s.ClearedSlots),
                JsonSerializer.Serialize(s.Order), s.Faults }));
    }

    public static void WriteSummary(string path, IEnumerable<SimulationResult> results) =>
        WriteRows(path, ["algorithm", "frames", "references", "hits", "faults", "fault_rate", "replacements"],
            results.Select(r => new object?[] { r.Policy.Label(), r.Capacity, r.Count, r.Hits, r.Faults, r.FaultRate, r.Replacements }));

    public static void WriteCurve(string path, IEnumerable<CurvePoint> curve) =>
        WriteRows(path, new[] { "frames" }.Concat(Policies.All.Select(p => p.Label())),
            curve.Select(c => new object?[] { c.Frames }.Concat(c.Results.Select(r => (object?)r.Faults))));
}
