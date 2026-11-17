using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Dismissible tag", GalleryCategory.Feedback, Order = 51, Spec = "DismissibleTag",
    Summary = "A tag with a close button, used for applied filters or chosen values that the user can remove.")]
public partial class DismissibleTagPage : GalleryPage
{
    public DismissibleTagPage()
    {
        InitializeComponent();
    }

    static readonly (string Text, CdsTagType Type)[] s_filters =
    [
        ("Region: us-east", CdsTagType.Blue),
        ("Status: running", CdsTagType.Green),
        ("Owner: platform", CdsTagType.Purple),
    ];

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Filters.Children.Count == 0) Populate();
    }

    void Populate()
    {
        Filters.Children.Clear();
        foreach (var (text, type) in s_filters)
        {
            var tag = new CdsDismissibleTag(text, type) { DismissLabel = "Remove filter", Margin = new Thickness(0, 0, 8, 8) };
            tag.Dismissed += (_, _) =>
            {
                Filters.Children.Remove(tag);
                Status.Text = $"{Filters.Children.Count} filters";
            };
            Filters.Children.Add(tag);
        }
        Status.Text = $"{Filters.Children.Count} filters";
    }

    void OnReset(object? sender, EventArgs e) => Populate();
}
