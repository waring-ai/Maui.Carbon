using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Patterns.Data;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Overflow content pattern", GalleryCategory.Patterns, Order = 73, Spec = "PatternOverflowContent",
    Summary = "Handling content that doesn't fit: end, front and mid-line truncation with the full text still reachable, truncated table cells with tooltips, an overflow menu for extra actions, a +n tag for extra labels, Show more for long text and Load more for long lists.")]
public partial class PatternOverflowContentPage : GalleryPage
{
    const string ObjectPath = "backups/prod-eu-01/2026-11-17/etcd-snapshot-full-0200.tar.gz";
    const string PodPath = "namespaces/payments/deployments/checkout-api/pods/checkout-api-7d9f8c6b5-x2k4q";
    const string Crn = "crn:v1:public:containers-kubernetes:eu-de:a/a7f3c9e2b41d4e8f:c7k2m9d40f8e1a3b6c55::";
    const int Total = 24;

    int _shown = 4;
    bool _expanded;

    public PatternOverflowContentPage()
    {
        InitializeComponent();
        SetupTruncation();
        SetupTable();
        RefreshEvents();
    }

    void SetupTruncation()
    {
        EndText.Text = ObjectPath;
        EndTip.Text = ObjectPath;
        EndFull.Text = "Full text in the tooltip: " + ObjectPath;
        FrontText.Text = PodPath;
        FrontTip.Text = PodPath;
        FrontFull.Text = "Full text in the tooltip: " + PodPath;
        MidText.Text = K2Data.MidTruncate(Crn, 12, 12);
        MidCopy.Text = Crn;
    }

    void SetupTable()
    {
        DeploymentTable.RowKeySelector = r => ((K2Deployment)r).Name;
        DeploymentTable.Headers.Add(new CdsDataTableHeader
        {
            Key = "name", Header = "Deployment", Width = new GridLength(224), IsSortable = false,
            ValueSelector = r => ((K2Deployment)r).Name,
            CellTemplate = new DataTemplate(() =>
            {
                var label = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Link, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, WidthRequest = 192 };
                label.SetBinding(Label.TextProperty, static (K2Deployment d) => d.Name, mode: BindingMode.OneTime);
                var tip = new CdsTooltip { Placement = CdsPlacement.BottomStart, HorizontalOptions = LayoutOptions.Start, Body = label };
                tip.SetBinding(CdsTooltip.TextProperty, static (K2Deployment d) => d.Name, mode: BindingMode.OneTime);
                return tip;
            }),
        });
        DeploymentTable.Headers.Add(new CdsDataTableHeader
        {
            Key = "desc", Header = "Description", Width = new GridLength(360), IsSortable = false,
            ValueSelector = r => ((K2Deployment)r).Description,
            CellTemplate = new DataTemplate(() =>
            {
                var label = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, WidthRequest = 328 };
                label.SetBinding(Label.TextProperty, static (K2Deployment d) => d.Description, mode: BindingMode.OneTime);
                return label;
            }),
        });
        DeploymentTable.Headers.Add(new CdsDataTableHeader
        {
            Key = "updated", Header = "Updated", Width = new GridLength(140), IsSortable = false,
            ValueSelector = r => ((K2Deployment)r).Updated,
        });
        DeploymentTable.RowActions.Add(new CdsDataTableRowAction { Label = "View logs" });
        DeploymentTable.RowActions.Add(new CdsDataTableRowAction { Label = "Roll back" });
        DeploymentTable.RowActions.Add(new CdsDataTableRowAction { Label = "Delete deployment", Kind = CdsMenuItemKind.Danger });
        DeploymentTable.ItemsSource = K2Data.Deployments;
    }

    void OnMoreTagClicked(object? sender, EventArgs e)
    {
        MoreLabels.IsOpen = !MoreLabels.IsOpen;
        MoreTag.IsExpanded = MoreLabels.IsOpen;
    }

    void OnShowMoreClicked(object? sender, EventArgs e)
    {
        _expanded = !_expanded;
        AboutText.MaxLines = _expanded ? int.MaxValue : 3;
        AboutText.LineBreakMode = _expanded ? LineBreakMode.WordWrap : LineBreakMode.TailTruncation;
        ShowMoreButton.Text = _expanded ? "Show less" : "Show more";
    }

    void OnLoadMoreClicked(object? sender, EventArgs e)
    {
        _shown = Math.Min(_shown + 3, K2Data.Events.Count);
        RefreshEvents();
    }

    void RefreshEvents()
    {
        EventsList.Items.Clear();
        foreach (var text in K2Data.Events.Take(_shown))
            EventsList.Items.Add(new CdsContainedListItem { Text = text });
        EventsCount.Text = $"Showing {_shown} of {Total}";
        LoadMoreButton.IsEnabled = _shown < K2Data.Events.Count;
    }
}
