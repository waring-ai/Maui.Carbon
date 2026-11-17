using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A read-only pill that labels or categorizes an item (a status, a region, a technology). Use the same color for the
/// same category everywhere. Labels never wrap: they truncate at 208 and the full text is the tooltip. For a removable
/// filter use <c>CdsDismissibleTag</c>; for a tag that opens detail, <c>CdsOperationalTag</c>.
/// </summary>
public partial class CdsTag : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTag), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsTagType), typeof(CdsTag), CdsTagType.Gray, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsTagSize), typeof(CdsTag), CdsTagSize.Md, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsTag), null, propertyChanged: OnLayoutChanged);

    /// <summary>The pill (fill, outline and radius).</summary>
    protected readonly Border Pill = new() { StrokeThickness = 0, Padding = 0, MinimumWidthRequest = 32, MaximumWidthRequest = 208 };

    /// <summary>The row inside the pill: [icon][label][trailing].</summary>
    protected readonly HorizontalStackLayout Row = new() { Spacing = 0, VerticalOptions = LayoutOptions.Center };

    readonly CdsIcon _icon = new() { IsVisible = false, Margin = new Thickness(0, 0, CdsSpacing.Spacing02, 0) };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };

    /// <summary>Creates a medium gray tag.</summary>
    public CdsTag()
    {
        Row.Add(_icon);
        Row.Add(_label);
        Pill.Content = Row;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        SetChrome(BuildChrome(Pill));
        UpdateLayout();
    }

    /// <summary>Creates a tag with a label and color.</summary>
    public CdsTag(string text, CdsTagType type = CdsTagType.Gray) : this()
    {
        Text = text;
        Type = type;
    }

    /// <summary>The label: a few words, under about 20 characters, sentence case unless it is a value.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The color. Default <see cref="CdsTagType.Gray"/>.</summary>
    public CdsTagType Type
    {
        get => (CdsTagType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>Height: sm 18, md 24 (default), lg 32.</summary>
    public CdsTagSize Size
    {
        get => (CdsTagSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>A decorative 16px icon before the label; not drawn at sm.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The colors in effect (for subclasses that add hover or a close button).</summary>
    protected CdsTagVisuals CurrentVisuals => CdsTagTokens.Resolve(Type, Is(CdsInteractionState.Disabled));

    /// <summary>Builds the control's visual tree around the pill. Interactive subclasses add a hit target and focus ring.</summary>
    protected virtual View BuildChrome(Border pill) => pill;

    /// <summary>Extra end padding the subclass needs (a close button sits flush with the end edge).</summary>
    protected virtual bool HasTrailingContent => false;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CurrentVisuals;
        SetColor(Pill, BackgroundColorProperty, v.Background);
        SetBrush(Pill, Border.StrokeProperty, v.Border);
        Pill.StrokeThickness = v.Border is null ? 0 : 1;
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Foreground);
    }

    /// <summary>Re-applies layout after a property change.</summary>
    protected void UpdateLayout()
    {
        var height = CdsTagTokens.Height(Size);
        var hasIcon = Icon is not null && Size != CdsTagSize.Sm;
        Pill.HeightRequest = height;
        Pill.MinimumHeightRequest = height;
        Pill.StrokeShape = new RoundRectangle { CornerRadius = CdsRadius.Circle(height) };

        var start = hasIcon ? (Size == CdsTagSize.Lg ? CdsSpacing.Spacing03 : CdsSpacing.Spacing02)
            : (Size == CdsTagSize.Lg ? CdsSpacing.Spacing05 : CdsSpacing.Spacing03);
        var end = HasTrailingContent ? 0 : (Size == CdsTagSize.Lg ? CdsSpacing.Spacing05 : CdsSpacing.Spacing03);
        Pill.Padding = new Thickness(start, 0, end, 0);

        _icon.Icon = Icon;
        _icon.IsVisible = hasIcon;
        _label.Text = Text;
        // The row measures its children unconstrained, so cap the label for the ellipsis to appear inside 208.
        var trailing = HasTrailingContent ? height : 0;
        _label.MaximumWidthRequest = Math.Max(0, Pill.MaximumWidthRequest - start - end - (hasIcon ? 16 + CdsSpacing.Spacing02 : 0) - trailing);
        if (!string.IsNullOrEmpty(Text)) ToolTipProperties.SetText(Pill, Text);
        RefreshVisuals();
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsTag)b).UpdateLayout();
}
