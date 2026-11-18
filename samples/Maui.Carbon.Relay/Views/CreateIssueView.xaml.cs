using System.ComponentModel;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The create-issue wizard. Bindings carry the values; this code-behind adapts the progress indicator's step clicks, the
/// labels multi-select, field blur (validation shows after a field is left), the progress steps' invalid marks and the
/// review list rows (structured list rows take views, not a template).
/// </summary>
public partial class CreateIssueView : ContentView
{
    CreateIssueViewModel? _vm;

    public CreateIssueView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        SummaryInput.FocusLost += (_, _) => _vm?.TouchField("title");
        DescriptionInput.FocusLost += (_, _) => _vm?.TouchField("description");
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null) _vm.PropertyChanged -= OnViewModelChanged;
        _vm = BindingContext as CreateIssueViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        ReminderPicker.TimeZones.Clear();
        foreach (var zone in _vm.TimeZones) ReminderPicker.TimeZones.Add(zone);
        ReminderPicker.SelectedTimeZone = _vm.ReminderTimeZone;
        SyncInvalidSteps();
        BuildReview();
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CreateIssueViewModel.InvalidSteps):
                SyncInvalidSteps();
                break;
            case nameof(CreateIssueViewModel.ReviewRows):
                BuildReview();
                break;
            case nameof(CreateIssueViewModel.StepIndex):
                _ = Scroller.ScrollToAsync(0, 0, false);
                break;
        }
    }

    void SyncInvalidSteps()
    {
        if (_vm is null) return;
        for (var i = 0; i < Progress.Steps.Count && i < _vm.InvalidSteps.Count; i++)
        {
            var step = Progress.Steps[i];
            step.IsInvalid = _vm.InvalidSteps[i] && i != _vm.StepIndex;
        }
    }

    /// <summary>Only completed steps can be clicked; the view model decides and moves the indicator itself.</summary>
    void OnStepRequested(object? sender, CdsProgressStepRequestedEventArgs e)
    {
        e.Cancel = true;
        _vm?.GoToStep(e.Index);
    }

    void OnLabelsChanged(object? sender, CdsListBoxSelectionChangedEventArgs e) =>
        _vm?.SetLabels(e.SelectedItems.OfType<string>());

    void BuildReview()
    {
        ReviewList.Rows.Clear();
        if (_vm is null) return;
        foreach (var row in _vm.ReviewRows)
        {
            var edit = new CdsLink { Text = "Edit", VerticalOptions = LayoutOptions.Start };
            SemanticProperties.SetDescription(edit, $"Edit {row.Section.ToLowerInvariant()}");
            edit.Clicked += (_, _) => _vm?.EditSectionCommand.Execute(row);
            var item = new CdsStructuredListRow { Label = row.Section };
            item.Cells.Add(row.Section);
            item.Cells.Add(row.Details);
            item.Cells.Add(edit);
            ReviewList.Rows.Add(item);
        }
    }
}
