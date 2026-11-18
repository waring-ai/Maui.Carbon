using System.ComponentModel;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The dashboard. Bindings carry the values; this code-behind fills the "My work" structured list (its rows are
/// declarative cells, not an items source) and adapts list selection, item clicks and the notice's close button to
/// view model commands.
/// </summary>
public partial class DashboardView : ContentView
{
    DashboardViewModel? _vm;
    bool _building;

    public DashboardView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Root, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null) _vm.PropertyChanged -= OnViewModelChanged;
        _vm = BindingContext as DashboardViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        BuildMyWork();
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardViewModel.MyWork)) BuildMyWork();
    }

    /// <summary>One row per issue: key, summary, priority as a shape indicator with its name, and the due date.</summary>
    void BuildMyWork()
    {
        if (_vm is null) return;
        _building = true;
        try
        {
            MyWorkList.SelectedIndex = -1;
            MyWorkList.Rows.Clear();
            foreach (var issue in _vm.MyWork)
            {
                var row = new CdsStructuredListRow { Id = issue.Key, Label = $"Open {issue.Key}" };
                row.Cells.Add(issue.Key);
                row.Cells.Add(issue.Title);
                row.Cells.Add(new CdsShapeIndicator { Kind = CarbonMap.Shape(issue.Priority), Label = issue.PriorityText, TextSize = 14 });
                row.Cells.Add(issue.IsOverdue ? $"{issue.DueText} (overdue)" : issue.DueText);
                MyWorkList.Rows.Add(row);
            }
        }
        finally
        {
            _building = false;
        }
    }

    void OnMyWorkSelected(object? sender, EventArgs e)
    {
        if (_building || _vm is null || MyWorkList.SelectedItem?.Id is not { } key) return;
        _vm.OpenIssueCommand.Execute(key);
    }

    void OnActivityClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: ActivityRow row }) _vm?.OpenIssueCommand.Execute(row.IssueKey);
    }

    /// <summary>The view model hides the notice (and its wrapper), so it can show again for the next sprint.</summary>
    void OnNoticeClosed(object? sender, CdsNotificationCloseEventArgs e)
    {
        e.Cancel = true;
        _vm?.DismissSprintNoticeCommand.Execute(null);
    }
}
