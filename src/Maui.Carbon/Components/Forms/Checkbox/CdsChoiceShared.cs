using Microsoft.Maui.Layouts;

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

/// <summary>
/// The message row under a group or control: an optional 16px status icon and the text in <c>helper-text-01</c>. The
/// icon and text are created with the first message (most controls never show one).
/// </summary>
internal sealed class CdsFormMessage : Grid
{
    CdsIcon? _icon;
    CdsText? _text;
    readonly Dictionary<(BindableObject, BindableProperty), string?> _applied = [];
    readonly CdsMessageAlert _alert = new();

    public CdsFormMessage()
    {
        ColumnSpacing = 0;
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        IsVisible = false;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }

    /// <summary>The text currently shown, or <c>null</c>.</summary>
    public string? CurrentText => IsVisible ? _text?.Text : null;

    /// <summary>Whether the leading status icon is shown (for tests).</summary>
    internal bool ShowsIcon => _icon?.IsVisible == true;

    /// <summary>Applies a resolved message.</summary>
    public void Apply(CdsFormMessageVisuals v)
    {
        if (_text is null)
        {
            if (string.IsNullOrEmpty(v.Text)) return;
            _icon = new CdsIcon { IsVisible = false, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 1, 0, 0) };
            _text = new CdsText { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper };
            this.Add(_icon, 0);
            this.Add(_text, 1);
        }
        _text.Text = v.Text;
        IsVisible = !string.IsNullOrEmpty(v.Text);
        // role="alert": the row itself is out of the accessibility tree, so a new error or warning is spoken once.
        _alert.Update(this, v.Kind is CdsFormMessageKind.Invalid or CdsFormMessageKind.Warn ? v.Text : null);
        Set(_text, Label.TextColorProperty, v.TextColor);
        if (v.Icon is { } icon)
        {
            _icon!.Icon = icon;
            _icon.IsVisible = true;
            _text.Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
            Set(_icon, CdsIcon.ColorProperty, v.IconColor);
        }
        else
        {
            _icon!.IsVisible = false;
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

/// <summary>
/// A row of one main view (a checkbox's box and label, a radio's circle and label, a group's legend) and Carbon's
/// <c>decorator</c> slot after it. Carbon lays them out in a wrapping flex row: the decorator follows the end of the main view
/// with <c>spacing-03</c> between, and drops to the next line (at the start edge) when the main view leaves no room for it, so
/// a long label wraps first and the AI label never leaves the container. An AI label is shown at size mini (md when its kind is
/// inline and the row allows it: single controls only; groups force the default kind), the inline kind is
/// nudged up 1px as Carbon does.
/// </summary>
internal sealed class CdsDecoratedRow : Layout
{
    readonly bool _allowInline;
    readonly bool _centersDecorator;
    View? _decorator;

    /// <param name="main">The label part; the decorator follows it.</param>
    /// <param name="allowInline">Whether an inline AI label keeps its kind (single checkbox and radio) or is forced to default (groups).</param>
    /// <param name="centersDecorator">Centers the decorator on the line (radio buttons) instead of top-aligning it (checkbox, legends).</param>
    public CdsDecoratedRow(View main, bool allowInline, bool centersDecorator)
    {
        _allowInline = allowInline;
        _centersDecorator = centersDecorator;
        Add(main);
    }

    /// <summary>The view after the main one, or <c>null</c>.</summary>
    public View? Decorator
    {
        get => _decorator;
        set
        {
            if (ReferenceEquals(_decorator, value)) return;
            if (_decorator is { } old)
            {
                old.PropertyChanged -= OnDecoratorPropertyChanged;
                Remove(old);
            }
            _decorator = value;
            if (value is null) return;
            value.PropertyChanged += OnDecoratorPropertyChanged;
            value.HorizontalOptions = LayoutOptions.Start;
            value.VerticalOptions = LayoutOptions.Start;
            Normalize(value);
            Add(value);
        }
    }

    // A decorator's size and kind are the control's to decide (Carbon clones the AI label with them).
    void OnDecoratorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is CdsAILabel label && (e.PropertyName == CdsAILabel.SizeProperty.PropertyName || e.PropertyName == CdsAILabel.KindProperty.PropertyName))
            Normalize(label);
    }

    void Normalize(View decorator)
    {
        var inline = false;
        if (decorator is CdsAILabel label)
        {
            inline = _allowInline && label.Kind == CdsAILabelKind.Inline;
            if (!_allowInline && label.Kind != CdsAILabelKind.Default) label.Kind = CdsAILabelKind.Default;
            var size = inline ? CdsAILabelSize.Md : CdsAILabelSize.Mini;
            if (label.Size != size) label.Size = size;
        }
        // 8px after the label (Carbon: "> * { margin-inline-start: $spacing-03 }"); the inline kind sits 1px higher.
        decorator.Margin = new Thickness(CdsSpacing.Spacing03, inline ? -1 : 0, 0, 0);
    }

    /// <summary>
    /// MAUI gives a child the disabled state of its parent, which would turn off an AI label inside a disabled checkbox,
    /// radio button or group. The AI label is never disabled with its control (Carbon: "should never be disabled as part of a
    /// disabled or read-only state"), so the row is always enabled. What is inside it that must be disabled is: the hit
    /// target sets its own state.
    /// </summary>
    protected override bool IsEnabledCore => true;

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    readonly record struct Plan(double MainWidth, Size Main, Size Decorator, bool Wraps, double DecoratorX, double DecoratorY, Size Total);

    sealed class Manager(CdsDecoratedRow row) : LayoutManager(row)
    {
        IView? Main => row.Count > 0 && row[0].Visibility != Visibility.Collapsed ? row[0] : null;

        IView? Decorator => Main is not null && row.Count > 1 && row[1].Visibility != Visibility.Collapsed ? row[1] : null;

        Plan Compute(double available, double heightConstraint)
        {
            if (Main is not { } main) return default;
            // The main view takes its natural width, or all the width there is when that is less (its label then wraps).
            var natural = main.Measure(double.PositiveInfinity, heightConstraint);
            var mainWidth = double.IsFinite(available) ? Math.Min(natural.Width, Math.Max(0, available)) : natural.Width;
            var mainSize = mainWidth < natural.Width ? main.Measure(mainWidth, heightConstraint) : natural;
            if (Decorator is not { } decorator) return new Plan(mainWidth, mainSize, default, false, 0, 0, new Size(mainWidth, mainSize.Height));

            var size = decorator.Measure(double.PositiveInfinity, heightConstraint);
            // flex-wrap: wrap: the decorator goes to the next line when it does not fit after the main view.
            var wraps = double.IsFinite(available) && mainWidth + size.Width > available + 0.01;
            if (wraps)
                return new Plan(mainWidth, mainSize, size, true, 0, mainSize.Height, new Size(Math.Max(mainWidth, size.Width), mainSize.Height + size.Height));
            var height = Math.Max(mainSize.Height, size.Height);
            var y = row._centersDecorator ? (height - size.Height) / 2 : 0;
            return new Plan(mainWidth, mainSize, size, false, mainWidth, y, new Size(mainWidth + size.Width, height));
        }

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var plan = Compute(widthConstraint, heightConstraint);
            return plan.Total;
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            var plan = Compute(bounds.Width, double.PositiveInfinity);
            if (Main is { } main) main.Arrange(new Rect(bounds.X, bounds.Y, plan.MainWidth, plan.Main.Height));
            if (Decorator is { } decorator)
                decorator.Arrange(new Rect(bounds.X + plan.DecoratorX, bounds.Y + plan.DecoratorY, plan.Decorator.Width, plan.Decorator.Height));
            return plan.Total;
        }
    }
}
