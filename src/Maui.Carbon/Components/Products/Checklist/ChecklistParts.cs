namespace Maui.Carbon;

/// <summary>
/// The 40px progress ring of a checklist header: a <see cref="CdsChecklistTokens.ChartTrack"/> ring and a
/// <see cref="CdsChecklistTokens.ChartProgress"/> arc of <c>value x 360</c> degrees clockwise from the top, 4 wide
/// (Carbon's conic gradient under a 32px <c>layer-01</c> disc). An image to screen readers, named by the chart label.
/// </summary>
internal sealed class ChecklistChart : GraphicsView, IDrawable
{
    static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        "TrackColor", typeof(Color), typeof(ChecklistChart), null,
        propertyChanged: static (b, _, _) => ((ChecklistChart)b).Invalidate());

    static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        "ProgressColor", typeof(Color), typeof(ChecklistChart), null,
        propertyChanged: static (b, _, _) => ((ChecklistChart)b).Invalidate());

    double _value;

    public ChecklistChart()
    {
        Drawable = new CdsWeakDrawable(this);
        WidthRequest = ChecklistModel.ChartSize;
        HeightRequest = ChecklistModel.ChartSize;
        MinimumWidthRequest = ChecklistModel.ChartSize;
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        SetDynamicResource(TrackColorProperty, CdsChecklistTokens.ChartTrack);
        SetDynamicResource(ProgressColorProperty, CdsChecklistTokens.ChartProgress);
        AutomationProperties.SetIsInAccessibleTree(this, true);
    }

    /// <summary>Progress, 0 to 1.</summary>
    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            Invalidate();
        }
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        var size = (float)Math.Min(dirtyRect.Width, dirtyRect.Height);
        if (size <= 0) return;
        var ring = (float)ChecklistModel.ChartRing;
        var rect = new RectF(dirtyRect.X + ring / 2, dirtyRect.Y + ring / 2, size - ring, size - ring);
        canvas.StrokeSize = ring;
        canvas.StrokeLineCap = LineCap.Butt;
        if (GetValue(TrackColorProperty) is Color track)
        {
            canvas.StrokeColor = track;
            canvas.DrawEllipse(rect);
        }
        var sweep = (float)ChecklistModel.Sweep(_value);
        if (sweep <= 0 || GetValue(ProgressColorProperty) is not Color progress) return;
        canvas.StrokeColor = progress;
        if (sweep >= 360) canvas.DrawEllipse(rect);
        else canvas.DrawArc(rect, 90, 90 - sweep, true, false);
    }
}

/// <summary>
/// A checklist link: the label of a task with an action, or "View all". Carbon renders a primary button restyled as a
/// link: <c>body-compact-01</c> in <c>link-primary</c>, underlined on hover and press, two lines then an ellipsis.
/// </summary>
internal sealed class ChecklistLink : CdsControl
{
    readonly Grid _root = new();
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation, InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    string? _text;
    string? _name;

    public ChecklistLink()
    {
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        _root.Add(_label);
        _root.Add(_ring);
        _root.Add(_hit);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_root);
    }

    public event EventHandler? Activated;

    /// <summary>The visible text, also the tooltip.</summary>
    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            _label.Text = value;
            if (string.IsNullOrEmpty(value)) _hit.ClearValue(ToolTipProperties.TextProperty);
            else ToolTipProperties.SetText(_hit, value);
            SemanticProperties.SetDescription(_hit, _name ?? value);
        }
    }

    /// <summary>The accessible name when it differs from the text (the task status appended).</summary>
    public string? AccessibleName
    {
        get => _name;
        set
        {
            _name = value;
            SemanticProperties.SetDescription(_hit, value ?? _text);
        }
    }

    internal CdsHitTarget Hit => _hit;

    protected override void OnActivated() => Activated?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsChecklistTokens.Link(State);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        _label.TextDecorations = v.Underline ? TextDecorations.Underline : TextDecorations.None;
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}
