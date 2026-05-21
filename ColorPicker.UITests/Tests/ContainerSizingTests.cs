using ColorPicker.UITests.Infrastructure;
using ColorPicker.UITests.PageObjects;
using Xunit;

namespace ColorPicker.UITests.Tests;

/// <summary>
/// Tier 4 — ContainerSizingTests.
///
/// Exercises the 3×3 matrix of host sizing modes
/// (Fixed × Auto × Fill) on width × height with ColorWheel as the
/// payload, plus a couple of cross-control sanity checks for the
/// most common cells. Asserts that the host responds to each mode
/// correctly and the control stays within the host's bounds.
///
/// Size-token vocabulary in the spec:
///   integer → Fixed     (e.g. "400")
///   "auto"  → wrap content (no WidthRequest, HorizontalOptions=Start)
///   "fill"  → expand into parent (HorizontalOptions=Fill, no WidthRequest)
/// </summary>
[Collection(ColorPicker.UITests.Infrastructure.AppiumServerCollection.Name)]
public class ContainerSizingTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    private readonly LayoutTestAppFixture _fixture;
    public ContainerSizingTests(LayoutTestAppFixture fixture) => _fixture = fixture;

    private const double Tol = 2.0; // logical-px tolerance

    public static TheoryData<string, string, string> WheelMatrix()
    {
        // (scenario, wMode, hMode)
        return new TheoryData<string, string, string>
        {
            { "wheel:400x400",       "fixed", "fixed" },
            { "wheel:400xfill",      "fixed", "fill"  },
            { "wheel:400xauto",      "fixed", "auto"  },
            { "wheel:fillx400",      "fill",  "fixed" },
            { "wheel:fillxfill",     "fill",  "fill"  },
            { "wheel:fillxauto",     "fill",  "auto"  },
            { "wheel:autox400",      "auto",  "fixed" },
            { "wheel:autoxfill",     "auto",  "fill"  },
            { "wheel:autoxauto",     "auto",  "auto"  },
        };
    }

    [Theory]
    [MemberData(nameof(WheelMatrix))]
    public void Host_Honors_Sizing_Mode(string scenario, string wMode, string hMode)
    {
        var state = _fixture.Page.Apply(scenario);

        AssertDimension("W", wMode, state.HostBounds.W, state.ViewportBounds.W,
                        expectedFixed: 400, scenario);
        AssertDimension("H", hMode, state.HostBounds.H, state.ViewportBounds.H,
                        expectedFixed: 400, scenario);

        // Control must always render with positive area and fit inside host.
        Assert.True(state.ControlBounds.W > 0 && state.ControlBounds.H > 0,
            $"Control collapsed to zero in {scenario} (ctrl={state.ControlBounds})");
        Assert.True(state.ControlBounds.W <= state.HostBounds.W + 1,
            $"Control W {state.ControlBounds.W} overflows host {state.HostBounds.W} ({scenario})");
        Assert.True(state.ControlBounds.H <= state.HostBounds.H + 1,
            $"Control H {state.ControlBounds.H} overflows host {state.HostBounds.H} ({scenario})");
    }

    [Theory]
    [InlineData("triangle:fillxfill")]
    [InlineData("rgb:fillxfill")]
    [InlineData("hsl:fillxfill")]
    public void Other_Controls_Fill_Mode(string scenario)
    {
        var state = _fixture.Page.Apply(scenario);
        // Fill on both axes ⇒ host should match viewport within tolerance.
        Assert.True(Math.Abs(state.HostBounds.W - state.ViewportBounds.W) <= Tol,
            $"Host W {state.HostBounds.W} should ≈ viewport {state.ViewportBounds.W} ({scenario})");
        Assert.True(Math.Abs(state.HostBounds.H - state.ViewportBounds.H) <= Tol,
            $"Host H {state.HostBounds.H} should ≈ viewport {state.ViewportBounds.H} ({scenario})");
        Assert.True(state.ControlBounds.W > 0 && state.ControlBounds.H > 0,
            $"Control collapsed in {scenario}");
    }

    private static void AssertDimension(string axis, string mode,
                                        double hostV, double viewportV,
                                        double expectedFixed, string scenario)
    {
        switch (mode)
        {
            case "fixed":
                Assert.True(Math.Abs(hostV - expectedFixed) <= 1,
                    $"{axis}: host {hostV} should == {expectedFixed} ({scenario})");
                break;
            case "fill":
                Assert.True(Math.Abs(hostV - viewportV) <= Tol,
                    $"{axis}: host {hostV} should ≈ viewport {viewportV} ({scenario})");
                break;
            case "auto":
                // Auto = wrap content. Host must be positive but not extend
                // past the viewport (otherwise it ignored the auto request).
                Assert.True(hostV > 0,
                    $"{axis}: host {hostV} should be > 0 in auto mode ({scenario})");
                Assert.True(hostV <= viewportV + Tol,
                    $"{axis}: host {hostV} exceeds viewport {viewportV} in auto mode ({scenario})");
                break;
            default:
                throw new ArgumentException("unknown mode: " + mode);
        }
    }
}
