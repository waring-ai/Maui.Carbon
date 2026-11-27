using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Checklist", GalleryCategory.Products, Order = 4, Spec = "Checklist",
    Summary = "An onboarding checklist that tracks a person's progress through a set of tasks.")]
public partial class ChecklistPage : GalleryPage
{
    public ChecklistPage()
    {
        InitializeComponent();
    }

    void OnTaskClicked(object? sender, EventArgs e) =>
        Status.Text = sender is CdsChecklistTask task ? $"Opened {task.Label}" : "Task";

    void OnViewAll(object? sender, EventArgs e) => Status.Text = "View all";

    void OnOpenChanged(object? sender, CdsChoiceChangedEventArgs<bool> e) => Status.Text = e.NewValue ? "Open" : "Closed";
}
