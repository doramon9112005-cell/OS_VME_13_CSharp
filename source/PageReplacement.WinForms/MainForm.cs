using PageReplacement.Core;

namespace PageReplacement.WinForms;

public sealed class MainForm : Form
{
    public const string Textbook = "7,0,1,2,0,3,0,4,2,3,0,3,0,3,2,1,2,0,1,7,0,1";
    public const string Belady = "1,2,3,4,1,2,5,1,2,3,4,5";
    private readonly TextBox references = new() { Text = Textbook, Dock = DockStyle.Fill };
    private readonly NumericUpDown frameCount = new() { Minimum = 1, Maximum = 32, Value = 3, Width = 65 };
    private readonly NumericUpDown maxFrames = new() { Minimum = 1, Maximum = 32, Value = 10, Width = 65 };
    private readonly NumericUpDown speed = new() { Minimum = 100, Maximum = 2000, Increment = 100, Value = 500, Width = 75 };
    private readonly ComboBox algorithm = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 165 };
    private readonly Label status = new() { Dock = DockStyle.Fill, Text = "Nhập chuỗi hoặc mở CSV, sau đó bấm Mô phỏng.", AutoEllipsis = true };
    private readonly Label metrics = new() { Dock = DockStyle.Fill, AutoEllipsis = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
    private readonly Label detail = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
    private readonly Label curveNote = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
    private readonly FlowLayoutPanel framePanel = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(4) };
    private readonly DataGridView traceGrid = Grid();
    private readonly DataGridView compareGrid = Grid();
    private readonly CurveView curveView = new();
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly System.Windows.Forms.Timer playTimer = new();
    private readonly List<Button> calculateButtons = new();
    private SimulationResult? result;
    private InputCase? resultInput;
    private IReadOnlyList<SimulationResult>? comparisons;
    private IReadOnlyList<CurvePoint>? curve;
    private InputCase? comparisonInput;
    private int current;
    private bool busy, selecting;

    public MainForm()
    {
        Text = "OS_VME_13 | Page Replacement Lab — C#";
        Size = new Size(1240, 860); MinimumSize = new Size(1060, 720);
        StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10); BackColor = Color.White;
        algorithm.Items.AddRange(Policies.All.Select(p => (object)p.Label()).ToArray()); algorithm.SelectedIndex = 0;
        var root = CreateTableLayout(1, 5); root.Padding = new Padding(16);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 98));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 6));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        Controls.Add(root);
        root.Controls.Add(new Label { Text = "PAGE REPLACEMENT LAB\nOS_VME_13 • FIFO / LRU / OPT / Second-Chance", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 15, FontStyle.Bold) }, 0, 0);
        var input = CreateTableLayout(1, 2); input.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); input.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        input.Controls.Add(references, 0, 0);
        var controls = Flow();
        controls.Controls.Add(Caption("Frames")); controls.Controls.Add(frameCount); controls.Controls.Add(algorithm);
        calculateButtons.Add(AddButton(controls, "Mô phỏng", async () => await RunSimulationAsync()));
        AddButton(controls, "Mở CSV", LoadInput); AddButton(controls, "Lưu input", SaveInput);
        AddButton(controls, "Mẫu slide", () => Preset(Textbook)); AddButton(controls, "Belady", () => Preset(Belady));
        input.Controls.Add(controls, 0, 1); root.Controls.Add(input, 0, 1); root.Controls.Add(tabs, 0, 2); root.Controls.Add(status, 0, 4);
        BuildTraceTab(); BuildCompareTab();
        playTimer.Tick += (_, _) => { if (result is null || current >= result.Count) playTimer.Stop(); else ShowStep(current + 1); };
        speed.ValueChanged += (_, _) => playTimer.Interval = (int)speed.Value;
        playTimer.Interval = (int)speed.Value;
    }

    private static TableLayoutPanel CreateTableLayout(int columns, int rows) => new() { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rows, Margin = Padding.Empty };
    private static FlowLayoutPanel Flow() => new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false, Padding = new Padding(0, 4, 0, 0) };
    private static Label Caption(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(4, 7, 4, 0) };
    private static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, AllowUserToResizeRows = false
    };
    private static Button AddButton(Control parent, string title, Action action)
    {
        var b = new Button { Text = title, AutoSize = true, Height = 30, Padding = new Padding(6, 0, 6, 0), Margin = new Padding(3) };
        b.Click += (_, _) => action(); parent.Controls.Add(b); return b;
    }
    private static void Columns(DataGridView grid, params (string Name, int Width)[] specs)
    {
        foreach (var (name, width) in specs)
        {
            int i = grid.Columns.Add(name, name); grid.Columns[i].Width = width; grid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    private void BuildTraceTab()
    {
        var tab = new TabPage("Truy vết từng bước") { Padding = new Padding(10) }; tabs.TabPages.Add(tab);
        var panel = CreateTableLayout(1, 5);
        foreach (int height in new[] { 34, 48, 145, 52 }) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); tab.Controls.Add(panel);
        panel.Controls.Add(metrics, 0, 0);
        var nav = Flow();
        AddButton(nav, "Đầu", () => ShowStep(0)); AddButton(nav, "Trước", () => ShowStep(current - 1));
        AddButton(nav, "Sau", () => ShowStep(current + 1)); AddButton(nav, "Cuối", () => ShowStep(result?.Count ?? 0));
        AddButton(nav, "Chạy / Dừng", TogglePlay); nav.Controls.Add(Caption("ms/bước")); nav.Controls.Add(speed);
        AddButton(nav, "Xuất trace CSV", ExportTrace);
        panel.Controls.Add(nav, 0, 1); panel.Controls.Add(framePanel, 0, 2); panel.Controls.Add(detail, 0, 3);
        Columns(traceGrid, ("Bước", 55), ("Page", 80), ("Hit/Fault", 85), ("Frames", 280), ("Victim", 80),
            ("Bit R", 160), ("Hand", 65), ("Xóa R tại", 120), ("Faults", 75));
        traceGrid.VirtualMode = true;
        traceGrid.CellValueNeeded += (_, e) =>
        {
            if (result is null || e.RowIndex >= result.Steps.Count) return;
            var s = result.Steps[e.RowIndex];
            e.Value = e.ColumnIndex switch
            {
                0 => s.Index, 1 => s.Page, 2 => s.Hit ? "HIT" : "FAULT",
                3 => string.Join(" | ", s.Frames.Select(p => p?.ToString() ?? "—")), 4 => s.Victim?.ToString() ?? "—",
                5 => string.Join(" ", s.Bits), 6 => s.Hand?.ToString() ?? "—", 7 => string.Join(",", s.ClearedSlots), 8 => s.Faults, _ => null
            };
        };
        traceGrid.CellFormatting += (_, e) =>
        {
            if (result is not null && e.RowIndex >= 0 && e.RowIndex < result.Steps.Count)
                e.CellStyle!.BackColor = result.Steps[e.RowIndex].Hit ? Color.FromArgb(233, 247, 239) : Color.FromArgb(255, 240, 230);
        };
        traceGrid.SelectionChanged += (_, _) => { if (!selecting && traceGrid.SelectedRows.Count > 0) ShowStep(traceGrid.SelectedRows[0].Index + 1); };
        panel.Controls.Add(traceGrid, 0, 4);
    }

    private void BuildCompareTab()
    {
        var tab = new TabPage("So sánh và đường cong") { Padding = new Padding(10) }; tabs.TabPages.Add(tab);
        var panel = CreateTableLayout(1, 4); tab.Controls.Add(panel);
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 155));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        var nav = Flow(); nav.Controls.Add(Caption("Frame tối đa")); nav.Controls.Add(maxFrames);
        calculateButtons.Add(AddButton(nav, "Tính so sánh", async () => await RunComparisonAsync()));
        AddButton(nav, "Xuất kết quả", ExportComparison);
        Columns(compareGrid, ("Thuật toán", 180), ("Frames", 90), ("Hits", 120), ("Faults", 120), ("Fault rate", 140), ("Thay thế", 130));
        panel.Controls.Add(nav, 0, 0); panel.Controls.Add(compareGrid, 0, 1); panel.Controls.Add(curveView, 0, 2); panel.Controls.Add(curveNote, 0, 3);
    }

    private InputCase ReadControls()
    {
        var pages = CsvFiles.ParseReferences(references.Text);
        if (pages.Length > 5000) throw new ArgumentException("GUI hỗ trợ tối đa 5000 tham chiếu; dữ liệu lớn dùng CLI.");
        return new InputCase(pages, (int)frameCount.Value);
    }
    private void Preset(string text)
    {
        references.Text = text; frameCount.Value = 3; status.Text = "Đã đổi input. Bấm Mô phỏng / Tính so sánh để cập nhật kết quả.";
    }
    private void SetBusy(bool value)
    {
        busy = value; foreach (var b in calculateButtons) b.Enabled = !value;
        UseWaitCursor = value; if (value) { playTimer.Stop(); status.Text = "Đang tính toán…"; }
    }
    private static void Error(Exception ex) => MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public async Task RunSimulationAsync(bool rethrow = false)
    {
        if (busy) return;
        try
        {
            InputCase input = ReadControls(); Policy policy = Policies.Parse(algorithm.Text); SetBusy(true);
            var calculated = await Task.Run(() => Simulator.Run(input.Pages, input.Frames, policy));
            if (IsDisposed) return;
            result = calculated; resultInput = input;
            selecting = true; traceGrid.RowCount = result.Count; traceGrid.ClearSelection(); selecting = false;
            ShowStep(0); tabs.SelectedIndex = 0;
            status.Text = $"Đã tính {policy.Label()}, {input.Pages.Length} tham chiếu, {input.Frames} frame. Đổi input cần chạy lại.";
        }
        catch (Exception ex) { if (rethrow) throw; if (!IsDisposed) Error(ex); }
        finally { if (!IsDisposed) SetBusy(false); }
    }
    public async Task RunComparisonAsync(bool rethrow = false)
    {
        if (busy) return;
        try
        {
            InputCase input = ReadControls(); int maximum = (int)maxFrames.Value; SetBusy(true);
            var calculated = await Task.Run(() => (Simulator.Compare(input.Pages, input.Frames), Simulator.Curve(input.Pages, maximum)));
            if (IsDisposed) return;
            comparisons = calculated.Item1; curve = calculated.Item2; comparisonInput = input;
            compareGrid.Rows.Clear();
            foreach (var r in comparisons) compareGrid.Rows.Add(r.Policy.Label(), r.Capacity, r.Hits, r.Faults, r.FaultRate.ToString("P2"), r.Replacements);
            curveView.Points = curve; curveView.Invalidate();
            var anomalies = Enumerable.Range(1, curve.Count - 1).Where(i => curve[i].Results[0].Faults > curve[i - 1].Results[0].Faults)
                .Select(i => $"{i}→{i + 1}").ToArray();
            curveNote.Text = anomalies.Length > 0 ? "FIFO tăng lỗi khi tăng frame: " + string.Join(", ", anomalies) : "FIFO không có nghịch lý Belady trong khoảng vừa khảo sát.";
            status.Text = $"Bảng so sánh dùng {input.Frames} frame; đường cong dùng 1..{maximum} frame. Đã tính {input.Pages.Length} tham chiếu.";
            tabs.SelectedIndex = 1;
        }
        catch (Exception ex) { if (rethrow) throw; if (!IsDisposed) Error(ex); }
        finally { if (!IsDisposed) SetBusy(false); }
    }

    private void ShowStep(int index)
    {
        if (result is null) return;
        current = Math.Clamp(index, 0, result.Count); TraceStep? s = current == 0 ? null : result.Steps[current - 1];
        int faults = s?.Faults ?? 0;
        metrics.Text = $"{result.Policy.Label()}  |  Bước {current}/{result.Count}  |  Hits {current - faults}  |  Faults {faults}  |  Tổng cuối {result.Faults} ({result.FaultRate:P2})";
        detail.Text = s is null ? "Cold start: tất cả frame trống; faults=0; Clock bắt đầu hand=0 và R=0." :
            (s.Hit ? "HIT: page đã resident." : s.Victim.HasValue ? $"FAULT: loại page {s.Victim}, nạp vào F{s.Slot}." : $"FAULT: nạp vào frame trống F{s.Slot}.") +
            (s.Order.Count > 0 ? " Thứ tự cũ → mới: " + string.Join(",", s.Order) : "") +
            (s.Hand.HasValue ? $" Hand sau bước: F{s.Hand}; xóa R tại [{string.Join(",", s.ClearedSlots)}]." : "");
        framePanel.SuspendLayout();
        while (framePanel.Controls.Count > 0) { Control c = framePanel.Controls[0]; framePanel.Controls.RemoveAt(0); c.Dispose(); }
        for (int i = 0; i < result.Capacity; i++)
        {
            bool clock = result.Policy == Policy.SecondChance;
            string pointer = clock && i == (s?.Hand ?? 0) ? "> " : "";
            string bit = clock ? $"   R={s?.Bits[i] ?? 0}" : "";
            var label = new Label { Width = 132, Height = 57, BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter, Margin = new Padding(4),
                Text = $"{pointer}F{i}{bit}\n{s?.Frames[i]?.ToString() ?? "—"}",
                BackColor = s is not null && s.Slot == i ? (s.Hit ? Color.FromArgb(187, 247, 208) : Color.FromArgb(254, 215, 170)) : Color.White };
            framePanel.Controls.Add(label);
        }
        framePanel.ResumeLayout();
        selecting = true; traceGrid.ClearSelection();
        if (current > 0) { traceGrid.Rows[current - 1].Selected = true; traceGrid.FirstDisplayedScrollingRowIndex = current - 1; }
        selecting = false;
    }
    private void TogglePlay()
    {
        if (result is null || busy) return;
        if (playTimer.Enabled) playTimer.Stop();
        else { if (current == result.Count) ShowStep(0); playTimer.Start(); }
    }
    private void LoadInput()
    {
        using var d = new OpenFileDialog { Filter = "CSV files|*.csv|All files|*.*" };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var input = CsvFiles.ReadInput(d.FileName);
            if (input.Pages.Length > 5000 || input.Frames > 32) throw new ArgumentException("CSV vượt giới hạn GUI; hãy dùng CLI.");
            references.Text = string.Join(",", input.Pages); frameCount.Value = input.Frames;
            status.Text = "Đã mở CSV; bấm Mô phỏng / Tính so sánh.";
        }
        catch (Exception ex) { Error(ex); }
    }
    private void SaveInput()
    {
        try
        {
            var input = ReadControls(); using var d = new SaveFileDialog { Filter = "CSV files|*.csv", FileName = "input.csv" };
            if (d.ShowDialog(this) == DialogResult.OK) { CsvFiles.WriteInput(d.FileName, input); status.Text = "Đã lưu input."; }
        }
        catch (Exception ex) { Error(ex); }
    }
    private void ExportTrace()
    {
        if (result is null || resultInput is null) { MessageBox.Show("Hãy mô phỏng trước."); return; }
        using var d = new SaveFileDialog { Filter = "CSV files|*.csv", FileName = result.Policy.Label() + "_trace.csv" };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        try { CsvFiles.WriteTrace(d.FileName, result); status.Text = "Đã xuất toàn bộ trace của lần chạy gần nhất."; }
        catch (Exception ex) { Error(ex); }
    }
    private void ExportComparison()
    {
        if (comparisons is null || curve is null || comparisonInput is null) { MessageBox.Show("Hãy tính so sánh trước."); return; }
        using var d = new FolderBrowserDialog { Description = "Chọn thư mục lưu summary.csv, curve.csv và comparison_input.csv" };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        string[] paths = ["summary.csv", "curve.csv", "comparison_input.csv"];
        if (paths.Any(p => File.Exists(Path.Combine(d.SelectedPath, p))) &&
            MessageBox.Show("Có tệp kết quả cùng tên. Ghi đè?", "Xuất CSV", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        try
        {
            CsvFiles.WriteSummary(Path.Combine(d.SelectedPath, paths[0]), comparisons);
            CsvFiles.WriteCurve(Path.Combine(d.SelectedPath, paths[1]), curve);
            CsvFiles.WriteInput(Path.Combine(d.SelectedPath, paths[2]), comparisonInput); status.Text = "Đã xuất kết quả so sánh và input tái lập.";
        }
        catch (Exception ex) { Error(ex); }
    }
    public async Task SmokeTestAsync()
    {
        await RunSimulationAsync(true);
        if (result?.Faults != 15) throw new InvalidOperationException("GUI FIFO result mismatch.");
        ShowStep(5); if (current != 5) throw new InvalidOperationException("Step navigation failed.");
        ShowStep(0); await RunComparisonAsync(true);
        if (comparisons is null || comparisons.Count != 4 || comparisons[2].Faults != 9) throw new InvalidOperationException("Comparison failed.");
        tabs.SelectedIndex = 0; ShowStep(8);
    }
    protected override void OnFormClosed(FormClosedEventArgs e) { playTimer.Stop(); playTimer.Dispose(); base.OnFormClosed(e); }
}
