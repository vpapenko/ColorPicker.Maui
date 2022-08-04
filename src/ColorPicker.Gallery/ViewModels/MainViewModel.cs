namespace ColorPicker.Gallery;

public class MainViewModel : BaseGalleryViewModel
{
    protected override IEnumerable<SectionModel> CreateItems() => new SectionModel[]
    {
        //  ColorCircle
        new SectionModel( typeof(ColorCircleView), "ColorCircle",
            "The ColorCircle is used internally and demonstrates features common to all ColorPickers and Sliders." )

        //  Add more controls here
    };
}