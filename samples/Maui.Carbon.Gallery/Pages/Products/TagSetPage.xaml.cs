using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Tag set", GalleryCategory.Products, Order = 130, Spec = "TagSet",
    Summary = "A single row of tags that shows as many as fit and collapses the rest into a \"+N\" overflow tag; the overflow opens a popover listing the hidden tags.")]
public partial class TagSetPage : GalleryPage
{
    static readonly (string Text, CdsTagType Type)[] Pool =
    [
        ("Region: us-south", CdsTagType.Blue),
        ("Status: running", CdsTagType.Green),
        ("Runtime: Node.js", CdsTagType.Purple),
        ("Owner: platform", CdsTagType.Gray),
        ("Tier: gold", CdsTagType.Teal),
        ("Zone: dal10", CdsTagType.Cyan),
        ("Env: staging", CdsTagType.Magenta),
        ("Plan: standard", CdsTagType.CoolGray),
    ];

    int _added;

    public TagSetPage()
    {
        InitializeComponent();
        Reset();
    }

    void OnAddClicked(object? sender, EventArgs e) => Add();

    void OnResetClicked(object? sender, EventArgs e) => Reset();

    void Reset()
    {
        Filters.Items.Clear();
        _added = 0;
        for (var i = 0; i < 5; i++) Add();
    }

    void Add()
    {
        var (text, type) = Pool[_added % Pool.Length];
        var item = new CdsTagSetItem(_added < Pool.Length ? text : $"{text} ({_added / Pool.Length + 1})", type)
        {
            IsDismissible = true,
            DismissLabel = "Remove filter",
        };
        item.Dismissed += (_, _) => Filters.Items.Remove(item);
        Filters.Items.Add(item);
        _added++;
    }

    void OnOverflowChanged(object? sender, EventArgs e) =>
        Status.Text = $"{Filters.OverflowItems.Count} of {Filters.Items.Count} filters hidden";

    void OnCustomOverflowClicked(object? sender, CdsTagSetOverflowEventArgs e)
    {
        e.Handled = true;
        CustomStatus.Text = "Hidden: " + string.Join(", ", e.OverflowItems.Select(t => t.Text));
    }
}
