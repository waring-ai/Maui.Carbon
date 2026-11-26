using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>The graph types a <see cref="CdsComboChart"/> can draw (Carbon's <c>comboChartTypes[].type</c>).</summary>
public enum CdsComboChartGraph
{
    /// <summary>Simple bars (<c>simple-bar</c>).</summary>
    SimpleBar,
    /// <summary>Grouped bars (<c>grouped-bar</c>).</summary>
    GroupedBar,
    /// <summary>Stacked bars (<c>stacked-bar</c>).</summary>
    StackedBar,
    /// <summary>Lines with points (<c>line</c>).</summary>
    Line,
    /// <summary>Areas (<c>area</c>).</summary>
    Area,
    /// <summary>Stacked areas (<c>stacked-area</c>).</summary>
    StackedArea,
    /// <summary>Scatter dots (<c>scatter</c>).</summary>
    Scatter,
    /// <summary>Lollipops (<c>lollipop</c>).</summary>
    Lollipop,
}

/// <summary>
/// One graph of a <see cref="CdsComboChart"/> (Carbon's <c>comboChartTypes</c> entry): the graph type and the datasets
/// (groups) it draws, with that type's own options. Unset options keep the type's Carbon defaults.
/// </summary>
public sealed class CdsComboChartType : BindableObject
{
    /// <summary>Bindable property for <see cref="Graph"/>.</summary>
    public static readonly BindableProperty GraphProperty = Create(nameof(Graph), typeof(CdsComboChartGraph), CdsComboChartGraph.SimpleBar);

    /// <summary>Bindable property for <see cref="CorrespondingDatasets"/>.</summary>
    public static readonly BindableProperty CorrespondingDatasetsProperty = Create(nameof(CorrespondingDatasets), typeof(IList<string>), null);

    /// <summary>Bindable property for <see cref="Curve"/>.</summary>
    public static readonly BindableProperty CurveProperty = Create(nameof(Curve), typeof(CdsChartCurve?), null);

    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = Create(nameof(PointRadius), typeof(double?), null);

    /// <summary>Bindable property for <see cref="ShowPoints"/>.</summary>
    public static readonly BindableProperty ShowPointsProperty = Create(nameof(ShowPoints), typeof(bool?), null);

    /// <summary>Bindable property for <see cref="BarMaxWidth"/>.</summary>
    public static readonly BindableProperty BarMaxWidthProperty = Create(nameof(BarMaxWidth), typeof(double?), null);

    /// <summary>Raised when a setting changes.</summary>
    public event EventHandler? Changed;

    /// <summary>The graph type. Default <see cref="CdsComboChartGraph.SimpleBar"/>.</summary>
    public CdsComboChartGraph Graph
    {
        get => (CdsComboChartGraph)GetValue(GraphProperty);
        set => SetValue(GraphProperty, value);
    }

    /// <summary>The groups this graph draws; <c>null</c> draws every group.</summary>
    public IList<string>? CorrespondingDatasets
    {
        get => (IList<string>?)GetValue(CorrespondingDatasetsProperty);
        set => SetValue(CorrespondingDatasetsProperty, value);
    }

    /// <summary>The curve of a line or area graph; <c>null</c> is linear.</summary>
    public CdsChartCurve? Curve
    {
        get => (CdsChartCurve?)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    /// <summary>The point radius of line, area, scatter and lollipop graphs; <c>null</c> keeps the type's default (3 for lines, 4 for dots).</summary>
    public double? PointRadius
    {
        get => (double?)GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    /// <summary>Whether line and area graphs draw points; <c>null</c> keeps the type's default.</summary>
    public bool? ShowPoints
    {
        get => (bool?)GetValue(ShowPointsProperty);
        set => SetValue(ShowPointsProperty, value);
    }

    /// <summary>The largest bar thickness of bar graphs; <c>null</c> keeps 16.</summary>
    public double? BarMaxWidth
    {
        get => (double?)GetValue(BarMaxWidthProperty);
        set => SetValue(BarMaxWidthProperty, value);
    }

    static BindableProperty Create(string name, Type type, object? defaultValue) => BindableProperty.Create(
        name, type, typeof(CdsComboChartType), defaultValue,
        propertyChanged: static (b, _, _) => ((CdsComboChartType)b).Changed?.Invoke(b, EventArgs.Empty));
}

/// <summary>
/// Several graph types on one set of axes (Carbon's combo chart), such as bars for a volume and a line for a rate: each
/// <see cref="CdsComboChartType"/> in <see cref="ChartTypes"/> draws its datasets with that type's look. For a second
/// measure, give the right axis <see cref="CdsChartAxis.CorrespondingDatasets"/> and title both axes with their units;
/// two unrelated measures read more honestly as two charts. Colors follow the pairing for all datasets together, so a
/// dataset keeps its color whichever graph draws it. One ruler shows every dataset at the hovered position.
/// </summary>
[ContentProperty(nameof(ChartTypes))]
public class CdsComboChart : CdsCartesianChart
{
    readonly CdsItemCollection<CdsComboChartType> _types = [];
    readonly Dictionary<CdsComboChartType, CdsCartesianChart> _graphs = [];

    /// <summary>Creates a combo chart.</summary>
    public CdsComboChart()
    {
        _types.CollectionChanged += OnTypesChanged;
    }

    /// <summary>The graphs, drawn in order (later graphs on top): Carbon's <c>comboChartTypes</c>.</summary>
    public IList<CdsComboChartType> ChartTypes => _types;

    private protected override string ChartKind => "Combo chart";

    private protected override bool UsesRuler => true;

    private protected override CartesianOptions Options(ChartBuildContext ctx) => new() { RangeValues = rows => RangeValues(ctx, rows) };

    void OnTypesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsComboChartType t in e.OldItems)
            {
                t.Changed -= OnTypeChanged;
                _graphs.Remove(t);
            }
        if (e.NewItems is not null)
            foreach (CdsComboChartType t in e.NewItems)
            {
                t.Changed += OnTypeChanged;
                SetInheritedBindingContext(t, BindingContext);
            }
        RequestRebuild();
    }

    void OnTypeChanged(object? sender, EventArgs e)
    {
        if (sender is CdsComboChartType t) _graphs.Remove(t);
        RequestRebuild();
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var t in _types) SetInheritedBindingContext(t, BindingContext);
    }

    static bool IsStacked(CdsComboChartGraph g) => g is CdsComboChartGraph.StackedBar or CdsComboChartGraph.StackedArea;

    HashSet<string> GroupsOf(CdsComboChartType t, ChartBuildContext ctx) =>
        new(t.CorrespondingDatasets is { Count: > 0 } list ? list : ctx.Groups, StringComparer.Ordinal);

    // The range domain: stacked sums for the datasets of stacked graphs, plain values (or ranges) for the rest.
    IEnumerable<double> RangeValues(ChartBuildContext ctx, IReadOnlyList<CdsChartPoint> rows)
    {
        var stacked = new HashSet<string>(StringComparer.Ordinal);
        foreach (var t in _types) if (IsStacked(t.Graph)) stacked.UnionWith(GroupsOf(t, ctx));
        foreach (var p in rows)
        {
            if (stacked.Contains(p.Group ?? "")) continue;
            if (p.HasRange)
            {
                yield return p.Low!.Value;
                yield return p.High!.Value;
            }
            else if (p.Value is { } v) yield return v;
        }
        if (stacked.Count == 0) yield break;
        foreach (var column in rows.Where(p => stacked.Contains(p.Group ?? "")).GroupBy(p => (p.Key ?? p.Group ?? "") + "|" + p.Date?.Ticks + "|" + p.X))
        {
            double pos = 0, neg = 0;
            foreach (var p in column)
            {
                var v = p.Value ?? 0;
                if (v >= 0) pos += v;
                else neg += v;
            }
            yield return pos;
            yield return neg;
        }
    }

    CdsCartesianChart Graph(CdsComboChartType t)
    {
        if (_graphs.TryGetValue(t, out var g)) return g;
        CdsCartesianChart chart = t.Graph switch
        {
            CdsComboChartGraph.GroupedBar => new CdsGroupedBarChart(),
            CdsComboChartGraph.StackedBar => new CdsStackedBarChart(),
            CdsComboChartGraph.Line => new CdsLineChart(),
            CdsComboChartGraph.Area => new CdsAreaChart(),
            CdsComboChartGraph.StackedArea => new CdsStackedAreaChart(),
            CdsComboChartGraph.Scatter => new CdsScatterChart(),
            CdsComboChartGraph.Lollipop => new CdsLollipopChart(),
            _ => new CdsSimpleBarChart(),
        };
        switch (chart)
        {
            case CdsSimpleBarChart s when t.BarMaxWidth is { } w: s.BarMaxWidth = w; break;
            case CdsGroupedBarChart s when t.BarMaxWidth is { } w: s.BarMaxWidth = w; break;
            case CdsStackedBarChart s when t.BarMaxWidth is { } w: s.BarMaxWidth = w; break;
            case CdsLineChart l:
                if (t.Curve is { } lc) l.Curve = lc;
                if (t.PointRadius is { } lr) l.PointRadius = lr;
                if (t.ShowPoints is { } lp) l.ShowPoints = lp;
                break;
            case CdsAreaChart a:
                if (t.Curve is { } ac) a.Curve = ac;
                if (t.PointRadius is { } ar) a.PointRadius = ar;
                if (t.ShowPoints is { } ap) a.ShowPoints = ap;
                break;
            case CdsStackedAreaChart sa:
                if (t.Curve is { } sc) sa.Curve = sc;
                if (t.PointRadius is { } sr) sa.PointRadius = sr;
                if (t.ShowPoints is { } sp) sa.ShowPoints = sp;
                break;
            case CdsScatterChart sc2 when t.PointRadius is { } r2: sc2.PointRadius = r2; break;
            case CdsLollipopChart lo when t.PointRadius is { } r3: lo.PointRadius = r3; break;
        }
        _graphs[t] = chart;
        return chart;
    }

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        // One ruler for every dataset; each graph's own ruler marks map onto it.
        var rulerScene = new ChartScene { Width = scene.Width, Height = scene.Height, Plot = scene.Plot };
        CartesianFrame.RulerMarks(rulerScene, ctx, cs, ctx.ActivePoints, firstId: 0);
        var columns = rulerScene.Marks;
        var nextId = columns.Count;
        var selection = ctx.ActiveGroups.Count != ctx.Groups.Count;
        foreach (var t in _types)
        {
            var groups = GroupsOf(t, ctx);
            var own = ctx.Groups.Where(groups.Contains).ToList();
            var active = ctx.ActiveGroups.Where(groups.Contains).ToList();
            if (active.Count == 0) continue;
            var items = ctx.Items.Where(i => i is not CdsChartPoint p || groups.Contains(p.Group ?? "")).ToList();
            var sub = new ChartBuildContext(ctx.Width, ctx.Height, items, own, selection ? active : [], ctx.Colors, ctx.Culture)
            {
                ValueFormat = ctx.ValueFormat,
                ValueFormatter = ctx.ValueFormatter,
                ShowTotal = ctx.ShowTotal,
                TotalLabel = ctx.TotalLabel,
                GroupLabel = ctx.GroupLabel,
            };
            var part = new ChartScene { Width = scene.Width, Height = scene.Height, Plot = scene.Plot, HitMode = scene.HitMode, RulerVertical = scene.RulerVertical };
            Graph(t).BuildMarksInto(part, sub, cs);
            var ids = new Dictionary<int, int>();
            foreach (var m in part.Marks)
            {
                if (m.RulerAt is { } at)
                {
                    var column = columns.FirstOrDefault(c => Math.Abs(c.RulerAt!.Value - at) < 0.5f);
                    if (column is not null) ids[m.Id] = column.Id;
                    continue;
                }
                var id = nextId++;
                ids[m.Id] = id;
                scene.Marks.Add(Copy(m, id));
            }
            foreach (var s in part.Shapes)
            {
                if (s.Mark >= 0) s.Mark = ids.TryGetValue(s.Mark, out var mapped) ? mapped : -1;
                scene.Shapes.Add(s);
            }
            scene.Texts.AddRange(part.Texts);
        }
        // Ruler columns first: they are the data cursor's order across every graph.
        scene.Marks.InsertRange(0, columns);
        var datasets = ctx.ActiveGroups.Count;
        scene.Summary = datasets == 1 ? "1 dataset" : $"{datasets} datasets";
    }

    static ChartMark Copy(ChartMark m, int id) => new()
    {
        Id = id,
        Bounds = m.Bounds,
        Polygon = m.Polygon,
        Radius = m.Radius,
        Group = m.Group,
        Rows = m.Rows,
        Title = m.Title,
        Description = m.Description,
        RulerAt = m.RulerAt,
        Anchor = m.Anchor,
        IsReference = m.IsReference,
    };
}
