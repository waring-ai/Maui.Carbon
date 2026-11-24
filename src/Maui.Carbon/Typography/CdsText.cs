namespace Maui.Carbon;

/// <summary>
/// A label that renders a Carbon type style. Text defaults to <c>text-primary</c>; set <see cref="Tone"/> for the other
/// Carbon text colors, or set <c>TextColor</c> yourself. Fluid styles step up at the page breakpoint
/// (<see cref="CdsContextKeys.Breakpoint"/>, see <see cref="CdsFluidType"/>).
/// </summary>
public class CdsText : Label
{
    /// <summary>Bindable property for <see cref="TypeStyle"/>.</summary>
    public static readonly BindableProperty TypeStyleProperty = BindableProperty.Create(
        nameof(TypeStyle), typeof(CdsTypeStyle), typeof(CdsText), CdsTypeStyle.BodyCompact01,
        propertyChanged: static (b, _, _) => ((CdsText)b).ApplyTypeStyle());

    // The page breakpoint, read only while the style is fluid (one dynamic resource per fluid label, none for the rest).
    static readonly BindableProperty ContextBreakpointProperty = BindableProperty.Create(
        "ContextBreakpoint", typeof(CdsBreakpoint?), typeof(CdsText), null,
        propertyChanged: static (b, _, _) => ((CdsText)b).ApplyTypeStyle());

    bool _tracksBreakpoint;

    /// <summary>Bindable property for <see cref="Tone"/>.</summary>
    public static readonly BindableProperty ToneProperty = BindableProperty.Create(
        nameof(Tone), typeof(CdsTextTone), typeof(CdsText), CdsTextTone.Primary,
        propertyChanged: static (b, _, n) => ((CdsText)b).ApplyTone((CdsTextTone)n));

    /// <summary>Creates a label in <c>body-compact-01</c> and <c>text-primary</c>.</summary>
    public CdsText()
    {
        CdsTypography.Apply(this, CdsTypeStyle.BodyCompact01);
        ApplyTone(CdsTextTone.Primary);
    }

    /// <summary>Creates a label with text and a type style.</summary>
    public CdsText(string text, CdsTypeStyle style = CdsTypeStyle.BodyCompact01, CdsTextTone tone = CdsTextTone.Primary) : this()
    {
        Text = text;
        TypeStyle = style;
        Tone = tone;
    }

    /// <summary>The Carbon type style. Default <c>body-compact-01</c>.</summary>
    public CdsTypeStyle TypeStyle
    {
        get => (CdsTypeStyle)GetValue(TypeStyleProperty);
        set => SetValue(TypeStyleProperty, value);
    }

    /// <summary>The Carbon text color role. Default <see cref="CdsTextTone.Primary"/>.</summary>
    public CdsTextTone Tone
    {
        get => (CdsTextTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    void ApplyTone(CdsTextTone tone) => this.SetDynamicResource(TextColorProperty, tone.ColorKey());

    void ApplyTypeStyle()
    {
        var style = TypeStyle;
        if (!CdsFluidType.IsFluid(style))
        {
            CdsTypography.Apply(this, style);
            return;
        }
        if (!_tracksBreakpoint)
        {
            _tracksBreakpoint = true;
            // Re-enters through ContextBreakpointProperty's callback when the page already publishes a breakpoint.
            SetDynamicResource(ContextBreakpointProperty, CdsContextKeys.Breakpoint);
        }
        CdsTypography.Apply(this, style, (CdsBreakpoint?)GetValue(ContextBreakpointProperty) ?? CdsBreakpoint.Sm);
    }
}

/// <summary>Carbon text color roles.</summary>
public enum CdsTextTone
{
    /// <summary><c>text-primary</c>: primary content.</summary>
    Primary,
    /// <summary><c>text-secondary</c>: labels and secondary copy.</summary>
    Secondary,
    /// <summary><c>text-helper</c>: helper text.</summary>
    Helper,
    /// <summary><c>text-error</c>: error messages.</summary>
    Error,
    /// <summary><c>text-placeholder</c>: placeholder text (intentionally low contrast).</summary>
    Placeholder,
    /// <summary><c>text-disabled</c>: disabled text (intentionally low contrast).</summary>
    Disabled,
    /// <summary><c>text-on-color</c>: text on saturated fills.</summary>
    OnColor,
    /// <summary><c>text-inverse</c>: text on <c>background-inverse</c>.</summary>
    Inverse,
    /// <summary><c>link-primary</c>: interactive text.</summary>
    Link,
}

/// <summary>Maps <see cref="CdsTextTone"/> to color keys.</summary>
public static class CdsTextToneExtensions
{
    /// <summary>The color key for a tone.</summary>
    public static string ColorKey(this CdsTextTone tone) => tone switch
    {
        CdsTextTone.Secondary => CdsColorKeys.TextSecondary,
        CdsTextTone.Helper => CdsColorKeys.TextHelper,
        CdsTextTone.Error => CdsColorKeys.TextError,
        CdsTextTone.Placeholder => CdsColorKeys.TextPlaceholder,
        CdsTextTone.Disabled => CdsColorKeys.TextDisabled,
        CdsTextTone.OnColor => CdsColorKeys.TextOnColor,
        CdsTextTone.Inverse => CdsColorKeys.TextInverse,
        CdsTextTone.Link => CdsColorKeys.LinkPrimary,
        _ => CdsColorKeys.TextPrimary,
    };
}
