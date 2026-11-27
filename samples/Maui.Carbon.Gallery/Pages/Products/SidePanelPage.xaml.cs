using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Side panel", GalleryCategory.Products, Order = 10, Spec = "SidePanel",
    Summary = "A full-height panel that slides in from the edge of the page to create, edit or view details without leaving it.")]
public partial class SidePanelPage : GalleryPage
{
    public SidePanelPage()
    {
        InitializeComponent();
        FlavorDropdown.ItemsSource = new[] { "bx2.4x16", "bx2.8x32", "cx2.16x32" };
        FlavorDropdown.SelectedItem = "bx2.4x16";
        RegionDropdown.ItemsSource = new[] { "All regions", "Frankfurt", "London", "Dallas" };
        RegionDropdown.SelectedItem = "All regions";
        SlidePanel.PageContent = PageArea;
        LogSnippet.Code = string.Join("\n", Enumerable.Range(0, 60).Select(i =>
            $"2026-11-27T10:{i:00}:12Z worker-{i % 9} kubelet: pod checkout-api-{i:000} ready"));
    }

    void OnEditClicked(object? sender, EventArgs e) => EditPanel.IsOpen = true;

    void OnStartClicked(object? sender, EventArgs e) => StartPanel.IsOpen = !StartPanel.IsOpen;

    void OnOverlayClicked(object? sender, EventArgs e) => OverlayPanel.IsOpen = true;

    void OnSlideInClicked(object? sender, EventArgs e) => SlidePanel.IsOpen = !SlidePanel.IsOpen;

    void OnToolbarClicked(object? sender, EventArgs e) => ToolbarPanel.IsOpen = true;

    void OnEditClosing(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Edit closed without saving";

    void OnEditCancel(object? sender, EventArgs e) => EditPanel.IsOpen = false;

    void OnEditSave(object? sender, EventArgs e)
    {
        EditPanel.IsOpen = false;
        Status.Text = "Cluster saved";
    }

    void OnFiltersApply(object? sender, EventArgs e)
    {
        OverlayPanel.IsOpen = false;
        Status.Text = "Filters applied";
    }

    void OnFiltersReset(object? sender, EventArgs e) => Status.Text = "Filters reset";

    void OnStepsClicked(object? sender, EventArgs e)
    {
        SetStep(0);
        StepsPanel.IsOpen = true;
    }

    void SetStep(int step)
    {
        StepsPanel.CurrentStep = step;
        StepsPanel.Label = $"Step {step + 1} of 2";
        RuleName.IsVisible = step == 0;
        RuleDetail.IsVisible = step == 1;
        StepsNext.Text = step == 0 ? "Next" : "Create rule";
    }

    void OnStepsBack(object? sender, EventArgs e) => SetStep(0);

    void OnStepsCancel(object? sender, EventArgs e) => StepsPanel.IsOpen = false;

    void OnStepsNext(object? sender, EventArgs e)
    {
        if (StepsPanel.CurrentStep == 0)
        {
            SetStep(1);
            return;
        }
        StepsPanel.IsOpen = false;
        Status.Text = "Rule created";
    }

    void OnToolbarAction(object? sender, EventArgs e) =>
        Status.Text = sender is CdsButton b ? $"{b.Text ?? b.IconDescription} selected" : Status.Text;

    void OnSizeClicked(object? sender, EventArgs e)
    {
        if (sender is not CdsButton { CommandParameter: string name } || !Enum.TryParse<CdsSidePanelSize>(name, out var size)) return;
        SizePanel.Size = size;
        SizeText.Text = $"This panel is {name.ToLowerInvariant()}.";
        SizePanel.IsOpen = true;
    }

    void OnSizeClose(object? sender, EventArgs e) => SizePanel.IsOpen = false;
}
