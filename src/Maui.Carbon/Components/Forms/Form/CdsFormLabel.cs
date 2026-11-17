namespace Maui.Carbon;

/// <summary>
/// A Carbon label (<c>label-01</c>, <c>text-secondary</c>) for a control that has no label of its own, with an optional
/// toggletip slot after the text (<see cref="CdsContainer.Body"/>). Tapping the label focuses <see cref="For"/>.
/// It sits 8 above the control unless <see cref="HasMargin"/> is off.
/// </summary>
public class CdsFormLabel : CdsContainer
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsFormLabel), null,
        propertyChanged: static (b, _, n) => ((CdsFormLabel)b)._text.Text = (string?)n);

    /// <summary>Bindable property for <see cref="HasMargin"/>.</summary>
    public static readonly BindableProperty HasMarginProperty = BindableProperty.Create(
        nameof(HasMargin), typeof(bool), typeof(CdsFormLabel), true,
        propertyChanged: static (b, _, n) => ((CdsFormLabel)b)._root.Margin = (bool)n ? new Thickness(0, 0, 0, CdsSpacing.Spacing03) : new Thickness(0));

    readonly Grid _root = new() { ColumnSpacing = CdsSpacing.Spacing02, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center };
    readonly ContentView _slot = new() { VerticalOptions = LayoutOptions.Center };

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

    /// <summary>The control this label names; tapping the label focuses it.</summary>
    public VisualElement? For { get; set; }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _slot.Content = newBody;

    /// <inheritdoc/>
    protected override void ApplyVisuals() =>
        SetColor(_text, Microsoft.Maui.Controls.Label.TextColorProperty,
            Is(CdsInteractionState.Disabled) ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
}
