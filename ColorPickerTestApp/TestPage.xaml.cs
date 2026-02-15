namespace ColorPickerTestApp;

using ColorPicker.Controls;

public partial class TestPage : ContentPage
{
    public TestPage()
    {
        System.Diagnostics.Debug.WriteLine( "[TestPage] Constructor START" );

        Title = "ColorWheel Test";

        var colorWheel = new ColorWheel
        {
            WidthRequest          = 300,
            HeightRequest         = 300,
            ShowLuminosityWheel   = false,
            ShowLuminositySlider  = false,
            ShowAlphaSlider       = true,
            HorizontalOptions     = LayoutOptions.Center,
            VerticalOptions       = LayoutOptions.Center
        };

        Content = new VerticalStackLayout
        {
            Padding           = new Thickness( 20 ),
            Spacing           = 20,
            VerticalOptions   = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text              = "Test: ColorWheel control",
                    FontSize          = 18,
                    HorizontalOptions = LayoutOptions.Center
                },
                colorWheel,
                new Label
                {
                    Text              = "If you see a color wheel with sliders, ColorWheel works.",
                    FontSize          = 14,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };

        System.Diagnostics.Debug.WriteLine( "[TestPage] Constructor END" );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine( "[TestPage] OnAppearing" );
    }
}
