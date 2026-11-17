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
    }

    void OnValueChanged(object? sender, CdsSliderChangedEventArgs e) => Status.Text = $"{e.Value} GB";
}
