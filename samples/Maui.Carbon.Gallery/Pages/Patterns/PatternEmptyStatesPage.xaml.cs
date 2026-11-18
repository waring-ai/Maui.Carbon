using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Patterns.Data;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Empty states", GalleryCategory.Patterns, Order = 50, Spec = "PatternEmptyStates",
    Summary = "Empty states for four situations: first use, no search results, data that failed to load, and no permission; each explains why the area is empty and gives the next step.")]
public partial class PatternEmptyStatesPage : GalleryPage
{
    // The table gives the block 16 at the sides and 48 above and below; add 32 to reach 48 all round.
    static readonly Thickness s_tablePadding = new(CdsSpacing.Spacing07, 0);
    static readonly Thickness s_tilePadding = new(0, CdsSpacing.Spacing05, 0, CdsSpacing.Spacing07);

    public PatternEmptyStatesPage()
    {
        InitializeComponent();

        BuildFirstUse();
        BuildNoResults();
        BuildTiles();
    }

    void BuildFirstUse()
    {
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", Width = new GridLength(220) });
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "location", Header = "Location", Width = new GridLength(160) });
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "class", Header = "Storage class", Width = new GridLength(160) });
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "size", Header = "Size", Width = GridLength.Star, Align = CdsTableAlignment.End });
        BucketTable.ItemsSource = Array.Empty<object>();

        var create = new CdsButton { Text = "Create bucket", Icon = CdsIconName.Add };
        create.Clicked += (_, _) => Status.Text = "Opening the create bucket form.";
        var learn = new CdsLink { Text = "Learn about buckets" };
        learn.Clicked += (_, _) => Status.Text = "Opening the bucket documentation.";

        BucketTable.EmptyContent = new K1EmptyState(
            CdsIconName.Folder,
            "Create your first bucket",
            "Buckets hold objects such as backups, logs and media files. You choose the location and storage class when you create one.",
            [create, learn],
            s_tablePadding);
    }

    void BuildNoResults()
    {
        K1Clusters.AddColumns(SearchTable, created: false);
        SearchTable.ItemsSource = K1Clusters.All.Take(2).ToList();

        var clear = new CdsButton { Text = "Clear search", Kind = CdsButtonKind.Tertiary };
        clear.Clicked += (_, _) =>
        {
            SearchTable.SearchText = string.Empty;
            Status.Text = "Search cleared. All clusters are back.";
        };

        SearchTable.EmptyContent = new K1EmptyState(
            CdsIconName.Search,
            "No clusters match “staging-ap”",
            "Check the spelling or search by region or version instead. Clear the search to see all 23 clusters.",
            [clear],
            s_tablePadding);
    }

    void BuildTiles()
    {
        var retry = new CdsButton { Text = "Try again", Kind = CdsButtonKind.Tertiary, Size = CdsSize.Md, Icon = CdsIconName.Restart };
        retry.Clicked += (_, _) => Status.Text = "Retrying the usage request.";
        var health = new CdsLink { Text = "Service status", Icon = CdsIconName.Launch };
        health.Clicked += (_, _) => Status.Text = "Opening the service status page in a browser.";
        ErrorHost.Content = new K1EmptyState(
            CdsIconName.WarningAlt,
            "Usage data didn’t load",
            "The billing service didn’t respond. Try again in a moment. If it keeps failing, check the service status page.",
            [retry, health],
            s_tilePadding);

        var request = new CdsButton { Text = "Request access", Kind = CdsButtonKind.Secondary, Size = CdsSize.Md };
        request.Clicked += (_, _) => Status.Text = "Access request sent to the account administrators.";
        var groups = new CdsLink { Text = "About access groups" };
        groups.Clicked += (_, _) => Status.Text = "Opening the access groups documentation.";
        PermissionHost.Content = new K1EmptyState(
            CdsIconName.ViewOff,
            "You don’t have access to API keys",
            "Viewing API keys needs the Key manager role. Ask an account administrator to add you to the Key managers access group.",
            [request, groups],
            s_tilePadding);
    }
}
