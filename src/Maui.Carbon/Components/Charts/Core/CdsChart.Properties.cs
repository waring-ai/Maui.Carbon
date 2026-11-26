using System.Collections;
using System.Globalization;

namespace Maui.Carbon;

public abstract partial class CdsChart
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsChart), null, propertyChanged: static (b, _, _) => ((CdsChart)b).OnChromeChanged());

    /// <summary>Bindable property for <see cref="Data"/>.</summary>
    public static readonly BindableProperty DataProperty = BindableProperty.Create(
        nameof(Data), typeof(IEnumerable), typeof(CdsChart), null, propertyChanged: static (b, o, n) => ((CdsChart)b).OnDataChanged((IEnumerable?)o, (IEnumerable?)n));

    /// <summary>Bindable property for <see cref="IsLoading"/>.</summary>
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading), typeof(bool), typeof(CdsChart), false, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="IsLegendEnabled"/>.</summary>
    public static readonly BindableProperty IsLegendEnabledProperty = BindableProperty.Create(
        nameof(IsLegendEnabled), typeof(bool?), typeof(CdsChart), null, propertyChanged: static (b, _, _) => ((CdsChart)b).OnFrameChanged());

    /// <summary>Bindable property for <see cref="LegendPosition"/>.</summary>
    public static readonly BindableProperty LegendPositionProperty = BindableProperty.Create(
        nameof(LegendPosition), typeof(CdsChartLegendPosition), typeof(CdsChart), CdsChartLegendPosition.Bottom, propertyChanged: static (b, _, _) => ((CdsChart)b).OnFrameChanged());

    /// <summary>Bindable property for <see cref="LegendAlignment"/>.</summary>
    public static readonly BindableProperty LegendAlignmentProperty = BindableProperty.Create(
        nameof(LegendAlignment), typeof(CdsChartLegendAlignment), typeof(CdsChart), CdsChartLegendAlignment.Start, propertyChanged: static (b, _, _) => ((CdsChart)b).OnFrameChanged());

    /// <summary>Bindable property for <see cref="IsLegendClickable"/>.</summary>
    public static readonly BindableProperty IsLegendClickableProperty = BindableProperty.Create(
        nameof(IsLegendClickable), typeof(bool), typeof(CdsChart), true, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="LegendOrder"/>.</summary>
    public static readonly BindableProperty LegendOrderProperty = BindableProperty.Create(
        nameof(LegendOrder), typeof(IList<string>), typeof(CdsChart), null, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="SelectedGroups"/>.</summary>
    public static readonly BindableProperty SelectedGroupsProperty = BindableProperty.Create(
        nameof(SelectedGroups), typeof(IReadOnlyList<string>), typeof(CdsChart), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsChart)b).OnSelectedGroupsChanged((IReadOnlyList<string>?)n));

    /// <summary>Bindable property for <see cref="ColorPairingOption"/>.</summary>
    public static readonly BindableProperty ColorPairingOptionProperty = BindableProperty.Create(
        nameof(ColorPairingOption), typeof(int), typeof(CdsChart), 1, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="ColorScale"/>.</summary>
    public static readonly BindableProperty ColorScaleProperty = BindableProperty.Create(
        nameof(ColorScale), typeof(IDictionary<string, string>), typeof(CdsChart), null, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="IsToolbarEnabled"/>.</summary>
    public static readonly BindableProperty IsToolbarEnabledProperty = BindableProperty.Create(
        nameof(IsToolbarEnabled), typeof(bool?), typeof(CdsChart), null, propertyChanged: static (b, _, _) => ((CdsChart)b).OnChromeChanged());

    /// <summary>Bindable property for <see cref="ToolbarControls"/>.</summary>
    public static readonly BindableProperty ToolbarControlsProperty = BindableProperty.Create(
        nameof(ToolbarControls), typeof(CdsChartToolbarControls), typeof(CdsChart), CdsChartToolbarControls.All, propertyChanged: static (b, _, _) => ((CdsChart)b).OnChromeChanged());

    /// <summary>Bindable property for <see cref="ToolbarIconCount"/>.</summary>
    public static readonly BindableProperty ToolbarIconCountProperty = BindableProperty.Create(
        nameof(ToolbarIconCount), typeof(int), typeof(CdsChart), 3, propertyChanged: static (b, _, _) => ((CdsChart)b).OnChromeChanged());

    /// <summary>Bindable property for <see cref="IsTooltipEnabled"/>.</summary>
    public static readonly BindableProperty IsTooltipEnabledProperty = BindableProperty.Create(
        nameof(IsTooltipEnabled), typeof(bool), typeof(CdsChart), true);

    /// <summary>Bindable property for <see cref="ShowTooltipTotal"/>.</summary>
    public static readonly BindableProperty ShowTooltipTotalProperty = BindableProperty.Create(
        nameof(ShowTooltipTotal), typeof(bool), typeof(CdsChart), true, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="TooltipTotalLabel"/>.</summary>
    public static readonly BindableProperty TooltipTotalLabelProperty = BindableProperty.Create(
        nameof(TooltipTotalLabel), typeof(string), typeof(CdsChart), null, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="ValueFormat"/>.</summary>
    public static readonly BindableProperty ValueFormatProperty = BindableProperty.Create(
        nameof(ValueFormat), typeof(string), typeof(CdsChart), null, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="ValueFormatter"/>.</summary>
    public static readonly BindableProperty ValueFormatterProperty = BindableProperty.Create(
        nameof(ValueFormatter), typeof(Func<double, string>), typeof(CdsChart), null, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="Culture"/>.</summary>
    public static readonly BindableProperty CultureProperty = BindableProperty.Create(
        nameof(Culture), typeof(CultureInfo), typeof(CdsChart), null, propertyChanged: OnSceneChanged);

    /// <summary>Bindable property for <see cref="ExportFileName"/>.</summary>
    public static readonly BindableProperty ExportFileNameProperty = BindableProperty.Create(
        nameof(ExportFileName), typeof(string), typeof(CdsChart), "myChart");

    /// <summary>The chart title (Carbon's <c>title</c>), 16px semibold. Name the measure and unit; it can state the main insight. It also names the chart for screen readers.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The data (Carbon's <c>data</c>): <see cref="CdsChartPoint"/>s for most charts (see each chart for its item type).
    /// An <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> collection redraws the chart when it
    /// changes; the chart does not watch the items' own properties, so replace an item to change it.
    /// </summary>
    public IEnumerable? Data
    {
        get => (IEnumerable?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>Shows a shimmering skeleton while the data loads (Carbon's <c>data.loading</c>). Empty data shows the same skeleton, still.</summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Whether the legend shows; <c>null</c> (default) follows the chart (on for series charts, off for meters and gauges). A single series needs no legend.</summary>
    public bool? IsLegendEnabled
    {
        get => (bool?)GetValue(IsLegendEnabledProperty);
        set => SetValue(IsLegendEnabledProperty, value);
    }

    /// <summary>Where the legend sits. Default <see cref="CdsChartLegendPosition.Bottom"/>; keep the same position for every chart on a dashboard.</summary>
    public CdsChartLegendPosition LegendPosition
    {
        get => (CdsChartLegendPosition)GetValue(LegendPositionProperty);
        set => SetValue(LegendPositionProperty, value);
    }

    /// <summary>How a top or bottom legend aligns its items. Default <see cref="CdsChartLegendAlignment.Start"/>.</summary>
    public CdsChartLegendAlignment LegendAlignment
    {
        get => (CdsChartLegendAlignment)GetValue(LegendAlignmentProperty);
        set => SetValue(LegendAlignmentProperty, value);
    }

    /// <summary>Whether clicking legend items isolates series. Default <c>true</c>.</summary>
    public bool IsLegendClickable
    {
        get => (bool)GetValue(IsLegendClickableProperty);
        set => SetValue(IsLegendClickableProperty, value);
    }

    /// <summary>The order of legend items by group name (Carbon's <c>legend.order</c>); groups not listed follow in data order. Colors still follow data order.</summary>
    public IList<string>? LegendOrder
    {
        get => (IList<string>?)GetValue(LegendOrderProperty);
        set => SetValue(LegendOrderProperty, value);
    }

    /// <summary>
    /// The isolated series (Carbon's <c>data.selectedGroups</c>); empty or <c>null</c> shows all. Two-way: clicking a legend
    /// item isolates it, clicking more adds them, clicking the last active item resets.
    /// </summary>
    public IReadOnlyList<string>? SelectedGroups
    {
        get => (IReadOnlyList<string>?)GetValue(SelectedGroupsProperty);
        set => SetValue(SelectedGroupsProperty, value);
    }

    /// <summary>
    /// Which of Carbon's curated pairings colors one to five series (Carbon's <c>color.pairing.option</c>; 1-group charts
    /// have 4 options, 2 and 3 groups 5, 4 groups 3, 5 groups 2). Six or more series always use <c>data-vis-01</c> to 14. Default 1.
    /// </summary>
    public int ColorPairingOption
    {
        get => (int)GetValue(ColorPairingOptionProperty);
        set => SetValue(ColorPairingOptionProperty, value);
    }

    /// <summary>
    /// Pins series colors by group name to color keys (<see cref="CdsColorKeys.DataVis01"/>…), Carbon's <c>color.scale</c>:
    /// give a dataset the same color in every chart of a dashboard. Unlisted groups keep their pairing colors.
    /// </summary>
    public IDictionary<string, string>? ColorScale
    {
        get => (IDictionary<string, string>?)GetValue(ColorScaleProperty);
        set => SetValue(ColorScaleProperty, value);
    }

    /// <summary>Whether the toolbar shows; <c>null</c> (default) follows the chart (on, except for meters).</summary>
    public bool? IsToolbarEnabled
    {
        get => (bool?)GetValue(IsToolbarEnabledProperty);
        set => SetValue(IsToolbarEnabledProperty, value);
    }

    /// <summary>The toolbar's controls. Default all: show as table, fullscreen, and CSV, PNG and JPG exports.</summary>
    public CdsChartToolbarControls ToolbarControls
    {
        get => (CdsChartToolbarControls)GetValue(ToolbarControlsProperty);
        set => SetValue(ToolbarControlsProperty, value);
    }

    /// <summary>How many toolbar slots show (Carbon's <c>numberOfIcons</c>): this many minus one icon buttons, then the overflow menu. Default 3.</summary>
    public int ToolbarIconCount
    {
        get => (int)GetValue(ToolbarIconCountProperty);
        set => SetValue(ToolbarIconCountProperty, value);
    }

    /// <summary>Whether hovering, tapping or moving the data cursor shows tooltips. Default <c>true</c>.</summary>
    public bool IsTooltipEnabled
    {
        get => (bool)GetValue(IsTooltipEnabledProperty);
        set => SetValue(IsTooltipEnabledProperty, value);
    }

    /// <summary>Whether multi-series tooltips end with a bold total. Default <c>true</c>.</summary>
    public bool ShowTooltipTotal
    {
        get => (bool)GetValue(ShowTooltipTotalProperty);
        set => SetValue(ShowTooltipTotalProperty, value);
    }

    /// <summary>The label of the tooltip total. Default "Total".</summary>
    public string? TooltipTotalLabel
    {
        get => (string?)GetValue(TooltipTotalLabelProperty);
        set => SetValue(TooltipTotalLabelProperty, value);
    }

    /// <summary>A .NET format string for values in tooltips and the table ("N0", "C0", "0.0 'GB'"); default: grouped, up to three decimals.</summary>
    public string? ValueFormat
    {
        get => (string?)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    /// <summary>Formats values in tooltips and the table (Carbon's <c>tooltip.valueFormatter</c>); wins over <see cref="ValueFormat"/>.</summary>
    public Func<double, string>? ValueFormatter
    {
        get => (Func<double, string>?)GetValue(ValueFormatterProperty);
        set => SetValue(ValueFormatterProperty, value);
    }

    /// <summary>The culture of numbers and dates (Carbon's <c>locale</c>); default the current culture.</summary>
    public CultureInfo? Culture
    {
        get => (CultureInfo?)GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    /// <summary>The file name (without extension) of exports. Default "myChart" (Carbon's <c>fileDownload.fileName</c>).</summary>
    public string? ExportFileName
    {
        get => (string?)GetValue(ExportFileNameProperty);
        set => SetValue(ExportFileNameProperty, value);
    }

    static void OnSceneChanged(BindableObject b, object? o, object? n) => ((CdsChart)b).RequestRebuild();
}

/// <summary>A chart export about to be shared: set <see cref="Handled"/> to save it yourself.</summary>
public sealed class CdsChartExportingEventArgs : EventArgs
{
    internal CdsChartExportingEventArgs(CdsChartExportFormat format, string fileName, byte[] content)
    {
        Format = format;
        FileName = fileName;
        Content = content;
    }

    /// <summary>CSV, PNG or JPG.</summary>
    public CdsChartExportFormat Format { get; }

    /// <summary>The file name with its extension ("myChart.csv").</summary>
    public string FileName { get; }

    /// <summary>The file's bytes (UTF-8 for CSV).</summary>
    public byte[] Content { get; }

    /// <summary>Set to <c>true</c> when you saved the file: the chart then skips the system share sheet.</summary>
    public bool Handled { get; set; }
}

/// <summary>The series a legend click isolated.</summary>
public sealed class CdsChartSelectedGroupsChangedEventArgs(IReadOnlyList<string> selectedGroups) : EventArgs
{
    /// <summary>The isolated groups; empty when every series shows.</summary>
    public IReadOnlyList<string> SelectedGroups { get; } = selectedGroups;
}
