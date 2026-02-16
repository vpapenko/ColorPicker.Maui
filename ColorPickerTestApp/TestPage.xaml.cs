namespace ColorPickerTestApp;

public partial class TestPage : ContentPage
{
    public TestPage()
    {
        System.Diagnostics.Debug.WriteLine( "[TestPage] Constructor START" );
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine( "[TestPage] Constructor END" );
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine( "[TestPage] OnAppearing" );
    }
}
