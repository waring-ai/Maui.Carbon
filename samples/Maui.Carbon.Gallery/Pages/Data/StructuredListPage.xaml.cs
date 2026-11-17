using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Structured list", GalleryCategory.Data, Order = 40, Spec = "StructuredList",
    Summary = "Simple tabular information in rows and cells without table chrome; selectable lists pick one row.")]
public partial class StructuredListPage : GalleryPage
{
    public StructuredListPage()
    {
        InitializeComponent();
        foreach (var list in new[] { CondensedList, FlushList })
        {
            list.Columns.Add(new CdsStructuredListColumn { Header = "Property", Width = GridLength.Auto, IsNoWrap = true });
            list.Columns.Add(new CdsStructuredListColumn { Header = "Value" });
            list.Rows.Add(new CdsStructuredListRow("Cluster ID", "c7f2a91d-0b4e"));
            list.Rows.Add(new CdsStructuredListRow("Kubernetes version", "1.29.4"));
            list.Rows.Add(new CdsStructuredListRow("Ingress subdomain", "prod-us-east-1.containers"));
            list.Rows.Add(new CdsStructuredListRow("Created", "March 4, 2026"));
        }
    }

    void OnFlavorChanged(object? sender, EventArgs e) =>
        Status.Text = FlavorList.SelectedItem is { } row ? $"Selected: {row.Cells[0]}" : "Nothing selected";

    void OnPlanChanged(object? sender, EventArgs e) =>
        PlanStatus.Text = $"You can change the plan later from Billing. Selected: {PlanList.SelectedItem?.Cells[0]}.";
}
