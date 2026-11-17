using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Number input", GalleryCategory.Forms, Order = 50, Spec = "NumberInput",
    Summary = "A numeric field with decrement and increment steppers, constrained by min, max and step.")]
public partial class NumberInputPage : GalleryPage
{
    public NumberInputPage()
    {
        InitializeComponent();
    }

    void OnValueChanged(object? sender, CdsNumberChangedEventArgs e) =>
        Status.Text = e.Value is { } v ? $"Value is {v} ({e.Direction})" : "Value is empty";
}
