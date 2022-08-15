namespace ColorPicker.Gallery;

public partial class ColorCircleView : ContentPage
{
    public ColorCircleView()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        ColorCircle.SelectedColorChanged += ColorCircle_SelectedColorChangedEvent;
        ColorCircle.SelectedColor = GetColorFromString( "#9D9E9C" )!;
    }

    protected override void OnDisappearing()
    {
        ColorCircle.SelectedColorChanged -= ColorCircle_SelectedColorChangedEvent;
    }

    void SelectedColorEntry_Completed( object sender, EventArgs e )
    {
        var color =  GetColorFromString( ((Entry)sender).Text );

        if ( color is not null )
            ColorCircle.SelectedColor = color;
    }

    void ReticleSizeEntry_Completed( object sender, EventArgs e )
    {
        var reticleSize = GetSizeFromText( ((Entry)sender).Text );

        ColorCircle.ReticleRadius = reticleSize < 15f ? 15f
                                                      : reticleSize > 40f ? 40f
                                                                          : reticleSize;
    }

    void HasCrossHairs_CheckedChanged( object sender, CheckedChangedEventArgs e )
    {
        ColorCircle.ShowReticleCrossHairs = HasCrossHairs.IsChecked;
    }

    private void ColorCircle_SelectedColorChangedEvent( object? sender, ColorChangedEventArgs e )
    {
        SelectedColorEntry.Text = e.NewColor.ToHex();
    }

    Color? GetColorFromString( string value )
    {
        if ( string.IsNullOrEmpty( value ) )
            return null;

        try
        {
            if ( Color.TryParse( value, out var newColor ) )
                return newColor;

            return null;
        }
        catch ( Exception )
        {
            return null;
        }
    }

    float GetSizeFromText( string value )
    {
        if ( !string.IsNullOrEmpty( value ) && float.TryParse( value, out var result ) )
            return result;

        return 20f;
    }
}