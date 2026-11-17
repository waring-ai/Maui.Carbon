using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Radio button group", GalleryCategory.Forms, Order = 32, Spec = "RadioButtonGroup",
    Summary = "A set of radio buttons for choosing exactly one of a few visible options.")]
public partial class RadioButtonGroupPage : GalleryPage
{
    public RadioButtonGroupPage()
    {
        InitializeComponent();
    }

    void OnPlanChanged(object? sender, CdsChoiceChangedEventArgs<object> e) =>
        Status.Text = e.NewValue is null ? "Nothing selected" : $"Selected: {Plan.SelectedItem?.Label}";
}
