namespace Maui.Carbon;

/// <summary>
/// A fieldset: a legend (<c>label-01</c>, <c>text-secondary</c>) naming a set of related fields, the fields
/// (<see cref="CdsContainer.Body"/>), and an optional message under the group (<c>helper-text-01</c>, or
/// <c>text-error</c> when <see cref="IsInvalid"/>). Setting <c>IsEnabled</c> to false disables every nested control.
/// Use one group per related set; do not nest groups for spacing.
/// </summary>
public class CdsFormGroup : CdsContainer
{
    /// <summary>Bindable property for <see cref="Legend"/>.</summary>
    public static readonly BindableProperty LegendProperty = BindableProperty.Create(
        nameof(Legend), typeof(string), typeof(CdsFormGroup), null,
        propertyChanged: static (b, _, n) => ((CdsFormGroup)b).OnLegendChanged((string?)n));

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsFormGroup), false,
        propertyChanged: static (b, _, _) => ((CdsFormGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="MessageText"/>.</summary>
    public static readonly BindableProperty MessageTextProperty = BindableProperty.Create(
        nameof(MessageText), typeof(string), typeof(CdsFormGroup), null,
        propertyChanged: static (b, _, _) => ((CdsFormGroup)b).RefreshVisuals());

    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsText _legend = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly ContentView _slot = new();
    readonly CdsText _message = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, IsVisible = false, Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0) };
    readonly List<VisualElement> _disabledByGroup = [];

    /// <summary>Creates a form group.</summary>
    public CdsFormGroup()
    {
        _root.Add(_legend);
        _root.Add(_slot);
        _root.Add(_message);
        SemanticProperties.SetHeadingLevel(_legend, SemanticHeadingLevel.None);
        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>The legend: names the set of related fields (location, billing contact, zones).</summary>
    public string? Legend
    {
        get => (string?)GetValue(LegendProperty);
        set => SetValue(LegendProperty, value);
    }

    /// <summary>Marks the group invalid: the message turns <c>text-error</c>.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>A note under the group (what the group needs, or what is wrong).</summary>
    public string? MessageText
    {
        get => (string?)GetValue(MessageTextProperty);
        set => SetValue(MessageTextProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        if (!IsEnabled) Restore();
        _slot.Content = newBody;
        if (!IsEnabled) Disable();
    }

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (((previous ^ State) & CdsInteractionState.Disabled) == 0) return;
        if (Is(CdsInteractionState.Disabled)) Disable();
        else Restore();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_legend, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        _message.Text = MessageText;
        _message.IsVisible = !string.IsNullOrEmpty(MessageText);
        SetColor(_message, Microsoft.Maui.Controls.Label.TextColorProperty,
            disabled ? CdsColorKeys.TextDisabled : IsInvalid ? CdsColorKeys.TextError : CdsColorKeys.TextHelper);
        SemanticProperties.SetHint(_root, MessageText);
    }

    void OnLegendChanged(string? legend)
    {
        _legend.Text = legend;
        _legend.IsVisible = !string.IsNullOrEmpty(legend);
        SemanticProperties.SetDescription(_root, legend);
    }

    void Disable()
    {
        foreach (var element in Descendants(_slot.Content))
        {
            if (!element.IsEnabled) continue;
            element.IsEnabled = false;
            _disabledByGroup.Add(element);
        }
    }

    void Restore()
    {
        foreach (var element in _disabledByGroup) element.IsEnabled = true;
        _disabledByGroup.Clear();
    }

    // Disables nested controls but does not descend into Carbon controls: they style their own parts.
    static IEnumerable<VisualElement> Descendants(Element? root)
    {
        if (root is VisualElement ve) yield return ve;
        if (root is CdsControl || root is not IVisualTreeElement tree) yield break;
        foreach (var child in tree.GetVisualChildren())
        {
            if (child is not Element element) continue;
            foreach (var d in Descendants(element)) yield return d;
        }
    }
}
