using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Selectable tag", GalleryCategory.Feedback, Order = 53, Spec = "SelectableTag",
    Summary = "A toggle tag the user can switch on and off, used for picking several options from a set of chips.")]
public partial class SelectableTagPage : GalleryPage
{
    public SelectableTagPage()
    {
        InitializeComponent();
    }

    readonly List<string> _selected = ["eu-de"];

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Chips.Children.Count > 0) return;
        foreach (var region in new[] { "us-south", "us-east", "eu-de", "eu-gb", "jp-tok" })
        {
            var chip = new CdsSelectableTag(region, _selected.Contains(region)) { Margin = new Thickness(0, 0, 8, 8) };
            chip.SelectedChanged += (_, e) =>
            {
                if (e.Value) _selected.Add(region);
                else _selected.Remove(region);
                Status.Text = _selected.Count == 0 ? "Nothing selected" : "Selected: " + string.Join(", ", _selected);
            };
            Chips.Children.Add(chip);
        }
    }
}
