using ColorPicker.Gallery.Models;
using System.Windows.Input;

namespace ColorPicker.Gallery.Views.Base;

public class BasePage : ContentPage
{
    public ICommand NavigateCommand { get; }

    SectionModel? _selectedItem;
    public SectionModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public BasePage()
    {
        NavigateCommand = new Command( async () =>
        {
            if ( SelectedItem != null )
            {
                await Navigation.PushAsync( PreparePage( SelectedItem ) );

                SelectedItem = null;
            }
        } );
    }

    Page PreparePage( SectionModel model )
    {
        var page    = (Handler?.MauiContext?.Services?.GetService( model.Type ) as Page) ?? (Page)Activator.CreateInstance( model.Type )!;
        page.Title  = model.Title;

        return page;
    }
}