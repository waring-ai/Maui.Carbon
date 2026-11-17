using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Checkbox group", GalleryCategory.Forms, Order = 31, Spec = "CheckboxGroup",
    Summary = "Groups checkboxes under one legend with a helper, invalid or warning message.")]
public partial class CheckboxGroupPage : GalleryPage
{
    public CheckboxGroupPage()
    {
        InitializeComponent();
    }

    void OnChannelChanged(object? sender, CheckedChangedEventArgs e)
    {
        var chosen = new[] { EmailBox, SlackBox }.Where(c => c.IsChecked).Select(c => c.Label).ToList();
        Channels.IsInvalid = chosen.Count == 0;
        Status.Text = chosen.Count == 0 ? "Nothing selected" : string.Join(", ", chosen);
    }
}
