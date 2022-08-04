﻿namespace ColorPicker.Gallery;

public abstract class BaseGalleryViewModel : BaseViewModel
{
    public IReadOnlyList<SectionModel>? Items { get; }

    protected abstract IEnumerable<SectionModel>? CreateItems();

    public BaseGalleryViewModel()
    {
        var items = CreateItems();

        if (items is not null)
            Items = items.ToList();
    }
}