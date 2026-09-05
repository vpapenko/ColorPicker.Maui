using ColorPicker.UITests.Infrastructure;

namespace ColorPicker.UITests.Tests;

[Collection(AppiumServerCollection.Name)]
public sealed class RendererCustomizationTests
    : IClassFixture<AppiumServerFixture>, IClassFixture<LayoutTestAppFixture>
{
    readonly LayoutTestAppFixture _fixture;

    public RendererCustomizationTests(LayoutTestAppFixture fixture)
        => _fixture = fixture;

    [Theory]
    [InlineData("wheel:400x400:customindicator")]
    [InlineData("triangle:400x400:customindicator")]
    [InlineData("hsl:400x200:customindicator")]
    public void Custom_Renderer_Can_Replace_Only_Indicators(string scenario)
    {
        _fixture.Page.Apply(scenario);

        using var image = _fixture.Page.CaptureCanvasImage();
        var customPixels = CountExactPixels(image, new Pixel(1, 2, 3, 255));

        Assert.True(
            customPixels >= 100,
            $"Expected the custom indicator renderer in '{scenario}', found {customPixels} exact test-color pixels.");
    }

    [Fact]
    public void Classic_Renderer_Property_Change_Repaints_Existing_Control()
    {
        _fixture.Page.Apply("wheel:400x400");
        _fixture.Page.Apply("wheel:400x400:ifill=#010203");

        var styledPixels = _fixture.Page.WaitForWindowColorPixels(
            new Pixel(1, 2, 3, 255),
            minimumCount: 100);

        Assert.True(
            styledPixels >= 100,
            $"Expected the configured classic indicator fill, found {styledPixels} exact pixels.");
    }

    [Fact]
    public void Renderer_Property_Can_Bind_To_Control_BindingContext()
    {
        _fixture.Page.Apply("wheel:400x400");
        _fixture.Page.Apply("wheel:400x400:bindfill=#010203");

        using var image = _fixture.Page.CaptureCanvasImage();
        var boundPixels = CountExactPixels(image, new Pixel(1, 2, 3, 255));

        Assert.True(
            boundPixels >= 100,
            $"Expected the bound renderer property, found {boundPixels} exact pixels.");
    }

    [Fact]
    public void Renderer_Binding_Survives_Removing_A_Shared_Child()
    {
        _fixture.Page.Apply("wheel:400x400:bindfill=#010203,removealphaafterattach");

        using var image = _fixture.Page.CaptureCanvasImage();
        var boundPixels = CountExactPixels(image, new Pixel(1, 2, 3, 255));

        Assert.True(
            boundPixels >= 100,
            $"Expected the renderer binding to survive child removal, found {boundPixels} exact pixels.");
    }

    [Fact]
    public void Renderer_Callback_Cannot_Leak_Canvas_State_To_Later_Elements()
    {
        _fixture.Page.Apply("wheel:400x400:bg=white");
        using var baseline = _fixture.Page.CaptureCanvasImage();

        _fixture.Page.Apply("wheel:400x400:bg=white,overrestore");
        using var isolated = _fixture.Page.CaptureCanvasImage();

        var differingPixels = CountDifferentPixels(baseline, isolated);
        Assert.True(
            differingPixels <= 10,
            $"An over-restoring renderer shifted later elements ({differingPixels} pixels differ).");
    }

    [Fact]
    public void Zero_Indicator_Thicknesses_Do_Not_Draw_Hairlines()
    {
        _fixture.Page.Apply("wheel:400x400:zerostrokestransparent");
        using var transparentStrokes = _fixture.Page.CaptureCanvasImage();

        _fixture.Page.Apply("wheel:400x400:zerostrokes");
        using var coloredStrokes = _fixture.Page.CaptureCanvasImage();

        Assert.Equal(0, CountDifferentPixels(transparentStrokes, coloredStrokes));
    }

    static int CountExactPixels(PixelImage image, Pixel expected)
    {
        var count = 0;
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                if (image[x, y] == expected)
                    count++;
            }
        }
        return count;
    }

    static int CountDifferentPixels(PixelImage first, PixelImage second)
    {
        Assert.Equal(first.Width, second.Width);
        Assert.Equal(first.Height, second.Height);

        var count = 0;
        for (var y = 0; y < first.Height; y++)
        {
            for (var x = 0; x < first.Width; x++)
            {
                if (first[x, y] != second[x, y])
                    count++;
            }
        }
        return count;
    }
}
