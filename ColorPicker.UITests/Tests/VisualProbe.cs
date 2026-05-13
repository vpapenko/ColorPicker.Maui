using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Visual probe — opt-in only. Iterates every distinct scenario used by
/// the suite, applies it, crops the host rectangle, and saves it as a
/// PNG to the path in the <c>PROBE_OUT</c> environment variable. Used
/// by humans to eyeball every test scenario.
///
/// Disabled unless <c>PROBE_OUT</c> is set so it doesn't run in CI.
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class VisualProbe
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    private readonly ITestOutputHelper _log;
    public VisualProbe(LayoutTestAppFixture fixture, ITestOutputHelper log)
    { _fixture = fixture; _log = log; }

    public static IEnumerable<object[]> AllScenarios()
    {
        var seen = new HashSet<string>();
        void Add(string id, string spec)
        {
            if (seen.Add(spec)) {/*ok*/}
            else id += "-dup";
            // we still emit duplicates with -dup suffix so the manifest is complete.
        }
        var list = new List<(string id, string spec)>();
        void Emit(string id, string spec) {
            // ensure unique id by suffixing
            string useId = id; int i = 1;
            while (list.Any(t => t.id == useId)) useId = id + "-" + (++i);
            list.Add((useId, spec));
        }

        // Tier 1 — LayoutSmoke
        foreach (var ctrl in new[] { "wheel", "triangle", "hsl", "rgb" })
        foreach (var sz   in new[] { "100x100", "800x800", "300x600", "600x300" })
            Emit($"smoke-{ctrl}-{sz}", $"{ctrl}:{sz}");

        // Tier 2 — feature matrix
        for (int mask = 0; mask < 16; mask++)
        {
            var opts = new List<string>();
            if ((mask & 1) != 0) opts.Add("alpha");
            if ((mask & 2) != 0) opts.Add("lumslider");
            if ((mask & 4) == 0) opts.Add("nolumwheel");
            if ((mask & 8) != 0) opts.Add("vertical");
            var spec = opts.Count == 0 ? "wheel:400x400" : "wheel:400x400:" + string.Join(",", opts);
            Emit($"feat-{mask:00}-{string.Join("_", opts)}".TrimEnd('-','_'), spec);
        }

        // Tier 4 — container sizing
        foreach (var s in new[] {
            "wheel:400x400","wheel:400xfill","wheel:400xauto",
            "wheel:fillx400","wheel:fillxfill","wheel:fillxauto",
            "wheel:autox400","wheel:autoxfill","wheel:autoxauto",
            "triangle:fillxfill","rgb:fillxfill","hsl:fillxfill" })
            Emit("sizing-" + s.Replace(":","-").Replace(",","_"), s);

        // Padding
        foreach (var s in new[] {"wheel:300x300","wheel:600x600","triangle:300x300"})
            Emit("padding-" + s.Replace(":","-"), s);

        // Triangle ring modes (rotate-with-hue vs. fixed-triangle rotating ring)
        foreach (var sz in new[] { "400x400", "300x600", "600x300" })
        {
            Emit($"triangle-rotate-{sz}",   $"triangle:{sz}");
            Emit($"triangle-norotate-{sz}", $"triangle:{sz}:norotate");
        }

        // Slider orientation + alpha visibility
        foreach (var ctrl in new[] { "hsl", "rgb" })
        foreach (var sz   in new[] { "400x400", "300x600", "600x300" })
        {
            Emit($"slider-{ctrl}-vertical-{sz}", $"{ctrl}:{sz}:vertical");
            Emit($"slider-{ctrl}-noalpha-{sz}",  $"{ctrl}:{sz}:noalpha");
        }

        // Tier — fixed PickerRadiusScale on standalone sliders. Without prs the
        // slider stack fills the whole cell; with prs > 0 the orthogonal axis
        // becomes fixed (thickness = count * 2.2 * prs * length) and only the
        // length axis still fills.
        foreach (var ctrl in new[] { "hsl", "rgb" })
        foreach (var sz   in new[] { "400x400", "600x300", "300x600" })
        foreach (var scale in new[] { "0.04", "0.08" })
        {
            Emit($"prs-{ctrl}-{sz}-prs{scale.Replace(".","")}",            $"{ctrl}:{sz}:prs={scale}");
            Emit($"prs-{ctrl}-{sz}-vertical-prs{scale.Replace(".","")}",   $"{ctrl}:{sz}:vertical,prs={scale}");
        }

        // Control-internal background (wbg=) — distinct from host bg=
        foreach (var s in new[] {
            "wheel:400x400:wbg=red",
            "wheel:400x400:wbg=yellow",
            "triangle:400x400:wbg=red",
            "triangle:400x400:wbg=blue" })
            Emit("wbg-" + s.Replace(":","-").Replace("=","-").Replace("#",""), s);

        // Background
        foreach (var s in new[] {
            "wheel:400x400:bg=red","wheel:400x400:bg=blue","wheel:400x400:bg=yellow",
            "wheel:400x400:bg=black","triangle:400x400:bg=red","triangle:400x400:bg=green",
            "wheel:400x400:bg=#FF8000" })
            Emit("bg-" + s.Replace(":","-").Replace("=","-").Replace("#","").Replace(",","_"), s);

        // Window events used "wheel:fillxfill" already.

        foreach (var t in list) yield return new object[] { t.id, t.spec };
    }

    [Theory]
    [MemberData(nameof(AllScenarios))]
    public void Capture(string id, string scenario)
    {
        var outDir = Environment.GetEnvironmentVariable("PROBE_OUT");
        if (string.IsNullOrEmpty(outDir))
        {
            _log.WriteLine("PROBE_OUT not set — skipping.");
            return;
        }
        Directory.CreateDirectory(outDir);

        var page = _fixture.Page;
        try
        {
            page.Apply(scenario);
        }
        catch
        {
            // Dump page debug state to a per-failure log file so we can
            // diagnose the regression without re-running each timeout.
            try
            {
                var trace  = page.Find("DebugTrace").Text ?? "";
                var marker = page.AppliedMarker.Text ?? "";
                var entry  = page.ScenarioEntry.Text ?? "";
                var status = page.Find("ScenarioStatus").Text ?? "";
                File.AppendAllText(Path.Combine(outDir, "_failures.log"),
                    $"\n--- {id} ({scenario}) ---\nENTRY='{entry}'\nSTATUS={status}\nMARKER={marker}\nTRACE={trace}\n");
            }
            catch { }
            throw;
        }

        // Use the in-process Skia canvas capture (deterministic, no DPI artifacts).
        var capturedPath = page.CaptureCanvas();
        var dest = Path.Combine(outDir, $"{id}.png");
        File.Copy(capturedPath, dest, overwrite: true);

        var markerText = page.AppliedMarker.Text ?? "";
        var traceText  = page.Find("DebugTrace").Text ?? "";
        File.AppendAllText(Path.Combine(outDir, "_manifest.tsv"),
            $"{id}\t{scenario}\tcanvas={capturedPath}\tmarker={markerText}\n");
        File.AppendAllText(Path.Combine(outDir, "_diag.txt"),
            $"\n=== {id} ({scenario}) ===\nMARKER={markerText}\nTRACE={traceText}\n");

        _log.WriteLine($"saved {dest}");
    }
}
