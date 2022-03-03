namespace ColorPickerTest;

public partial class App : Application
{
    public MauiContext?  MauiContext { get;}



	public App()
	{
		InitializeComponent();

        if ( Handler?.MauiContext is MauiContext context ) 
            MauiContext = context;

        MainPage = new MainPage();
	}
}
