using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Select", GalleryCategory.Forms, Order = 70, Spec = "Select",
    Summary = "A native select styled as a Carbon field for choosing one option from a list in a form.")]
public partial class SelectPage : GalleryPage
{
    static readonly string[] s_sizes = ["Small (2 vCPU)", "Medium (4 vCPU)", "Large (8 vCPU)"];

    public SelectPage()
    {
        InitializeComponent();
        foreach (var select in new[] { SmallSelect, MediumSelect, LargeSelect, WarnSelect, ReadOnlySelect, DisabledSelect, InlineSelect, Sizes })
        {
            select.ItemsSource = s_sizes;
        }
        SmallSelect.SelectedIndex = 0;
        MediumSelect.SelectedIndex = 1;
        LargeSelect.SelectedIndex = 2;
        WarnSelect.SelectedIndex = 0;
        ReadOnlySelect.SelectedIndex = 1;
        DisabledSelect.SelectedIndex = 2;
        InlineSelect.SelectedIndex = 0;
        InlineInvalidSelect.ItemsSource = new[] { "Name", "Created", "Region" };
        InvalidSelect.ItemsSource = new[] { "Dallas (us-south)", "Frankfurt (eu-de)" };
        Sizes.ItemToString = item => $"{item}";
    }

    void OnSelectedItemChanged(object? sender, SelectedItemChangedEventArgs e) =>
        Status.Text = e.SelectedItemIndex < 0 ? "Nothing chosen" : $"{e.SelectedItem} (index {e.SelectedItemIndex})";
}
