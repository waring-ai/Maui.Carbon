namespace Maui.Carbon;

/// <summary>The status of a <see cref="CdsProgressBar"/>.</summary>
public enum CdsProgressStatus
{
    /// <summary>Running (the default): an <c>interactive</c> fill.</summary>
    Active,
    /// <summary>Done: a full <c>support-success</c> bar with a check icon.</summary>
    Finished,
    /// <summary>Failed: a full <c>support-error</c> bar, an error icon and <c>text-error</c> helper text.</summary>
    Error,
}

/// <summary>Track height of a <see cref="CdsProgressBar"/>.</summary>
public enum CdsProgressBarSize
{
    /// <summary>8px track: pages, dialogs and full-width areas (the default).</summary>
    Big,
    /// <summary>4px track: cards, data tables and side panels.</summary>
    Small,
}

/// <summary>Layout of a <see cref="CdsProgressBar"/>.</summary>
public enum CdsProgressBarType
{
    /// <summary>Label above the track, helper text below (pages and dialogs).</summary>
    Default,
    /// <summary>Label beside the track; helper text is not shown (tables and lists).</summary>
    Inline,
    /// <summary>Like default with 16px side padding on label and helper text (side panels, dashboard cards).</summary>
    Indented,
}

/// <summary>Colors of a progress bar status.</summary>
/// <param name="Fill">The fill color key.</param>
/// <param name="Icon">The status icon color key (<c>null</c> when there is none).</param>
/// <param name="HelperText">The helper text color key.</param>
public readonly record struct CdsProgressBarVisuals(string Fill, string? Icon, string HelperText);

/// <summary>Maps progress bar statuses to tokens.</summary>
public static class CdsProgressBarTokens
{
    /// <summary>Active: <c>interactive</c> fill. Finished: <c>support-success</c>. Error: <c>support-error</c> with <c>text-error</c> helper text.</summary>
    public static CdsProgressBarVisuals Resolve(CdsProgressStatus status) => status switch
    {
        CdsProgressStatus.Finished => new(CdsColorKeys.SupportSuccess, CdsColorKeys.SupportSuccess, CdsColorKeys.TextSecondary),
        CdsProgressStatus.Error => new(CdsColorKeys.SupportError, CdsColorKeys.SupportError, CdsColorKeys.TextError),
        _ => new(CdsColorKeys.Interactive, null, CdsColorKeys.TextSecondary),
    };

    /// <summary>The track height: 8 (big) or 4 (small).</summary>
    public static double TrackHeight(CdsProgressBarSize size) => size == CdsProgressBarSize.Small ? 4 : 8;
}

/// <summary>Progress math (pure, tested).</summary>
internal static class ProgressBarModel
{
    /// <summary>The filled fraction from 0 to 1. Finished and error are always full; no value (indeterminate) is 0.</summary>
    public static double Fraction(double? value, double max, CdsProgressStatus status)
    {
        if (status != CdsProgressStatus.Active) return 1;
        if (value is not { } v || max <= 0 || double.IsNaN(v)) return 0;
        return Math.Clamp(v / max, 0, 1);
    }

    /// <summary>True when the bar shows the running sweep: active with no value.</summary>
    public static bool IsIndeterminate(double? value, CdsProgressStatus status) =>
        status == CdsProgressStatus.Active && value is null;

    /// <summary>The indeterminate block at a phase (0 to 1): its left edge and width for a track of <paramref name="trackWidth"/>; hidden during the pause at the end of each loop.</summary>
    public static (double X, double Width, bool Visible) IndeterminateBlock(double phase, double trackWidth)
    {
        phase -= Math.Floor(phase);
        var width = trackWidth * 0.25;
        if (phase >= 0.8) return (trackWidth, width, false);
        var t = phase / 0.8;
        return (-width + t * (trackWidth + width), width, true);
    }

    /// <summary>The accessible description: the label plus the value as a percentage, "loading" or "done"/"error".</summary>
    public static string Describe(string? label, double? value, double max, CdsProgressStatus status)
    {
        var state = status switch
        {
            CdsProgressStatus.Finished => "done",
            CdsProgressStatus.Error => "error",
            _ => value is null ? "loading" : $"{Math.Round(Fraction(value, max, status) * 100):0} percent",
        };
        return string.IsNullOrEmpty(label) ? state : $"{label}, {state}";
    }
}

/// <summary>
/// A horizontal bar showing how far a task has progressed, or that it is running when the amount is unknown. Give it a
/// <see cref="Label"/> (what is progressing; required even when hidden) and a <see cref="Value"/> from 0 to
/// <see cref="Max"/>; leave the value <c>null</c> for the indeterminate sweep. A determinate bar only moves forward and
/// never resets; do not fake values. Finished and error add an icon and the finished or error color; helper text on
/// error must say what went wrong and what to do. The track is <c>layer-01</c> (it follows layers, so it stays visible on
/// cards) and the fill grows from the start over 110ms.
/// </summary>
public class CdsProgressBar : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsProgressBar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double?), typeof(CdsProgressBar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Max"/>.</summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max), typeof(double), typeof(CdsProgressBar), 100d, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsProgressBar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Status"/>.</summary>
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(CdsProgressStatus), typeof(CdsProgressBar), CdsProgressStatus.Active, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsProgressBarSize), typeof(CdsProgressBar), CdsProgressBarSize.Big, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsProgressBarType), typeof(CdsProgressBar), CdsProgressBarType.Default, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsProgressBar), false, propertyChanged: OnChanged);

    const string SweepAnimation = "cds-progress-sweep";

    readonly Grid _root = new() { RowSpacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };
    readonly CdsText _helper = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0) };
    readonly Border _track = new() { StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(), MinimumWidthRequest = 48, VerticalOptions = LayoutOptions.Center };
    readonly Grid _trackContent = new();
    readonly BoxView _fill = new() { AnchorX = 0, ScaleX = 0.0001 };
    readonly BoxView _sweep = new() { HorizontalOptions = LayoutOptions.Start, IsVisible = false };
    readonly CdsIcon _statusIcon = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center };
    double _shownFraction;
    CdsProgressStatus _announcedStatus = CdsProgressStatus.Active;

    /// <summary>Creates an indeterminate big default progress bar.</summary>
    public CdsProgressBar()
    {
        _trackContent.Add(_fill);
        _trackContent.Add(_sweep);
        _track.Content = _trackContent;
        _track.SizeChanged += (_, _) => RestartSweep();
        HorizontalOptions = LayoutOptions.Fill;
        SetChrome(_root);
        Loaded += (_, _) => RestartSweep();
        Unloaded += (_, _) => CdsLoop.Stop(_track, SweepAnimation);
        Update();
    }

    /// <summary>Creates a determinate progress bar.</summary>
    public CdsProgressBar(string label, double? value, double max = 100) : this()
    {
        Label = label;
        Max = max;
        Value = value;
    }

    /// <summary>What is progressing ("Uploading usage-march.csv"). Keep it the same throughout; status comes from color, icon and helper text. Required, even when hidden.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The progress from 0 to <see cref="Max"/>. <c>null</c> is the indeterminate animation, which shows no numbers.</summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The value at which the bar is full. Default 100.</summary>
    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>Text under the track ("42 of 100 MB"); on error it is <c>text-error</c> and says what to do. Not shown inline.</summary>
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Active (default), finished or error. Finished and error force a full bar with an icon.</summary>
    public CdsProgressStatus Status
    {
        get => (CdsProgressStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Track height: big 8 (default) or small 4.</summary>
    public CdsProgressBarSize Size
    {
        get => (CdsProgressBarSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Layout: default, inline (label beside the track) or indented.</summary>
    public CdsProgressBarType Type
    {
        get => (CdsProgressBarType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>Hide the visible label; it remains the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsProgressBarTokens.Resolve(Status);
        SetColor(_track, BackgroundColorProperty, CdsColorKeys.Layer01);
        SetColor(_fill, BoxView.ColorProperty, v.Fill);
        SetColor(_sweep, BoxView.ColorProperty, CdsColorKeys.Interactive);
        SetColor(_statusIcon, CdsIcon.ColorProperty, v.Icon);
        SetColor(_helper, Microsoft.Maui.Controls.Label.TextColorProperty, v.HelperText);
    }

    void Update()
    {
        var status = Status;
        var inline = Type == CdsProgressBarType.Inline;
        var indeterminate = ProgressBarModel.IsIndeterminate(Value, status);
        var settled = status != CdsProgressStatus.Active;
        var showLabel = !HideLabel && !string.IsNullOrEmpty(Label);
        var trackHeight = CdsProgressBarTokens.TrackHeight(Size);

        _label.Text = Label;
        _label.IsVisible = showLabel;
        _helper.Text = HelperText;
        _helper.IsVisible = !inline && !string.IsNullOrEmpty(HelperText);
        _statusIcon.IsVisible = settled;
        if (settled)
        {
            _statusIcon.Icon = status == CdsProgressStatus.Finished ? CdsIconName.CheckmarkFilled : CdsIconName.ErrorFilled;
            _statusIcon.Description = status == CdsProgressStatus.Finished ? "finished" : "error";
        }
        _track.HeightRequest = trackHeight;
        _sweep.HeightRequest = trackHeight;
        _sweep.IsVisible = indeterminate;
        _track.IsVisible = !(inline && settled);

        // Layout: default/indented stack label (with status icon), track, helper; inline puts all on one row.
        _root.Children.Clear();
        _root.RowDefinitions.Clear();
        _root.ColumnDefinitions.Clear();
        var sidePad = Type == CdsProgressBarType.Indented ? CdsSpacing.Spacing05 : 0;
        foreach (var view in new View[] { _label, _track, _helper, _statusIcon })
        {
            Grid.SetColumnSpan(view, 1);
            Grid.SetRowSpan(view, 1);
        }
        if (inline)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _label.Margin = new Thickness(0, 0, showLabel ? CdsSpacing.Spacing05 : 0, 0);
            _statusIcon.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
            _root.Add(_label, 0);
            _root.Add(_track, 1);
            _root.Add(_statusIcon, 2);
        }
        else
        {
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _label.Margin = new Thickness(sidePad, 0, 0, showLabel ? CdsSpacing.Spacing03 : 0);
            _statusIcon.Margin = new Thickness(CdsSpacing.Spacing05, 0, sidePad, showLabel ? CdsSpacing.Spacing03 : 0);
            _helper.Margin = new Thickness(sidePad, CdsSpacing.Spacing03, sidePad, 0);
            _root.Add(_label, 0, 0);
            _root.Add(_statusIcon, 1, 0);
            _root.Add(_track, 0, 1);
            Grid.SetColumnSpan(_track, 2);
            _root.Add(_helper, 0, 2);
            Grid.SetColumnSpan(_helper, 2);
            // Without a visible label the status icon sits on the track row instead.
            if (!showLabel && settled)
            {
                Grid.SetRow(_statusIcon, 1);
                Grid.SetColumnSpan(_track, 1);
                _statusIcon.Margin = new Thickness(CdsSpacing.Spacing05, 0, sidePad, 0);
            }
        }

        // The accessible name carries the label (even when hidden), the value and the status.
        SemanticProperties.SetDescription(_root, ProgressBarModel.Describe(Label, Value, Max, status));
        SemanticProperties.SetHint(_root, HelperText);

        // Fill: grows from the start edge over 110ms; finished and error are full.
        var target = ProgressBarModel.Fraction(Value, Max, status);
        AnimateFill(target);
        RefreshVisuals();
        RestartSweep();

        if (status != _announcedStatus)
        {
            _announcedStatus = status;
            if (IsLoaded && !string.IsNullOrEmpty(HelperText)) AnnounceStatus(status);
        }
    }

    void AnimateFill(double target)
    {
        var from = _shownFraction;
        _shownFraction = target;
        if (Math.Abs(from - target) < 0.0001)
        {
            _fill.ScaleX = Math.Max(target, 0.0001);
            return;
        }
        _ = CdsMotion.AnimateAsync(_fill, "cds-progress-fill", v => _fill.ScaleX = Math.Max(v, 0.0001), from, target,
            CdsDuration.Fast02, CdsEasing.StandardProductive);
    }

    void RestartSweep()
    {
        if (!ProgressBarModel.IsIndeterminate(Value, Status))
        {
            CdsLoop.Stop(_track, SweepAnimation);
            return;
        }
        if (!IsLoaded || _track.Width <= 0) return;
        var width = _track.Width;
        _sweep.WidthRequest = width * 0.25;
        CdsLoop.Start(_track, SweepAnimation, phase =>
        {
            var (x, w, visible) = ProgressBarModel.IndeterminateBlock(phase, width);
            _sweep.WidthRequest = w;
            _sweep.TranslationX = x;
            _sweep.IsVisible = visible;
        }, 0.4, 1400);
    }

    static void AnnounceStatus(CdsProgressStatus status)
    {
        try
        {
            SemanticScreenReader.Announce(status == CdsProgressStatus.Finished ? "Done" : "Error");
        }
        catch (Exception)
        {
            // No screen reader service on this platform.
        }
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsProgressBar)b).Update();
}
