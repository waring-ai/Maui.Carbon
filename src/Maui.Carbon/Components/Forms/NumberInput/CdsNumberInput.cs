using System.Globalization;

namespace Maui.Carbon;

/// <summary>The direction of a stepper or arrow-key change.</summary>
public enum CdsNumberDirection
{
    /// <summary>The value was typed or set.</summary>
    None,

    /// <summary>Decremented.</summary>
    Down,

    /// <summary>Incremented.</summary>
    Up,
}

/// <summary>Event data of <see cref="CdsNumberInput.ValueChanged"/> and <see cref="CdsNumberInput.Stepped"/>.</summary>
public sealed class CdsNumberChangedEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsNumberChangedEventArgs(double? value, CdsNumberDirection direction)
    {
        Value = value;
        Direction = direction;
    }

    /// <summary>The new value; <c>null</c> when the field is empty.</summary>
    public double? Value { get; }

    /// <summary>How the value changed.</summary>
    public CdsNumberDirection Direction { get; }
}

/// <summary>
/// A numeric field with decrement and increment steppers, constrained by <see cref="Min"/>, <see cref="Max"/> and
/// <see cref="Step"/>. A value outside the range, or an empty field without <see cref="AllowEmpty"/>, is invalid
/// automatically. Steppers clamp to the range and keep the decimal precision of the step and the value; as in Carbon they
/// are not Tab stops (they stay clickable and visible to screen readers): keyboard users type or use Up / Down. Always set
/// a sensible default and state the range in the label or helper text.
/// </summary>
public partial class CdsNumberInput : CdsFormField
{
    /// <summary>The native entry inside the field.</summary>
    protected readonly CdsNativeEntry Entry = new() { Keyboard = Keyboard.Numeric };

    /// <summary>The decrement button.</summary>
    internal readonly CdsFieldIconButton DownButton = new(CdsIconName.Subtract) { IsTabStop = false };

    /// <summary>The increment button.</summary>
    internal readonly CdsFieldIconButton UpButton = new(CdsIconName.Add) { IsTabStop = false };

    /// <summary>The steppers (divider, decrement, divider, increment) at the end of the field.</summary>
    protected readonly HorizontalStackLayout Controls = new() { Spacing = 0, VerticalOptions = LayoutOptions.Fill, Margin = new Thickness(0, 0, -CdsSpacing.Spacing05, 0) };

    readonly BoxView _dividerStart = new() { WidthRequest = 1, HeightRequest = 16, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly BoxView _dividerMiddle = new() { WidthRequest = 1, HeightRequest = 16, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    NumberValidity _validity;
    CdsNumberDirection _direction;
    bool _fromEntry;

    /// <summary>Creates a medium number input with value 0.</summary>
    public CdsNumberInput()
    {
        DownButton.Clicked += (_, _) => StepBy(CdsNumberDirection.Down, true);
        UpButton.Clicked += (_, _) => StepBy(CdsNumberDirection.Up, true);
        Controls.Add(_dividerStart);
        Controls.Add(DownButton);
        Controls.Add(_dividerMiddle);
        Controls.Add(UpButton);
        Chrome.Trailing.Add(Controls);
        Chrome.Input = Entry;
        Entry.TextChanged += OnEntryTextChanged;
        Entry.Unfocused += (_, _) => Normalize();
        Entry.Completed += (_, _) => Normalize();
        TrackFocus(Entry);
        Entry.Text = NumberInputModel.Format(Value, Step, CultureInfo.CurrentCulture);
        SetChrome(Chrome);
        UpdateChrome();
    }

    /// <summary>Raised when the value changes by typing, stepping or <see cref="Value"/>.</summary>
    public event EventHandler<CdsNumberChangedEventArgs>? ValueChanged;

    /// <summary>Raised when a stepper button or arrow key changes the value.</summary>
    public event EventHandler<CdsNumberChangedEventArgs>? Stepped;

    /// <inheritdoc/>
    protected override VisualElement InputElement => Entry;

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected override bool EffectiveInvalid => IsInvalid || (_validity != NumberValidity.Valid && !IsReadOnly);

    /// <inheritdoc/>
    protected override string? EffectiveInvalidText => string.IsNullOrEmpty(InvalidText) ? "Number is not valid" : InvalidText;

    /// <summary>Moves the value one step up or down (as the steppers and the arrow keys do).</summary>
    public void StepBy(CdsNumberDirection direction)
    {
        if (direction != CdsNumberDirection.None) StepBy(direction, false);
    }

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (!Is(CdsInteractionState.Focused) || IsReadOnly) return false;
        switch (e.Key)
        {
            case CdsKey.Up:
                StepBy(CdsNumberDirection.Up, true);
                return true;
            case CdsKey.Down:
                StepBy(CdsNumberDirection.Down, true);
                return true;
            default:
                return false;
        }
    }

    /// <inheritdoc/>
    protected override void ApplyInput()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        Entry.IsEnabled = IsEnabled;
        SetColor(Entry, Microsoft.Maui.Controls.Entry.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(Entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        DownButton.IsEnabled = IsEnabled;
        UpButton.IsEnabled = IsEnabled;
        DownButton.Muted = IsReadOnly;
        UpButton.Muted = IsReadOnly;
        var divider = disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.BorderSubtle01;
        SetColor(_dividerMiddle, BoxView.ColorProperty, divider);
        SetColor(_dividerStart, BoxView.ColorProperty,
            EffectiveInvalid && !disabled ? CdsColorKeys.BorderSubtle01 : null);
    }

    /// <inheritdoc/>
    protected override void UpdateChrome()
    {
        _validity = NumberInputModel.Validate(Entry.Text, CultureInfo.CurrentCulture, Min, Max, AllowEmpty);
        base.UpdateChrome();
        // Fluid: the steppers are 40 square at the bottom of the 64px field whatever the size.
        var fluid = IsFluidMode;
        var height = fluid ? CdsSize.Md.Height() : Size.Height();
        // Fluid: a disabled number input keeps a border-subtle-01 bottom rule.
        Chrome.DisabledRuleColorKey = fluid ? CdsColorKeys.BorderSubtle01 : null;
        Controls.VerticalOptions = fluid ? LayoutOptions.End : LayoutOptions.Fill;
        Chrome.Trailing.VerticalOptions = fluid ? LayoutOptions.End : LayoutOptions.Center;
        Chrome.Trailing.Margin = new Thickness(0);
        DownButton.SetSize(height, height);
        UpButton.SetSize(height, height);
        DownButton.Description = DecrementLabel;
        UpButton.Description = IncrementLabel;
        Controls.IsVisible = !HideSteppers;
        Entry.Placeholder = Placeholder;
        Entry.IsReadOnly = IsReadOnly;
        RefreshVisuals();
    }

    void StepBy(CdsNumberDirection direction, bool raiseStepped)
    {
        NumberInputModel.TryParse(Entry.Text, CultureInfo.CurrentCulture, out var current);
        var next = NumberInputModel.Step(current, direction == CdsNumberDirection.Up ? 1 : -1, Step, Min, Max);
        _direction = direction;
        var changed = Value != next;
        Value = next;
        // Same value (clamped at a limit): still refresh the text and validity.
        SetEntryText(NumberInputModel.Format(next, Step, CultureInfo.CurrentCulture));
        UpdateChrome();
        _direction = CdsNumberDirection.None;
        if (raiseStepped) Stepped?.Invoke(this, new CdsNumberChangedEventArgs(next, direction));
        if (!changed) ValueChanged?.Invoke(this, new CdsNumberChangedEventArgs(next, direction));
    }

    void OnValuePropertyChanged(double? value)
    {
        if (!_fromEntry) SetEntryText(NumberInputModel.Format(value, Step, CultureInfo.CurrentCulture));
        UpdateChrome();
        ValueChanged?.Invoke(this, new CdsNumberChangedEventArgs(value, _direction));
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        NumberInputModel.TryParse(e.NewTextValue, CultureInfo.CurrentCulture, out var parsed);
        _fromEntry = true;
        try
        {
            Value = parsed;
        }
        finally
        {
            _fromEntry = false;
        }
        UpdateChrome();
    }

    // Reformats a valid number to its precision when the user leaves the field.
    void Normalize()
    {
        if (NumberInputModel.TryParse(Entry.Text, CultureInfo.CurrentCulture, out var value) && value is not null)
            SetEntryText(NumberInputModel.Format(value, Step, CultureInfo.CurrentCulture));
    }

    void SetEntryText(string text)
    {
        if (Entry.Text == text) return;
        var previous = _fromEntry;
        _fromEntry = true;
        try
        {
            Entry.Text = text;
        }
        finally
        {
            _fromEntry = previous;
        }
    }
}
