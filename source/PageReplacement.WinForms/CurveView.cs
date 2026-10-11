using System.Drawing.Drawing2D;
using PageReplacement.Core;

namespace PageReplacement.WinForms;

public sealed class CurveView : Control
{
    public IReadOnlyList<CurvePoint> Points { get; set; } = Array.Empty<CurvePoint>();
    private static readonly Color[] Colors = [Color.RoyalBlue, Color.SeaGreen, Color.DarkViolet, Color.DarkOrange];
    public CurveView() { DoubleBuffered = true; BackColor = Color.White; Dock = DockStyle.Fill; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Points.Count == 0) { e.Graphics.DrawString("Chọn Tính so sánh để xem đường cong số lỗi trang.", Font, Brushes.DimGray, 20, 20); return; }
        if (Width < 250 || Height < 130) return;
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        float left = 65, right = Width - 25, top = 50, bottom = Height - 45;
        int maximum = Math.Max(1, Points.Max(p => p.Results.Max(r => r.Faults)));
        using var gridPen = new Pen(Color.FromArgb(225, 231, 238));
        for (int k = 0; k <= 4; k++)
        {
            float y = bottom - k * (bottom - top) / 4;
            g.DrawLine(gridPen, left, y, right, y);
            g.DrawString(Math.Round(k * maximum / 4d).ToString(), Font, Brushes.DimGray, 4, y - 8);
        }
        g.DrawLine(Pens.SlateGray, left, top, left, bottom); g.DrawLine(Pens.SlateGray, left, bottom, right, bottom);
        g.DrawString("Page faults", Font, Brushes.Black, 5, 12);
        for (int j = 0; j < Policies.All.Length; j++)
        {
            using var pen = new Pen(Colors[j], 2); using var brush = new SolidBrush(Colors[j]);
            g.DrawString(Policies.All[j].Label(), Font, brush, 150 + j * 140, 12);
            var points = Points.Select(p => new PointF(left + (p.Frames - 1) * (right - left) / Math.Max(1, Points.Count - 1),
                bottom - (float)p.Results[j].Faults / maximum * (bottom - top))).ToArray();
            if (points.Length > 1) g.DrawLines(pen, points);
            for (int i = 0; i < points.Length; i++)
            {
                g.FillEllipse(brush, points[i].X - 3, points[i].Y - 3, 6, 6);
                if (j == 0 && (Points.Count <= 16 || (i + 1) % 2 == 0))
                    g.DrawString((i + 1).ToString(), Font, Brushes.DimGray, points[i].X - 5, bottom + 7);
            }
        }
        g.DrawString("Số frame", Font, Brushes.DimGray, right - 70, bottom + 25);
    }
}
