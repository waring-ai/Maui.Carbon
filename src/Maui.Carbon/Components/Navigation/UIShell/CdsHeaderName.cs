using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The product name at the start of the header: a regular-weight <see cref="Prefix"/> ("IBM") followed by the semibold
/// <see cref="Text"/>, 14px, <c>text-primary</c>, 32 of padding after it (16 before; 8 after the menu button). Keep it
/// brief. It is a link home: handle <see cref="CdsShellItem.Clicked"/> or bind <see cref="CdsShellItem.Command"/>. Focus is a 2px
/// <c>focus</c> border.
/// </summary>
public class CdsHeaderName : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsHeaderName), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Prefix"/>.</summary>
    public static readonly BindableProperty PrefixProperty = BindableProperty.Create(
        nameof(Prefix), typeof(string), typeof(CdsHeaderName), "IBM", propertyChanged: OnChanged);

    readonly Grid _root = new() { HeightRequest = UIShellModel.HeaderHeight };
    readonly Border _box = new() { StrokeThickness = 2, StrokeShape = new Rectangle(), InputTransparent = true };
    readonly Label _label = new() { VerticalOptions = LayoutOptions.Center, InputTransparent = true, LineBreakMode = LineBreakMode.NoWrap };
    readonly CdsHitTarget _hit;
    bool _afterMenuButton;

    /// <summary>Creates a name.</summary>
    public CdsHeaderName()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        AutomationProperties.SetExcludedWithChildren(_box, true);
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _label.SetDynamicResource(Label.TextColorProperty, CdsColorKeys.TextPrimary);
        _hit = CreateHitTarget(CdsAccessibilityRole.Link);
        _root.Add(_box);
        _root.Add(_label);
        _root.Add(_hit);
        SetChrome(_root);
        Update();
    }

    /// <summary>Creates a name.</summary>
    public CdsHeaderName(string text, string prefix = "IBM") : this()
    {
        Text = text;
        Prefix = prefix;
    }

    /// <summary>The product name ("Platform").</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The regular-weight prefix before the name. Default "IBM"; an empty string hides it.</summary>
    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    /// <summary>The name follows a visible menu button, so it starts 8 (not 16) in (set by <see cref="CdsHeader"/>).</summary>
    internal void SetAfterMenuButton(bool value)
    {
        if (_afterMenuButton == value) return;
        _afterMenuButton = value;
        Update();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetBrush(_box, Border.StrokeProperty, State.HasFlag(CdsInteractionState.FocusVisible) ? CdsColorKeys.Focus : null);
    }

    void Update()
    {
        var text = new FormattedString();
        if (!string.IsNullOrEmpty(Prefix))
        {
            var prefix = new Span { Text = Prefix + " " };
            CdsTypography.Apply(prefix, CdsTypeStyle.BodyCompact01);
            text.Spans.Add(prefix);
        }
        var name = new Span { Text = Text };
        CdsTypography.Apply(name, CdsTypeStyle.HeadingCompact01);
        text.Spans.Add(name);
        _label.FormattedText = text;
        // 2px border + padding: 16 (8 after the menu button) before, 32 after.
        _label.Margin = new Thickness((_afterMenuButton ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05) + 2, 0, CdsSpacing.Spacing07 + 2, 0);
        var description = string.IsNullOrEmpty(Prefix) ? Text : $"{Prefix} {Text}";
        SemanticProperties.SetDescription(_hit, description);
        RefreshVisuals();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsHeaderName)b).Update();
}
