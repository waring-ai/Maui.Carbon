namespace Maui.Carbon;

/// <summary>
/// A Carbon label (<c>label-01</c>, <c>text-secondary</c>) for a control that has no label of its own, with an optional
/// toggletip slot after the text (<see cref="CdsContainer.Body"/>). Tapping the label focuses <see cref="For"/>, and the
/// label text becomes the accessible name of <see cref="For"/> (Carbon's <c>&lt;label for&gt;</c>), so screen readers read
/// it with the control instead of as a separate text. It sits 8 above the control unless <see cref="HasMargin"/> is off.
/// </summary>
public class CdsFormLabel : CdsContainer
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsFormLabel), null,
        propertyChanged: static (b, _, n) =>
        {
            var label = (CdsFormLabel)b;
            label._text.Text = (string?)n;
            label.NameTarget();
        });

    /// <summary>Bindable property for <see cref="HasMargin"/>.</summary>
    public static readonly BindableProperty HasMarginProperty = BindableProperty.Create(
        nameof(HasMargin), typeof(bool), typeof(CdsFormLabel), true,
        propertyChanged: static (b, _, n) => ((CdsFormLabel)b)._root.Margin = (bool)n ? new Thickness(0, 0, 0, CdsSpacing.Spacing03) : new Thickness(0));

    readonly Grid _root = new() { ColumnSpacing = CdsSpacing.Spacing02, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center };
    readonly ContentView _slot = new() { VerticalOptions = LayoutOptions.Center };
    VisualElement? _for;
    // The element named last and the name given to it, so a name the app set itself is never overwritten or removed.
    VisualElement? _named;
    string? _givenName;

    /// <summary>Creates a label.</summary>
    public CdsFormLabel()
    {
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.Add(_text, 0);
        _root.Add(_slot, 1);
        HorizontalOptions = LayoutOptions.Start;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (For is CdsControl c) c.Focus();
            else For?.Focus();
        };
        _text.GestureRecognizers.Add(tap);
        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>The label text: short, sentence case, no colon.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Keeps the 8 margin below the label (Carbon's <c>label--no-margin</c> turns it off).</summary>
    public bool HasMargin
    {
        get => (bool)GetValue(HasMarginProperty);
        set => SetValue(HasMarginProperty, value);
    }

    /// <summary>
    /// The control this label names: tapping the label focuses it, and <see cref="Text"/> becomes its accessible name
    /// (on the control's hit target or input for a Carbon control) unless the control already has a
    /// <c>SemanticProperties.Description</c> of its own. While it names a control, the label text itself is hidden from
    /// screen readers so it is not read twice.
    /// </summary>
    public VisualElement? For
    {
        get => _for;
        set
        {
            if (ReferenceEquals(_for, value)) return;
            _for = value;
            NameTarget();
        }
    }

    /// <summary>The element that receives the accessible name for <paramref name="target"/>.</summary>
    static VisualElement NameTargetOf(VisualElement target) =>
        target is CdsControl { AccessibilityTarget: { } inner } ? inner : target;

    void NameTarget()
    {
        var target = _for is null ? null : NameTargetOf(_for);
        // Take the name back from the element named before (unless the app has replaced it since).
        if (_named is not null && !ReferenceEquals(_named, target))
        {
            if (SemanticProperties.GetDescription(_named) == _givenName) SemanticProperties.SetDescription(_named, null);
            _named = null;
            _givenName = null;
        }
        if (target is not null)
        {
            var current = SemanticProperties.GetDescription(target);
            var ours = ReferenceEquals(_named, target) && current == _givenName;
            if (string.IsNullOrEmpty(current) || ours)
            {
                var name = string.IsNullOrEmpty(Text) ? null : Text;
                SemanticProperties.SetDescription(target, name);
                _named = name is null ? null : target;
                _givenName = name;
            }
        }
        // The control carries the name: keep the label text out of the tree so it is not read as a separate element.
        AutomationProperties.SetIsInAccessibleTree(_text, _named is null);
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _slot.Content = newBody;

    /// <inheritdoc/>
    protected override void ApplyVisuals() =>
        SetColor(_text, Microsoft.Maui.Controls.Label.TextColorProperty,
            Is(CdsInteractionState.Disabled) ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
}
