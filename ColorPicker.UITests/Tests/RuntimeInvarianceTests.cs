using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Tier 3 — RuntimeInvarianceTests.
///
/// Verifies that the layout produced by <em>creating a wheel with feature
/// flag X already set</em> matches the layout produced by <em>creating a
/// bare wheel and then toggling X on at runtime</em>. This is the exact
/// regression class fixed by the SkiaPickerBase.ArrangeOverride change
/// (canvas was not re-arranged at the new Frame.Size when child layout
/// invalidated due to a property change).
///
/// The harness detects "same control + same size" Apply() calls and mutates
/// the existing instance instead of rebuilding it, so this suite exercises
/// the runtime-toggle path.
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class RuntimeInvarianceTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public RuntimeInvarianceTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    private const int HostSize = 400;

    public static TheoryData<string> Toggles()
    {
        var data = new TheoryData<string>();
        foreach (var opt in new[] { "alpha", "lumslider", "nolumwheel", "vertical" })
            data.Add(opt);
        // A few combos as well.
        data.Add("alpha,vertical");
        data.Add("alpha,lumslider");
        data.Add("nolumwheel,vertical");
        data.Add("alpha,lumslider,nolumwheel,vertical");
        return data;
    }

    [Theory]
    [MemberData(nameof(Toggles))]
    public void Toggling_Flags_At_Runtime_Matches_Fresh_Build(string flagsCsv)
    {
        var page = _fixture.Page;

        // 0) Bare baseline (no flags) at the test size, for the "flags actually
        // do something" assertion below. The Apply() with a different size first
        // guarantees this is a true rebuild, not a runtime toggle.
        page.Apply($"wheel:{HostSize - 50}x{HostSize - 50}");
        var bare = page.Apply($"wheel:{HostSize}x{HostSize}");

        // 1) Reference: wheel built fresh with the flags already set.
        page.Apply($"wheel:{HostSize - 50}x{HostSize - 50}");
        var fresh = page.Apply($"wheel:{HostSize}x{HostSize}:{flagsCsv}");

        // 2) Runtime toggle: bare wheel, then toggle the same flags on.
        page.Apply($"wheel:{HostSize - 50}x{HostSize - 50}");
        page.Apply($"wheel:{HostSize}x{HostSize}");                 // bare
        var toggled = page.Apply($"wheel:{HostSize}x{HostSize}:{flagsCsv}"); // mutated in place

        // The two flagged layouts must agree to within 1 logical px.
        Assert.True(Math.Abs(fresh.HostBounds.W - toggled.HostBounds.W) <= 1,
            $"Host W differs: fresh={fresh.HostBounds.W} toggled={toggled.HostBounds.W} ({flagsCsv})");
        Assert.True(Math.Abs(fresh.HostBounds.H - toggled.HostBounds.H) <= 1,
            $"Host H differs: fresh={fresh.HostBounds.H} toggled={toggled.HostBounds.H} ({flagsCsv})");
        Assert.True(Math.Abs(fresh.ControlBounds.W - toggled.ControlBounds.W) <= 1,
            $"Control W differs: fresh={fresh.ControlBounds.W} toggled={toggled.ControlBounds.W} ({flagsCsv})");
        Assert.True(Math.Abs(fresh.ControlBounds.H - toggled.ControlBounds.H) <= 1,
            $"Control H differs: fresh={fresh.ControlBounds.H} toggled={toggled.ControlBounds.H} ({flagsCsv})");

        // Anti-no-op guard: flags that change the outer layout (alpha and
        // lumslider both add a slider stack that shrinks the wheel circle)
        // must produce a layout DIFFERENT from the bare wheel. Otherwise both
        // fresh and toggled could be silently broken identically (e.g., a flag
        // handler became a no-op) and the equality check above still passes.
        // Flags that only change inner rendering (nolumwheel removes the
        // luminosity ring, vertical flips orientation but keeps a square host)
        // don't change outer ControlBounds and are exempt.
        var flagsList = flagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var sizeChangingFlags = new HashSet<string> { "alpha", "lumslider" };
        if (flagsList.Any(f => sizeChangingFlags.Contains(f)))
        {
            var dW = Math.Abs(fresh.ControlBounds.W - bare.ControlBounds.W);
            var dH = Math.Abs(fresh.ControlBounds.H - bare.ControlBounds.H);
            Assert.True(dW > 1 || dH > 1,
                $"Flags '{flagsCsv}' produced no visible layout change vs bare wheel: " +
                $"bare={bare.ControlBounds} fresh={fresh.ControlBounds}. " +
                "Either the flag handler is a no-op or the harness ignored the flags.");
        }
    }
}
