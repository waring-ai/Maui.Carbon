using System.ComponentModel;
using System.Globalization;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The reports screen. Bindings carry the filters, chart and tiles; this code-behind fills the static workload table
/// (CdsTable rows are declarative cells, not an items source) whenever the view model's workload changes.
/// </summary>
public partial class ReportsView : ContentView
{
    ReportsViewModel? _vm;

    public ReportsView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Root, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null) _vm.PropertyChanged -= OnViewModelChanged;
        _vm = BindingContext as ReportsViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        BuildWorkload();
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ReportsViewModel.Workload)) BuildWorkload();
    }

    void BuildWorkload()
    {
        if (_vm is null) return;
        WorkloadTable.Rows.Clear();
        foreach (var w in _vm.Workload)
        {
            var load = new CdsIconIndicator { Kind = ReportsConverters.Indicator(w.Level), Label = w.LevelText, VerticalOptions = LayoutOptions.Center };
            var row = new CdsTableRow();
            row.Cells.Add(new CdsTableCell(w.Name));
            row.Cells.Add(new CdsTableCell(w.Role));
            row.Cells.Add(new CdsTableCell(w.Open.ToString(CultureInfo.InvariantCulture)));
            row.Cells.Add(new CdsTableCell(w.Points.ToString(CultureInfo.InvariantCulture)));
            row.Cells.Add(new CdsTableCell(w.ShareText));
            row.Cells.Add(new CdsTableCell { Body = load });
            WorkloadTable.Rows.Add(row);
        }
    }
}
