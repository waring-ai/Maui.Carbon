using System.ComponentModel;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The team table and detail panel. Bindings carry the values; this code-behind builds the table columns, mirrors the
/// view model's selected person into the table's radio selection (rows are rebuilt on every refresh) and adapts the
/// table's selection and the issue links' clicks.
/// </summary>
public partial class TeamView : ContentView
{
    TeamViewModel? _vm;
    bool _syncing;

    public TeamView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        TeamColumns.Add(Table);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null) _vm.PropertyChanged -= OnViewModelChanged;
        _vm = BindingContext as TeamViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        PushSelection();
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TeamViewModel.Selected) or nameof(TeamViewModel.Rows)) PushSelection();
    }

    void PushSelection()
    {
        if (_vm is null) return;
        var current = Table.SelectedRows?.Cast<TeamMemberRow>().FirstOrDefault()?.Id;
        if (current == _vm.Selected?.Id && _vm.Selected is not null) return;
        _syncing = true;
        try
        {
            Table.SelectedRows = _vm.Selected is { } row ? new List<object> { row } : new List<object>();
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnSelectionChanged(object? sender, CdsDataTableRowsEventArgs e)
    {
        if (_syncing || _vm is null) return;
        if (e.Rows.Cast<TeamMemberRow>().FirstOrDefault() is { } row) _vm.Select(row.Id);
    }

    void OnIssueClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: TeamIssueLink link }) _vm?.OpenIssueCommand.Execute(link);
    }
}
