using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("Breadcrumb", GalleryCategory.Navigation, Order = 20,
    Summary = "Shows where the current page sits in a hierarchy as a row of links separated by slashes.")]
public partial class BreadcrumbPage : GalleryPage
{
    public BreadcrumbPage()
    {
        InitializeComponent();

        string[] levels = ["Dashboard", "Kubernetes", "Clusters", "prod-us-south", "Worker pools", "default"];
        foreach (var level in levels) LongTrail.Items.Add(new CdsBreadcrumbItem(level, "https://example.com/" + level.ToLowerInvariant()));

        foreach (var level in new[] { "Kubernetes", "Clusters", "prod-us-south" })
            OverflowCrumb.OverflowItems.Add(new CdsBreadcrumbItem(level, "https://example.com/" + level.ToLowerInvariant()));

        var overflow = new CdsBreadcrumbItem();
        foreach (var level in new[] { "Kubernetes", "Clusters" })
        {
            var item = new CdsBreadcrumbItem { Text = level };
            item.Clicked += (_, _) => Status.Text = $"Chose {level} from the overflow list";
            overflow.OverflowItems.Add(item);
        }
        InteractiveTrail.Items.Add(new CdsBreadcrumbItem("Dashboard", "https://example.com/"));
        InteractiveTrail.Items.Add(overflow);
        var pools = new CdsBreadcrumbItem { Text = "Worker pools" };
        pools.Clicked += (_, _) => Status.Text = "Activated Worker pools";
        InteractiveTrail.Items.Add(pools);
    }
}
