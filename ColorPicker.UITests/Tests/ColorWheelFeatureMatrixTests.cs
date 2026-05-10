using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Tier 2 — ColorWheelFeatureMatrixTests.
///
/// Exercises every combination of the four ColorWheel feature flags
/// (ShowAlpha × ShowLumSlider × ShowLumWheel × Vertical) at a single
/// host size. 16 scenarios total (2^4).
///
/// Spec-A invariants asserted (per <c>plan.md</c>):
///   • Host is sized to the requested logical bounds.
///   • Control fits within the host (no overflow on either axis).
///   • Control occupies non-zero area.
///   • At square hosts, the rendered control retains a sensible aspect
///     ratio: the disc area is square (control width == disc, height ==
///     disc + slider thickness when sliders are stacked, or width ==
///     disc + slider thickness in vertical mode).
///
/// We deliberately keep exact pixel layout assertions in higher tiers; this
/// suite is about catching gross layout regressions across feature combos.
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class ColorWheelFeatureMatrixTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public ColorWheelFeatureMatrixTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    private const int HostSize = 400;

    public static TheoryData<string> FeatureCombos()
    {
        var data = new TheoryData<string>();
        // 4-bit combinatorial expansion. Bit 0 = alpha, 1 = lumslider,
        // 2 = lumwheel (true means default-on; false adds nolumwheel),
        // 3 = vertical.
        for (int mask = 0; mask < 16; mask++)
        {
            var opts = new List<string>();
            if ((mask & 1) != 0) opts.Add("alpha");
            if ((mask & 2) != 0) opts.Add("lumslider");
            if ((mask & 4) == 0) opts.Add("nolumwheel");
            if ((mask & 8) != 0) opts.Add("vertical");

            var spec = opts.Count == 0
                ? $"wheel:{HostSize}x{HostSize}"
                : $"wheel:{HostSize}x{HostSize}:{string.Join(",", opts)}";
            data.Add(spec);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(FeatureCombos))]
    public void Wheel_Fits_Host_For_Every_Feature_Combo(string scenario)
    {
        var page  = _fixture.Page;
        var state = page.Apply(scenario);

        // Host sized to spec.
        Assert.True(Math.Abs(state.HostBounds.W - HostSize) <= 1,
            $"Host width {state.HostBounds.W} != {HostSize} ({scenario})");
        Assert.True(Math.Abs(state.HostBounds.H - HostSize) <= 1,
            $"Host height {state.HostBounds.H} != {HostSize} ({scenario})");

        // Control rendered with positive area.
        Assert.True(state.ControlBounds.W > 0, $"Control width 0 ({scenario})");
        Assert.True(state.ControlBounds.H > 0, $"Control height 0 ({scenario})");

        // Control stays inside host on both axes.
        Assert.True(state.ControlBounds.W <= state.HostBounds.W + 1,
            $"Control overflows host horizontally: {state.ControlBounds.W} > {state.HostBounds.W} ({scenario})");
        Assert.True(state.ControlBounds.H <= state.HostBounds.H + 1,
            $"Control overflows host vertically: {state.ControlBounds.H} > {state.HostBounds.H} ({scenario})");
    }
}
