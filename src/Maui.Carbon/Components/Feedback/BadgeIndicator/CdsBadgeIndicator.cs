using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>Formatting of badge counts (pure logic).</summary>
internal static class BadgeIndicatorModel
{
    /// <summary>The text of a count: empty for a dot (no count), the number, or "{max}+" above <paramref name="max"/>.</summary>
    public static string Format(int? count, int max)
    {
        if (count is not { } n || n <= 0) return "";
        return n > max ? $"{max}+" : n.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>True when the badge is a dot (no positive count).</summary>
    public static bool IsDot(int? count) => count is not > 0;
}

/// <summary>
/// A red dot or count pill for the top-right corner of its parent, used on icon buttons for unread or pending
/// items. It is a standalone view: put it in the same grid cell as the button, on top, and it pins itself to the
/// top-right with Carbon's offsets (8px for a dot, 4px for a count). Fill is <c>support-error</c>, text
/// <c>text-on-color</c>. Use one badge per control and use them sparingly. The badge has no role: describe the count in
/// the button's label, or tie it with the button's accessible description. A dot has no text, so say the new state in
/// the button label ("Activity, new items").
/// </summary>
public class CdsBadgeIndicator : CdsControl
{
    /// <summary>Bindable property for <see cref="Count"/>.</summary>
    public static readonly BindableProperty CountProperty = BindableProperty.Create(
        nameof(Count), typeof(int?), typeof(CdsBadgeIndicator), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="MaxCount"/>.</summary>
    public static readonly BindableProperty MaxCountProperty = BindableProperty.Create(
        nameof(MaxCount), typeof(int), typeof(CdsBadgeIndicator), 999,
        coerceValue: static (_, v) => Math.Max(1, (int)v), propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsCompactHost"/>.</summary>
    public static readonly BindableProperty IsCompactHostProperty = BindableProperty.Create(
        nameof(IsCompactHost), typeof(bool), typeof(CdsBadgeIndicator), false, propertyChanged: OnChanged);

    readonly Border _pill = new() { Padding = new Thickness(CdsSpacing.Spacing02, 0, CdsSpacing.Spacing02, 2), StrokeThickness = 0 };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.OnColor, MaxLines = 1, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };

    /// <summary>Creates a dot.</summary>
    public CdsBadgeIndicator()
    {
        _pill.Content = _text;
        _pill.StrokeShape = new RoundRectangle { CornerRadius = 100 };
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Start;
        InputTransparent = true;
        CascadeInputTransparent = true;
        SetChrome(_pill);
        Update();
    }

    /// <summary>Creates a count badge.</summary>
    public CdsBadgeIndicator(int count) : this()
    {
        Count = count;
    }

    /// <summary>The number of new items. <c>null</c> or 0 shows a plain 8px dot; use a count when the number matters.</summary>
    public int? Count
    {
        get => (int?)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>Counts above this show "{MaxCount}+". Default 999 (Carbon's "999+").</summary>
    public int MaxCount
    {
        get => (int)GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>The host is a small (32) button: the badge then sits 4px from the corner even as a dot (a dot otherwise sits 8px).</summary>
    public bool IsCompactHost
    {
        get => (bool)GetValue(IsCompactHostProperty);
        set => SetValue(IsCompactHostProperty, value);
    }

    /// <summary>The text shown for the current count (empty for a dot).</summary>
    public string DisplayText => BadgeIndicatorModel.Format(Count, MaxCount);

    /// <inheritdoc/>
    protected override void ApplyVisuals() => SetColor(_pill, BackgroundColorProperty, CdsColorKeys.SupportError);

    void Update()
    {
        var dot = BadgeIndicatorModel.IsDot(Count);
        _text.Text = DisplayText;
        _text.IsVisible = !dot;
        _pill.MinimumWidthRequest = 8;
        _pill.MinimumHeightRequest = 8;
        _pill.MaximumHeightRequest = 16;
        if (dot)
        {
            _pill.WidthRequest = 8;
            _pill.HeightRequest = 8;
            _pill.Padding = 0;
        }
        else
        {
            _pill.WidthRequest = -1;
            _pill.HeightRequest = 16;
            _pill.Padding = new Thickness(CdsSpacing.Spacing02, 0, CdsSpacing.Spacing02, 2);
        }
        var offset = dot && !IsCompactHost ? CdsSpacing.Spacing03 : CdsSpacing.Spacing02;
        Margin = new Thickness(0, offset, offset, 0);
        // A count is plain text read after the host button; a dot has no text and stays out of the tree.
        AutomationProperties.SetIsInAccessibleTree(this, !dot);
        RefreshVisuals();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsBadgeIndicator)b).Update();
}
