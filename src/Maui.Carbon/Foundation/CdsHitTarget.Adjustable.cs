namespace Maui.Carbon;

// The adjustable hook of a hit target: the value a screen reader speaks (aria-valuetext) and the increment and decrement
// requests assistive technology makes (VoiceOver swipe up or down / VO+arrows, TalkBack scroll forward or backward).
public sealed partial class CdsHitTarget
{
    /// <summary>Bindable property for <see cref="AccessibleValue"/>.</summary>
    public static readonly BindableProperty AccessibleValueProperty = BindableProperty.Create(
        nameof(AccessibleValue), typeof(string), typeof(CdsHitTarget), null);

    /// <summary>
    /// The value a screen reader speaks for the target (the web's <c>aria-valuetext</c>), such as a slider thumb's
    /// formatted value ("120 GB"). Reported as the element's value (Apple's accessibility value, Android's state
    /// description, the UI Automation value) ahead of any state words. <c>null</c> (default) when the target has no value.
    /// Set it for every <see cref="CdsAccessibilityRole.Adjustable"/> target, and keep it current as the value changes.
    /// </summary>
    public string? AccessibleValue
    {
        get => (string?)GetValue(AccessibleValueProperty);
        set => SetValue(AccessibleValueProperty, value);
    }

    /// <summary>
    /// Raised when assistive technology asks an <see cref="CdsAccessibilityRole.Adjustable"/> target to increase its
    /// value by one step: VoiceOver's swipe up (iOS) or VO+Up (Mac Catalyst), TalkBack's scroll forward. The owning
    /// control steps its value as for the Right or Up arrow and updates <see cref="AccessibleValue"/>; the screen reader
    /// then reads the new value itself, so do not announce it as well. Not raised while disabled or read only.
    /// </summary>
    public event EventHandler? Incremented;

    /// <summary>
    /// Raised when assistive technology asks an <see cref="CdsAccessibilityRole.Adjustable"/> target to decrease its
    /// value by one step (VoiceOver's swipe down or VO+Down, TalkBack's scroll backward). See <see cref="Incremented"/>.
    /// </summary>
    public event EventHandler? Decremented;

    /// <summary>Whether the target takes increment and decrement requests: adjustable, enabled and not read only.</summary>
    internal bool CanAdjust => Role == CdsAccessibilityRole.Adjustable && IsEnabled && !IsReadOnly;

    /// <summary>
    /// Called by the platform handlers when assistive technology asks to step the value; raises
    /// <see cref="Incremented"/> or <see cref="Decremented"/>. Returns <c>false</c> when the target cannot be adjusted.
    /// </summary>
    internal bool RequestAdjust(bool increase)
    {
        if (!CanAdjust) return false;
        (increase ? Incremented : Decremented)?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
