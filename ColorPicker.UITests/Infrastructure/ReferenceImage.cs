namespace ColorPicker.UITests.Infrastructure;

/// <summary>
/// Tier 6 / Layer 2 reference image infrastructure.
///
/// Stores golden PNGs under
/// <c>ColorPicker.UITests/References/dpi-{N}/{scenario-id}.png</c> where
/// <c>N</c> is the value returned by <c>GetDpiForWindow</c> (96, 120, 144 …).
///
/// Default mode: load the reference, crop the live capture to the same
/// rectangle, and assert the per-pixel difference is within tolerance.
///
/// Regenerate mode (env var <c>REGEN_REFS=1</c>): save the live capture as
/// the new reference and skip comparison. Use this when the picker visuals
/// change intentionally — re-run, eyeball the resulting PNGs, then commit.
/// </summary>
public static class ReferenceImage
{
    public const string RegenEnvVar = "REGEN_REFS";

    /// <summary>Resolves the references directory for a given DPI, walking
    /// up from the test bin folder to the project source tree so saved
    /// references land in source control rather than the build output.</summary>
    public static string ResolveDir(int dpi)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            var candidate = Path.Combine(dir, "ColorPicker.UITests", "References");
            if (Directory.Exists(Path.GetDirectoryName(candidate)!))
            {
                var dpiDir = Path.Combine(candidate, $"dpi-{dpi}");
                Directory.CreateDirectory(dpiDir);
                return dpiDir;
            }
            // Heuristic: when running from bin/Release/net8.0/ → walk up to
            // the test project directory and from there to the repo root.
            var parent = Directory.GetParent(dir)?.FullName;
            if (parent == dir) break;
            dir = parent;
        }
        // Fallback: write next to the test binaries.
        var fallback = Path.Combine(AppContext.BaseDirectory, "References", $"dpi-{dpi}");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    public static string ResolvePath(int dpi, string scenarioId)
        => Path.Combine(ResolveDir(dpi), scenarioId + ".png");

    public static bool RegenRequested =>
        Environment.GetEnvironmentVariable(RegenEnvVar) == "1";

    /// <summary>Compare two same-size images; return the fraction of pixels
    /// whose sum-of-channel diff exceeds <paramref name="perPixelTol"/>.</summary>
    public static double FractionMismatched(
        PixelImage a, PixelImage b, int perPixelTol)
    {
        if (a.Width != b.Width || a.Height != b.Height)
            throw new InvalidOperationException(
                $"Size mismatch: {a.Width}×{a.Height} vs {b.Width}×{b.Height}");

        long bad = 0;
        long total = (long)a.Width * a.Height;
        for (int y = 0; y < a.Height; y++)
        for (int x = 0; x < a.Width;  x++)
        {
            var pa = a[x, y]; var pb = b[x, y];
            int d = Math.Abs(pa.R - pb.R)
                  + Math.Abs(pa.G - pb.G)
                  + Math.Abs(pa.B - pb.B);
            if (d > perPixelTol) bad++;
        }
        return bad / (double)total;
    }

    /// <summary>Crop a screenshot to the rectangle of interest.</summary>
    public static PixelImage Crop(PixelImage source, PixelRect rect)
        => source.Crop(rect.X, rect.Y, rect.W, rect.H);

    public static void Save(PixelImage img, string path) => img.Save(path);

    public static PixelImage Load(string path) => PixelImage.Load(path);
}
