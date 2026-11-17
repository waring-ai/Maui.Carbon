namespace Maui.Carbon;

/// <summary>
/// A single-line text field for short, unique values (names, IDs, addresses): a flat <c>field-01</c> rectangle with a
/// 1px <c>border-strong-01</c> bottom rule. Put fields on a <c>layer-01</c> surface inside a <see cref="CdsLayer"/>
/// so they step to <c>field-02</c>. Validate on blur or submit, not on every keystroke.
/// </summary>
public partial class CdsTextInput : CdsControl
{
    /// <summary>The field scaffold (label, field box, message).</summary>
    protected readonly CdsFieldChrome Chrome = new();

    /// <summary>The native entry inside the field.</summary>
    protected readonly CdsNativeEntry Entry = new();

    /// <summary>Creates a medium text input.</summary>
    public CdsTextInput()
    {
        Chrome.Input = Entry;
        Entry.TextChanged += OnEntryTextChanged;
        Entry.Completed += (_, _) => Completed?.Invoke(this, EventArgs.Empty);
        TrackFocus(Entry);
        SetChrome(Chrome);
        UpdateChrome();
    }

    /// <summary>Raised when the text changes (typing or <see cref="Text"/>).</summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <summary>Raised when the user presses Return.</summary>
    public event EventHandler? Completed;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        if (IsInvalid) state |= CdsInteractionState.Invalid;
        if (IsWarning) state |= CdsInteractionState.Warn;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        Chrome.Apply(state);
        SetColor(Entry, Microsoft.Maui.Controls.Entry.TextColorProperty,
            Is(CdsInteractionState.Disabled) ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(Entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty,
            Is(CdsInteractionState.Disabled) ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        SemanticProperties.SetHint(Entry, Chrome.CurrentMessage);
    }

    /// <summary>Re-applies texts, sizes and accessibility after a property change.</summary>
    protected virtual void UpdateChrome()
    {
        Chrome.Label = Label;
        Chrome.HideLabel = HideLabel;
        Chrome.HelperText = HelperText;
        Chrome.InvalidText = InvalidText;
        Chrome.WarnText = WarnText;
        Chrome.Size = Size;
        Chrome.IsInline = IsInline;
        Entry.Placeholder = Placeholder;
        Entry.IsReadOnly = IsReadOnly;
        Entry.MaxLength = EnableCounter && MaxCount > 0 ? MaxCount : int.MaxValue;
        Chrome.Counter = EnableCounter && MaxCount > 0 ? $"{Entry.Text?.Length ?? 0}/{MaxCount}" : null;
        SemanticProperties.SetDescription(Entry, Label);
        RefreshVisuals();
    }

    void OnTextPropertyChanged(string? text)
    {
        if (Entry.Text != text) Entry.Text = text;
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        Text = e.NewTextValue;
        if (EnableCounter && MaxCount > 0) Chrome.Counter = $"{e.NewTextValue?.Length ?? 0}/{MaxCount}";
        TextChanged?.Invoke(this, e);
    }
}
