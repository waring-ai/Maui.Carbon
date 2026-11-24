using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// The vertical container of a Carbon form: fields stack 32 apart (<see cref="IsTight"/>: 24). Put
/// <see cref="CdsFormGroup"/>s and fields inside, with the actions last (primary action last, Cancel before it). Give
/// the form an accessible name with <see cref="CdsSemantics.GroupNameProperty"/> when the page has several forms (not
/// <c>SemanticProperties.Description</c>, which hides the fields from VoiceOver). On a failed
/// submit, move focus to the first invalid field.
/// </summary>
public class CdsForm : VerticalStackLayout
{
    /// <summary>Bindable property for <see cref="IsTight"/>.</summary>
    public static readonly BindableProperty IsTightProperty = BindableProperty.Create(
        nameof(IsTight), typeof(bool), typeof(CdsForm), false,
        propertyChanged: static (b, _, n) => ((CdsForm)b).Spacing = (bool)n ? CdsSpacing.Spacing06 : CdsSpacing.Spacing07);

    /// <summary>Bindable property for <see cref="SubmitCommand"/>.</summary>
    public static readonly BindableProperty SubmitCommandProperty = BindableProperty.Create(
        nameof(SubmitCommand), typeof(ICommand), typeof(CdsForm), null);

    /// <summary>Creates a form with 32 spacing.</summary>
    public CdsForm()
    {
        Spacing = CdsSpacing.Spacing07;
    }

    /// <summary>Raised by <see cref="Submit"/> (call it from the primary button).</summary>
    public event EventHandler? Submitted;

    /// <summary>Stack fields 24 apart instead of 32 for dense forms.</summary>
    public bool IsTight
    {
        get => (bool)GetValue(IsTightProperty);
        set => SetValue(IsTightProperty, value);
    }

    /// <summary>Command run by <see cref="Submit"/> after <see cref="Submitted"/>.</summary>
    public ICommand? SubmitCommand
    {
        get => (ICommand?)GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    /// <summary>Submits the form: raises <see cref="Submitted"/> and runs <see cref="SubmitCommand"/>.</summary>
    public void Submit()
    {
        Submitted?.Invoke(this, EventArgs.Empty);
        if (SubmitCommand?.CanExecute(null) == true) SubmitCommand.Execute(null);
    }
}

/// <summary>
/// The column every input sits in: a transparent wrapper that lines a custom control up with Carbon fields. Pair it
/// with <see cref="CdsFormLabel"/> to label a control that has no label of its own.
/// </summary>
public class CdsFormItem : VerticalStackLayout
{
    /// <summary>Creates a form item.</summary>
    public CdsFormItem()
    {
        Spacing = 0;
        HorizontalOptions = LayoutOptions.Fill;
    }
}
