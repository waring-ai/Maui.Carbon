using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Checkbox", GalleryCategory.Forms, Order = 30, Spec = "Checkbox",
    Summary = "A 16px box for choosing zero, one or several independent options.")]
public partial class CheckboxPage : GalleryPage
{
    bool _syncing;

    public CheckboxPage()
    {
        InitializeComponent();
        OnChildChanged(null, new CheckedChangedEventArgs(true));
    }

    void OnEmailChanged(object? sender, CheckedChangedEventArgs e) => Status.Text = e.Value ? "Checked: you will get an email" : "Not checked";

    void OnAllChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        ChildA.IsChecked = ChildB.IsChecked = ChildC.IsChecked = e.Value;
        _syncing = false;
    }

    void OnChildChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        var count = new[] { ChildA, ChildB, ChildC }.Count(c => c.IsChecked);
        AllBox.IsIndeterminate = count > 0 && count < 3;
        AllBox.IsChecked = count == 3;
        _syncing = false;
    }
}
