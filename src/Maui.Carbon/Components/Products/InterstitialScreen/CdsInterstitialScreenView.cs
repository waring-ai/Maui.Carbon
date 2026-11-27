namespace Maui.Carbon;

/// <summary>
/// One step of a <see cref="CdsInterstitialScreen"/> (Carbon's <c>InterstitialScreenView</c>): its content, and the
/// <see cref="StepTitle"/> the progress indicator shows for it. Keep each step to one idea: a heading, a short paragraph
/// and, if it helps, an illustration.
/// </summary>
public class CdsInterstitialScreenView : ContentView
{
    /// <summary>Bindable property for <see cref="StepTitle"/>.</summary>
    public static readonly BindableProperty StepTitleProperty = BindableProperty.Create(
        nameof(StepTitle), typeof(string), typeof(CdsInterstitialScreenView), null,
        propertyChanged: static (b, _, _) => ((CdsInterstitialScreenView)b).StepTitleChanged?.Invoke(b, EventArgs.Empty));

    /// <summary>The step's label in the progress indicator (one or two words). "Step n" when empty.</summary>
    public string? StepTitle
    {
        get => (string?)GetValue(StepTitleProperty);
        set => SetValue(StepTitleProperty, value);
    }

    /// <summary>Raised when <see cref="StepTitle"/> changes; the screen updates its progress indicator.</summary>
    internal event EventHandler? StepTitleChanged;
}
