using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Add-select", GalleryCategory.Products, Order = 50, Spec = "AddSelect",
    Summary = "Choose one or several items from a long or nested list in a tearsheet.")]
public partial class AddSelectPage : GalleryPage
{
    public AddSelectPage()
    {
        InitializeComponent();
        string[][] people =
        [
            ["ada", "Ada Lovelace", "ada@example.com", "Engineering", "Admin"],
            ["alan", "Alan Turing", "alan@example.com", "Research", "Editor"],
            ["grace", "Grace Hopper", "grace@example.com", "Engineering", "Editor"],
            ["edsger", "Edsger Dijkstra", "edsger@example.com", "Research", "Viewer"],
            ["barbara", "Barbara Liskov", "barbara@example.com", "Engineering", "Viewer"],
            ["katherine", "Katherine Johnson", "katherine@example.com", "Operations", "Admin"],
        ];
        foreach (var p in people)
        {
            var item = new CdsAddSelectItem { Id = p[0], Title = p[1], Value = p[0], Subtitle = p[2], Icon = CdsIconName.User };
            item.Attributes["department"] = p[3];
            item.Attributes["role"] = p[4];
            item.Meta.Add(new CdsAddSelectMeta { Title = "Department", Value = p[3] });
            item.Meta.Add(new CdsAddSelectMeta { Title = "Current role", Value = p[4] });
            Multi.Items.Add(item);
        }
        Multi.GlobalFilters.Add(new CdsAddSelectFilter { Id = "department", Label = "Department" });
        Multi.GlobalFilters.Add(new CdsAddSelectFilter { Id = "role", Label = "Role" });
        Multi.GlobalSortBy.Add("title");
        var modifier = new CdsAddSelectModifier { Id = "access", Title = "Access", Label = "Choose access" };
        modifier.Options.Add("Viewer");
        modifier.Options.Add("Editor");
        modifier.Options.Add("Admin");
        Multi.Modifier = modifier;

        var regions = new (string Id, string Name, string[] Centers)[]
        {
            ("eu", "Europe", ["Frankfurt", "London", "Paris"]),
            ("us", "Americas", ["Dallas", "Washington DC", "São Paulo"]),
            ("ap", "Asia Pacific", ["Tokyo", "Sydney"]),
        };
        foreach (var r in regions)
        {
            var region = new CdsAddSelectItem { Id = r.Id, Title = r.Name, Value = r.Id, Subtitle = $"{r.Centers.Length} data centers", Icon = CdsIconName.Folder };
            foreach (var c in r.Centers)
                region.Children.Add(new CdsAddSelectItem { Id = $"{r.Id}-{c}", Title = c, Value = c });
            Single.Items.Add(region);
        }
    }

    void OnMultiClicked(object? sender, EventArgs e) => Multi.IsOpen = true;

    void OnSingleClicked(object? sender, EventArgs e) => Single.IsOpen = true;

    void OnMultiSubmitted(object? sender, CdsAddSelectSubmittedEventArgs e) =>
        Status.Text = "Added " + string.Join(", ", e.Items.Select(i => $"{i.Title} ({e.Modifiers[i.Id] ?? "no access"})"));

    void OnSingleSubmitted(object? sender, CdsAddSelectSubmittedEventArgs e) => Status.Text = $"Chose {e.Items[0].Title}";
}
