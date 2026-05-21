using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Tier 1 — LayoutSmokeTests. For each (control × size), apply the scenario
/// and assert basic geometric invariants:
///   1. The host border is the requested size (within DPI tolerance).
///   2. The control's outer bounds fit within the host.
///   3. The control has rendered something (Width > 0 and Height > 0).
///
/// We deliberately keep these assertions loose at this tier — Tier 2 will
/// add per-control shape invariants (e.g. ColorWheel disc is square).
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class LayoutSmokeTests : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public LayoutSmokeTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    public static TheoryData<string> Scenarios()
    {
        var data = new TheoryData<string>();
        var sizes = new[] { "100x100", "800x800", "300x600", "600x300" };
        foreach (var ctrl in new[] { "wheel", "triangle", "hsl", "rgb" })
        foreach (var sz in sizes)
            data.Add($"{ctrl}:{sz}");
        return data;
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    public void Control_Fits_Within_Host(string scenario)
    {
        var page  = _fixture.Page;
        var state = page.Apply(scenario);

        var (expectedW, expectedH) = ParseSize(scenario);

        // 1. Host actually resized to the requested logical size (these are
        //    MAUI logical units, so DPI scale doesn't apply).
        Assert.True(Math.Abs(state.HostBounds.W - expectedW) <= 1,
            $"Host width {state.HostBounds.W} should == {expectedW} (scenario {scenario})");
        Assert.True(Math.Abs(state.HostBounds.H - expectedH) <= 1,
            $"Host height {state.HostBounds.H} should == {expectedH} (scenario {scenario})");

        // 2. Control sized > 0.
        Assert.True(state.ControlBounds.W > 0, $"Control width 0 in {scenario}");
        Assert.True(state.ControlBounds.H > 0, $"Control height 0 in {scenario}");

        // 3. Control fits inside host (no overflow). Allow 1px slack for rounding.
        Assert.True(state.ControlBounds.W <= state.HostBounds.W + 1,
            $"Control width {state.ControlBounds.W} > host {state.HostBounds.W} ({scenario})");
        Assert.True(state.ControlBounds.H <= state.HostBounds.H + 1,
            $"Control height {state.ControlBounds.H} > host {state.HostBounds.H} ({scenario})");
    }

    /// <summary>
    /// MeasureOverride probe: the inner control's reported DesiredSize must not
    /// exceed the host bounds (within rounding slack). Catches regressions where
    /// MeasureOverride returns a too-large size — the parent host would clamp
    /// the arranged ControlBounds, hiding the bug from
    /// <see cref="Control_Fits_Within_Host"/>. We only test aspect-locked
    /// controls (wheel, triangle) at square sizes here because slider stacks
    /// expand to fill on their long axis by design.
    /// </summary>
    [Theory]
    [InlineData("wheel:400x400")]
    [InlineData("wheel:200x200")]
    [InlineData("triangle:400x400")]
    [InlineData("triangle:200x200")]
    public void DesiredSize_Does_Not_Exceed_Host(string scenario)
    {
        var page  = _fixture.Page;
        var state = page.Apply(scenario);

        // DesiredSize is reported by the harness only for the new 5-segment
        // marker; older builds will report 0. Skip if not populated rather
        // than assert false-positive.
        if (state.ControlDesiredW <= 0 && state.ControlDesiredH <= 0)
            return;

        Assert.True(state.ControlDesiredW <= state.HostBounds.W + 2,
            $"Inner control DesiredSize.W={state.ControlDesiredW} exceeds host W={state.HostBounds.W} ({scenario}). " +
            "MeasureOverride is asking for more space than the host can give.");
        Assert.True(state.ControlDesiredH <= state.HostBounds.H + 2,
            $"Inner control DesiredSize.H={state.ControlDesiredH} exceeds host H={state.HostBounds.H} ({scenario}).");
    }

    private static (int w, int h) ParseSize(string scenario)
    {
        var parts = scenario.Split(':');
        var sz = parts[1].Split('x');
        return (int.Parse(sz[0]), int.Parse(sz[1]));
    }
}
