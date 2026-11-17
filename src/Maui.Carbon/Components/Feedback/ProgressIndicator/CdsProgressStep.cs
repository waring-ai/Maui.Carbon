namespace Maui.Carbon;

/// <summary>
/// One step of a <see cref="CdsProgressIndicator"/>. Labels are one or two words, ideally verb + noun ("Choose plan");
/// mark optional steps and say how to fix an invalid one in <see cref="SecondaryLabel"/>.
/// </summary>
public class CdsProgressStep : BindableObject
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsProgressStep), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="SecondaryLabel"/>.</summary>
    public static readonly BindableProperty SecondaryLabelProperty = BindableProperty.Create(
        nameof(SecondaryLabel), typeof(string), typeof(CdsProgressStep), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsProgressStep), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsProgressStep), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsDisabled"/>.</summary>
    public static readonly BindableProperty IsDisabledProperty = BindableProperty.Create(
        nameof(IsDisabled), typeof(bool), typeof(CdsProgressStep), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsComplete"/>.</summary>
    public static readonly BindableProperty IsCompleteProperty = BindableProperty.Create(
        nameof(IsComplete), typeof(bool), typeof(CdsProgressStep), false, propertyChanged: OnChanged);

    /// <summary>Creates an empty step.</summary>
    public CdsProgressStep()
    {
    }

    /// <summary>Creates a step with a label.</summary>
    public CdsProgressStep(string label, string? secondaryLabel = null)
    {
        Label = label;
        SecondaryLabel = secondaryLabel;
    }

    /// <summary>Raised when a property changed; the indicator redraws.</summary>
    internal event EventHandler? Changed;

    /// <summary>The step name. Required.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Small text under the label ("Optional", or the hint to fix an invalid step).</summary>
    public string? SecondaryLabel
    {
        get => (string?)GetValue(SecondaryLabelProperty);
        set => SetValue(SecondaryLabelProperty, value);
    }

    /// <summary>The accessible name of the step's icon. Keep it consistent with the visible label.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The step needs fixing: a <c>support-error</c> warning icon.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>The step cannot be used: skipped by Tab and not clickable.</summary>
    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    /// <summary>Forces the complete state on a step after the current one.</summary>
    public bool IsComplete
    {
        get => (bool)GetValue(IsCompleteProperty);
        set => SetValue(IsCompleteProperty, value);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsProgressStep)b).Changed?.Invoke(b, EventArgs.Empty);
}
