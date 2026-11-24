namespace Maui.Carbon;

/// <summary>Layout direction of a group of choices (checkboxes, radio buttons).</summary>
public enum CdsChoiceOrientation
{
    /// <summary>Choices stack top to bottom, 8px apart (Carbon's preferred layout; it scans faster).</summary>
    Vertical,
    /// <summary>Choices sit in a wrapping row, 16px apart.</summary>
    Horizontal,
}

/// <summary>Event data for a change of a choice control's value: the value before and after.</summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class CdsChoiceChangedEventArgs<T> : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsChoiceChangedEventArgs(T? oldValue, T? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>The value before the change.</summary>
    public T? OldValue { get; }

    /// <summary>The value after the change.</summary>
    public T? NewValue { get; }
}

/// <summary>Which message a form control shows under itself.</summary>
public enum CdsFormMessageKind
{
    /// <summary>Nothing.</summary>
    None,
    /// <summary>Helper text.</summary>
    Helper,
    /// <summary>The warning message with the warning triangle.</summary>
    Warn,
    /// <summary>The error message with the error icon.</summary>
    Invalid,
}

/// <summary>The resolved message of a form control: what to show and in which tokens.</summary>
/// <param name="Kind">Which message wins.</param>
/// <param name="Text">The text, or <c>null</c> when nothing shows.</param>
/// <param name="TextColor">Text color key.</param>
/// <param name="Icon">Status icon, or <c>null</c>.</param>
/// <param name="IconColor">Icon color key, or <c>null</c>.</param>
public readonly record struct CdsFormMessageVisuals(CdsFormMessageKind Kind, string? Text, string TextColor, CdsIconName? Icon, string? IconColor);

/// <summary>Picks the message shown under a form control: invalid beats warning beats helper text.</summary>
public static class CdsFormMessageResolver
{
    /// <summary>
    /// Resolves the message. Invalid and warning are ignored while disabled or read-only; an empty message falls back
    /// to the next one. Helper text uses <c>text-disabled</c> when disabled, or is hidden while disabled, invalid or
    /// warning when <paramref name="hideHelperWhenBlocked"/> is set (radio buttons).
    /// </summary>
    public static CdsFormMessageVisuals Resolve(CdsInteractionState state, string? helper, string? invalidText, string? warnText, bool hideHelperWhenBlocked = false)
    {
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var readOnly = state.HasFlag(CdsInteractionState.ReadOnly);
        var invalid = state.HasFlag(CdsInteractionState.Invalid) && !disabled && !readOnly;
        var warn = state.HasFlag(CdsInteractionState.Warn) && !invalid && !disabled && !readOnly;

        if (invalid && !string.IsNullOrEmpty(invalidText))
            return new(CdsFormMessageKind.Invalid, invalidText, CdsColorKeys.TextError, CdsIconName.WarningFilled, CdsColorKeys.SupportError);
        if (warn && !string.IsNullOrEmpty(warnText))
            return new(CdsFormMessageKind.Warn, warnText, CdsColorKeys.TextPrimary, CdsIconName.WarningAltFilled, CdsColorKeys.SupportWarning);
        if (string.IsNullOrEmpty(helper) || (hideHelperWhenBlocked && (disabled || invalid || warn)))
            return new(CdsFormMessageKind.None, null, CdsColorKeys.TextHelper, null, null);
        return new(CdsFormMessageKind.Helper, helper, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextHelper, null, null);
    }
}

/// <summary>The message row under a group or control: an optional 16px status icon and the text in <c>helper-text-01</c>.</summary>
internal sealed class CdsFormMessage : Grid
{
    readonly CdsIcon _icon = new() { IsVisible = false, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 1, 0, 0) };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper };
    readonly Dictionary<(BindableObject, BindableProperty), string?> _applied = [];
    readonly CdsMessageAlert _alert = new();

    public CdsFormMessage()
    {
        ColumnSpacing = 0;
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        this.Add(_icon, 0);
        this.Add(_text, 1);
        IsVisible = false;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    /// <summary>The text currently shown, or <c>null</c>.</summary>
    public string? CurrentText => IsVisible ? _text.Text : null;

    /// <summary>Applies a resolved message.</summary>
    public void Apply(CdsFormMessageVisuals v)
    {
        _text.Text = v.Text;
        IsVisible = !string.IsNullOrEmpty(v.Text);
        // role="alert": the row itself is out of the accessibility tree, so a new error or warning is spoken once.
        _alert.Update(this, v.Kind is CdsFormMessageKind.Invalid or CdsFormMessageKind.Warn ? v.Text : null);
        Set(_text, Label.TextColorProperty, v.TextColor);
        if (v.Icon is { } icon)
        {
            _icon.Icon = icon;
            _icon.IsVisible = true;
            _text.Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
            Set(_icon, CdsIcon.ColorProperty, v.IconColor);
        }
        else
        {
            _icon.IsVisible = false;
            _text.Margin = 0;
        }
    }

    void Set(Element target, BindableProperty property, string? key)
    {
        if (_applied.TryGetValue((target, property), out var current) && current == key) return;
        _applied[(target, property)] = key;
        CdsTokens.SetColor(target, property, key);
    }
}

/// <summary>Base of the small drawn glyphs (checkbox box, radio circle, toggle track, slider thumbs).</summary>
internal abstract class CdsGlyphView : GraphicsView, IDrawable
{
    protected CdsGlyphView()
    {
        Drawable = new CdsWeakDrawable(this);
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    public abstract void Draw(ICanvas canvas, RectF dirtyRect);

    protected static void Redraw(BindableObject b, object? o, object? n) => ((CdsGlyphView)b).Invalidate();

    protected static bool Visible(Color? c) => c is { Alpha: > 0 };
}

/// <summary>The mark inside a checkbox box.</summary>
internal enum CdsCheckMark
{
    None,
    Check,
    Dash,
}

/// <summary>The 16x16 checkbox box: 1px border, 2px corners, fill, and a check or dash mark.</summary>
internal sealed class CdsCheckGlyph : CdsGlyphView
{
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CdsCheckGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(CdsCheckGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty MarkColorProperty = BindableProperty.Create(nameof(MarkColor), typeof(Color), typeof(CdsCheckGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty MarkProperty = BindableProperty.Create(nameof(Mark), typeof(CdsCheckMark), typeof(CdsCheckGlyph), CdsCheckMark.None, propertyChanged: Redraw);

    public CdsCheckGlyph()
    {
        WidthRequest = 16;
        HeightRequest = 16;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
    }

    public Color? BorderColor { get => (Color?)GetValue(BorderColorProperty); set => SetValue(BorderColorProperty, value); }
    public Color? FillColor { get => (Color?)GetValue(FillColorProperty); set => SetValue(FillColorProperty, value); }
    public Color? MarkColor { get => (Color?)GetValue(MarkColorProperty); set => SetValue(MarkColorProperty, value); }
    public CdsCheckMark Mark { get => (CdsCheckMark)GetValue(MarkProperty); set => SetValue(MarkProperty, value); }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float size = 16;
        var radius = (float)CdsRadius.Checkbox;
        if (Visible(FillColor))
        {
            canvas.FillColor = FillColor!;
            canvas.FillRoundedRectangle(0, 0, size, size, radius);
        }
        if (Visible(BorderColor))
        {
            canvas.StrokeColor = BorderColor!;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(0.5f, 0.5f, size - 1, size - 1, radius - 0.5f);
        }
        if (!Visible(MarkColor)) return;
        canvas.FillColor = MarkColor!;
        switch (Mark)
        {
            case CdsCheckMark.Check:
                // The check of Carbon's checkbox--checked glyph, scaled from its 24-unit box to 16.
                var path = new PathF();
                path.MoveTo(6.67f, 11.67f);
                path.LineTo(3.33f, 8.36f);
                path.LineTo(4.39f, 7.33f);
                path.LineTo(6.67f, 9.57f);
                path.LineTo(11.6f, 4.67f);
                path.LineTo(12.67f, 5.72f);
                path.Close();
                canvas.FillPath(path);
                break;
            case CdsCheckMark.Dash:
                canvas.FillRectangle(4, 7, 8, 2);
                break;
        }
    }
}

/// <summary>The 18px radio circle: 1px ring and a dot at half size.</summary>
internal sealed class CdsRadioGlyph : CdsGlyphView
{
    public static readonly BindableProperty RingColorProperty = BindableProperty.Create(nameof(RingColor), typeof(Color), typeof(CdsRadioGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty DotColorProperty = BindableProperty.Create(nameof(DotColor), typeof(Color), typeof(CdsRadioGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty ShowDotProperty = BindableProperty.Create(nameof(ShowDot), typeof(bool), typeof(CdsRadioGlyph), false, propertyChanged: Redraw);

    public CdsRadioGlyph()
    {
        WidthRequest = 18;
        HeightRequest = 18;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
    }

    public Color? RingColor { get => (Color?)GetValue(RingColorProperty); set => SetValue(RingColorProperty, value); }
    public Color? DotColor { get => (Color?)GetValue(DotColorProperty); set => SetValue(DotColorProperty, value); }
    public bool ShowDot { get => (bool)GetValue(ShowDotProperty); set => SetValue(ShowDotProperty, value); }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float size = 18;
        if (Visible(RingColor))
        {
            canvas.StrokeColor = RingColor!;
            canvas.StrokeSize = 1;
            canvas.DrawCircle(size / 2, size / 2, size / 2 - 0.5f);
        }
        if (ShowDot && Visible(DotColor))
        {
            canvas.FillColor = DotColor!;
            canvas.FillCircle(size / 2, size / 2, size / 4);
        }
    }
}

/// <summary>Wraps one choice in a wrapping row so it gets its trailing gap (a plain <see cref="ContentView"/> would be mistaken for a Carbon control).</summary>
internal sealed class CdsChoiceCell : ContentView
{
}
