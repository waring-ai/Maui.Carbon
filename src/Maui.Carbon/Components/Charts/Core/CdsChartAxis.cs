using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// One axis of an axis chart (Carbon's <c>axes.left</c>, <c>axes.bottom</c>…). Give a chart the axes it should draw;
/// a chart with none draws a linear value axis on the left and a labels axis at the bottom. The domain (category or
/// time) axis is the labels or time axis; put the labels axis on the left for a horizontal chart.
/// </summary>
public sealed class CdsChartAxis : BindableObject
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = Create(nameof(Title), typeof(string), null);

    /// <summary>Bindable property for <see cref="ScaleType"/>.</summary>
    public static readonly BindableProperty ScaleTypeProperty = Create(nameof(ScaleType), typeof(CdsChartScaleType), CdsChartScaleType.Linear);

    /// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
    public static readonly BindableProperty IsVisibleProperty = Create(nameof(IsVisible), typeof(bool), true);

    /// <summary>Bindable property for <see cref="IncludeZero"/>.</summary>
    public static readonly BindableProperty IncludeZeroProperty = Create(nameof(IncludeZero), typeof(bool), true);

    /// <summary>Bindable property for <see cref="Minimum"/>.</summary>
    public static readonly BindableProperty MinimumProperty = Create(nameof(Minimum), typeof(double?), null);

    /// <summary>Bindable property for <see cref="Maximum"/>.</summary>
    public static readonly BindableProperty MaximumProperty = Create(nameof(Maximum), typeof(double?), null);

    /// <summary>Bindable property for <see cref="StartDate"/>.</summary>
    public static readonly BindableProperty StartDateProperty = Create(nameof(StartDate), typeof(DateTime?), null);

    /// <summary>Bindable property for <see cref="EndDate"/>.</summary>
    public static readonly BindableProperty EndDateProperty = Create(nameof(EndDate), typeof(DateTime?), null);

    /// <summary>Bindable property for <see cref="Categories"/>.</summary>
    public static readonly BindableProperty CategoriesProperty = Create(nameof(Categories), typeof(IList<string>), null);

    /// <summary>Bindable property for <see cref="TickCount"/>.</summary>
    public static readonly BindableProperty TickCountProperty = Create(nameof(TickCount), typeof(int?), null);

    /// <summary>Bindable property for <see cref="TickValues"/>.</summary>
    public static readonly BindableProperty TickValuesProperty = Create(nameof(TickValues), typeof(IList<double>), null);

    /// <summary>Bindable property for <see cref="TickFormat"/>.</summary>
    public static readonly BindableProperty TickFormatProperty = Create(nameof(TickFormat), typeof(string), null);

    /// <summary>Bindable property for <see cref="TickFormatter"/>.</summary>
    public static readonly BindableProperty TickFormatterProperty = Create(nameof(TickFormatter), typeof(Func<object, int, string>), null);

    /// <summary>Bindable property for <see cref="TickRotation"/>.</summary>
    public static readonly BindableProperty TickRotationProperty = Create(nameof(TickRotation), typeof(CdsChartTickRotation), CdsChartTickRotation.Auto);

    /// <summary>Bindable property for <see cref="Truncation"/>.</summary>
    public static readonly BindableProperty TruncationProperty = Create(nameof(Truncation), typeof(CdsChartTruncation), CdsChartTruncation.End);

    /// <summary>Bindable property for <see cref="TruncationThreshold"/>.</summary>
    public static readonly BindableProperty TruncationThresholdProperty = Create(nameof(TruncationThreshold), typeof(int), 16);

    /// <summary>Bindable property for <see cref="TruncationLength"/>.</summary>
    public static readonly BindableProperty TruncationLengthProperty = Create(nameof(TruncationLength), typeof(int), 14);

    /// <summary>Bindable property for <see cref="IsPercentage"/>.</summary>
    public static readonly BindableProperty IsPercentageProperty = Create(nameof(IsPercentage), typeof(bool), false);

    /// <summary>Bindable property for <see cref="IsStacked"/>.</summary>
    public static readonly BindableProperty IsStackedProperty = Create(nameof(IsStacked), typeof(bool?), null);

    /// <summary>Bindable property for <see cref="CorrespondingDatasets"/>.</summary>
    public static readonly BindableProperty CorrespondingDatasetsProperty = Create(nameof(CorrespondingDatasets), typeof(IList<string>), null);

    /// <summary>Bindable property for <see cref="IsMain"/>.</summary>
    public static readonly BindableProperty IsMainProperty = Create(nameof(IsMain), typeof(bool), false);

    /// <summary>Bindable property for <see cref="LogBase"/>.</summary>
    public static readonly BindableProperty LogBaseProperty = Create(nameof(LogBase), typeof(double), 10d);

    readonly CdsItemCollection<CdsChartThreshold> _thresholds = [];

    /// <summary>Creates a linear axis.</summary>
    public CdsChartAxis()
    {
        _thresholds.CollectionChanged += OnThresholdsChanged;
    }

    /// <summary>Raised when any setting of the axis or of one of its thresholds changes.</summary>
    public event EventHandler? Changed;

    /// <summary>The axis title: name the measure and its unit ("Revenue (USD)"). Hidden while the chart loads or is empty.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The scale. Default <see cref="CdsChartScaleType.Linear"/>.</summary>
    public CdsChartScaleType ScaleType
    {
        get => (CdsChartScaleType)GetValue(ScaleTypeProperty);
        set => SetValue(ScaleTypeProperty, value);
    }

    /// <summary>Whether the axis line, ticks and title are drawn. The scale still applies when hidden. Default <c>true</c>.</summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Whether a linear axis always includes zero. Default <c>true</c>, and keep it for bars and areas, which show size;
    /// line and scatter charts may turn it off to show change.
    /// </summary>
    public bool IncludeZero
    {
        get => (bool)GetValue(IncludeZeroProperty);
        set => SetValue(IncludeZeroProperty, value);
    }

    /// <summary>The lower end of a linear or log domain (Carbon's <c>domain[0]</c>); <c>null</c> reads it from the data. The axis still adds 10% headroom.</summary>
    public double? Minimum
    {
        get => (double?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>The upper end of a linear or log domain; <c>null</c> reads it from the data.</summary>
    public double? Maximum
    {
        get => (double?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>The start of a time domain; <c>null</c> reads it from the data.</summary>
    public DateTime? StartDate
    {
        get => (DateTime?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    /// <summary>The end of a time domain; <c>null</c> reads it from the data.</summary>
    public DateTime? EndDate
    {
        get => (DateTime?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    /// <summary>The categories of a labels axis, in order; <c>null</c> takes them from the data in order. Points of other categories are not drawn.</summary>
    public IList<string>? Categories
    {
        get => (IList<string>?)GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    /// <summary>How many ticks a continuous axis aims for; <c>null</c> fits as many as the axis length allows (about seven horizontally).</summary>
    public int? TickCount
    {
        get => (int?)GetValue(TickCountProperty);
        set => SetValue(TickCountProperty, value);
    }

    /// <summary>Exact tick values for a linear or log axis; values outside the domain are dropped.</summary>
    public IList<double>? TickValues
    {
        get => (IList<double>?)GetValue(TickValuesProperty);
        set => SetValue(TickValuesProperty, value);
    }

    /// <summary>A .NET format string for tick labels ("N0", "C0", "P0", "MMM d"); used when <see cref="TickFormatter"/> is not set.</summary>
    public string? TickFormat
    {
        get => (string?)GetValue(TickFormatProperty);
        set => SetValue(TickFormatProperty, value);
    }

    /// <summary>Formats a tick label from its value (a <see cref="double"/>, <see cref="DateTime"/> or category <see cref="string"/>) and index.</summary>
    public Func<object, int, string>? TickFormatter
    {
        get => (Func<object, int, string>?)GetValue(TickFormatterProperty);
        set => SetValue(TickFormatterProperty, value);
    }

    /// <summary>When the tick labels of a horizontal axis rotate. Default <see cref="CdsChartTickRotation.Auto"/>.</summary>
    public CdsChartTickRotation TickRotation
    {
        get => (CdsChartTickRotation)GetValue(TickRotationProperty);
        set => SetValue(TickRotationProperty, value);
    }

    /// <summary>How long category labels are cut. The full text stays in the tooltip and the table. Default <see cref="CdsChartTruncation.End"/>.</summary>
    public CdsChartTruncation Truncation
    {
        get => (CdsChartTruncation)GetValue(TruncationProperty);
        set => SetValue(TruncationProperty, value);
    }

    /// <summary>Labels longer than this many characters are truncated. Default 16.</summary>
    public int TruncationThreshold
    {
        get => (int)GetValue(TruncationThresholdProperty);
        set => SetValue(TruncationThresholdProperty, value);
    }

    /// <summary>How many characters a truncated label keeps. Default 14.</summary>
    public int TruncationLength
    {
        get => (int)GetValue(TruncationLengthProperty);
        set => SetValue(TruncationLengthProperty, value);
    }

    /// <summary>Shows the range as 0 to 100% (stacked bar and area charts rescale each stack to its share).</summary>
    public bool IsPercentage
    {
        get => (bool)GetValue(IsPercentageProperty);
        set => SetValue(IsPercentageProperty, value);
    }

    /// <summary>Whether a stacked chart stacks on this axis; <c>null</c> (default) follows the chart.</summary>
    public bool? IsStacked
    {
        get => (bool?)GetValue(IsStackedProperty);
        set => SetValue(IsStackedProperty, value);
    }

    /// <summary>On a secondary range axis (right or top), the groups measured against it: a dual-axis chart.</summary>
    public IList<string>? CorrespondingDatasets
    {
        get => (IList<string>?)GetValue(CorrespondingDatasetsProperty);
        set => SetValue(CorrespondingDatasetsProperty, value);
    }

    /// <summary>Makes this axis the primary one of its pair (right over left, top over bottom).</summary>
    public bool IsMain
    {
        get => (bool)GetValue(IsMainProperty);
        set => SetValue(IsMainProperty, value);
    }

    /// <summary>The base of a log scale. Default 10.</summary>
    public double LogBase
    {
        get => (double)GetValue(LogBaseProperty);
        set => SetValue(LogBaseProperty, value);
    }

    /// <summary>Dashed reference lines on this axis (targets, limits), labelled on hover.</summary>
    public IList<CdsChartThreshold> Thresholds => _thresholds;

    void OnThresholdsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsChartThreshold t in e.OldItems) t.PropertyChanged -= OnThresholdPropertyChanged;
        if (e.NewItems is not null)
            foreach (CdsChartThreshold t in e.NewItems)
            {
                t.PropertyChanged += OnThresholdPropertyChanged;
                SetInheritedBindingContext(t, BindingContext);
            }
        Changed?.Invoke(this, EventArgs.Empty);
    }

    void OnThresholdPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var t in _thresholds) SetInheritedBindingContext(t, BindingContext);
    }

    static BindableProperty Create(string name, Type type, object? defaultValue) => BindableProperty.Create(
        name, type, typeof(CdsChartAxis), defaultValue,
        propertyChanged: static (b, _, _) => ((CdsChartAxis)b).Changed?.Invoke(b, EventArgs.Empty));
}

/// <summary>
/// A dashed reference line on an axis (Carbon's <c>thresholds</c>): a target, limit or service level. Hovering it
/// shows "Label: value". Keep to one or two per chart.
/// </summary>
public sealed class CdsChartThreshold : BindableObject
{
    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = Create(nameof(Value), typeof(double?));

    /// <summary>Bindable property for <see cref="Date"/>.</summary>
    public static readonly BindableProperty DateProperty = Create(nameof(Date), typeof(DateTime?));

    /// <summary>Bindable property for <see cref="Key"/>.</summary>
    public static readonly BindableProperty KeyProperty = Create(nameof(Key), typeof(string));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = Create(nameof(Label), typeof(string));

    /// <summary>Bindable property for <see cref="ColorKey"/>.</summary>
    public static readonly BindableProperty ColorKeyProperty = Create(nameof(ColorKey), typeof(string));

    /// <summary>Bindable property for <see cref="ValueFormat"/>.</summary>
    public static readonly BindableProperty ValueFormatProperty = Create(nameof(ValueFormat), typeof(string));

    /// <summary>The position on a linear or log axis.</summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The position on a time axis.</summary>
    public DateTime? Date
    {
        get => (DateTime?)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    /// <summary>The category on a labels axis (the line runs through its middle).</summary>
    public string? Key
    {
        get => (string?)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    /// <summary>The name shown on hover. Default "Threshold".</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>A color key (<see cref="CdsColorKeys"/>) for the line; <c>null</c> draws Carbon's threshold red (Red 50).</summary>
    public string? ColorKey
    {
        get => (string?)GetValue(ColorKeyProperty);
        set => SetValue(ColorKeyProperty, value);
    }

    /// <summary>A .NET format string for the value on hover ("N0", "C0", "MMM d").</summary>
    public string? ValueFormat
    {
        get => (string?)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    static BindableProperty Create(string name, Type type) => BindableProperty.Create(name, type, typeof(CdsChartThreshold), null);
}
