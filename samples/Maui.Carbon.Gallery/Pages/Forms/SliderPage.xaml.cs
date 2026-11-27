using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Slider", GalleryCategory.Forms, Order = 34, Spec = "Slider",
    Summary = "A draggable thumb on a track for choosing a number or a range, with an editable number box.")]
public partial class SliderPage : GalleryPage
{
    public SliderPage()
    {
        InitializeComponent();
        Sampling.ValueFormatter = v => $"{v:P0}";
        Tooltipped.ValueFormatter = v => $"{v} GB";
        // Show the tooltip without interaction: keyboard focus opens it, so give the first slider focus once it is on screen.
        Tooltipped.Loaded += (_, _) => Tooltipped.Dispatcher.Dispatch(() => Tooltipped.Focus());
    }

    void OnValueChanged(object? sender, CdsSliderChangedEventArgs e) => Status.Text = $"{e.Value} GB";
}
