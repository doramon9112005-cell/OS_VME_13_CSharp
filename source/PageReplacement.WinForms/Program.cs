namespace PageReplacement.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var form = new MainForm();
        if (args.Contains("--smoke"))
        {
            form.Shown += async (_, _) =>
            {
                try
                {
                    await form.SmokeTestAsync();
                    Directory.CreateDirectory("output");
                    using var bitmap = new Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bitmap, form.ClientRectangle);
                    bitmap.Save("output/gui_smoke.png");
                    File.WriteAllText("output/gui_smoke.txt", "PASS: native WinForms initialized, simulation and comparison verified.");
                    Environment.ExitCode = 0;
                }
                catch (Exception ex)
                {
                    Directory.CreateDirectory("output"); File.WriteAllText("output/gui_smoke.txt", ex.ToString());
                    Environment.ExitCode = 1;
                }
                finally { form.Close(); }
            };
        }
        Application.Run(form);
    }
}
