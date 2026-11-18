namespace Maui.Carbon;

/// <summary>
/// Shared base of the Carbon form fields built on <see cref="CdsFieldChrome"/> (text area, number input, select).
/// It owns the common field API (label, helper, invalid, warning, read-only, size) and keeps the chrome in step with the
/// control's state. Subclasses put their native input in <see cref="Chrome"/> and override <see cref="InputElement"/>,
/// <see cref="UpdateChrome"/> and <see cref="ApplyInput"/>. Fluid variants subclass the concrete fields.
/// </summary>
public abstract partial class CdsFormField : CdsControl
{
    /// <summary>The field scaffold (label, field box, message).</summary>
    protected readonly CdsFieldChrome Chrome = new();

    /// <summary>Creates the field and keeps it in step with the fluid context of <see cref="Chrome"/>.</summary>
    protected CdsFormField()
    {
        Chrome.FluidLayoutChanged += (_, _) => UpdateChrome();
    }

    /// <summary>
    /// Whether the field renders its fluid layout: it sits inside a <see cref="CdsFluidForm"/> or is a fluid variant
    /// (see <see cref="CdsFieldChrome.IsFluid"/>). Fluid fields ignore <see cref="Size"/>, inline mode and helper text.
    /// </summary>
    protected bool IsFluidMode => Chrome.IsFluidLayout;

    /// <summary>The native input inside the field (it receives the accessible name and hint).</summary>
    protected abstract VisualElement InputElement { get; }

    /// <summary>Whether the field is invalid. Fields with automatic validation (number input) extend this.</summary>
    protected virtual bool EffectiveInvalid => IsInvalid;

    /// <summary>The error text shown while <see cref="EffectiveInvalid"/>.</summary>
    protected virtual string? EffectiveInvalidText => InvalidText;

    /// <summary>The state flags handed to the chrome: interaction state plus invalid, warning and read-only.</summary>
    protected CdsInteractionState FieldState
    {
        get
        {
            var state = State;
            if (EffectiveInvalid) state |= CdsInteractionState.Invalid;
            if (IsWarning) state |= CdsInteractionState.Warn;
            if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
            return state;
        }
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        Chrome.Apply(FieldState);
        var input = InputElement;
        if (input is null) return;
        SemanticProperties.SetHint(input, HintText());
        ApplyInput();
    }

    /// <summary>Applies the input's own colors and enabled state (called from <see cref="ApplyVisuals"/>).</summary>
    protected virtual void ApplyInput()
    {
    }

    /// <summary>The accessibility hint: the message under the field plus anything the subclass adds.</summary>
    protected virtual string? HintText() => Chrome.CurrentMessage;

    /// <summary>Re-applies texts, sizes and accessibility after a property change.</summary>
    protected virtual void UpdateChrome()
    {
        Chrome.Label = Label;
        Chrome.HideLabel = HideLabel;
        Chrome.HelperText = HelperText;
        Chrome.InvalidText = EffectiveInvalidText;
        Chrome.WarnText = WarnText;
        Chrome.Size = Size;
        Chrome.IsInline = IsInline;
        if (InputElement is { } input) SemanticProperties.SetDescription(input, Label);
        RefreshVisuals();
    }

    static void OnChromeChanged(BindableObject b, object? o, object? n) => ((CdsFormField)b).UpdateChrome();
}
