using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A clickable tag that triggers an action, such as opening a popover with more detail or the full list behind a
/// "+3" tag. A native button with a 1px <c>tag-border-*</c> outline on the <c>tag-background-*</c> fill; hover switches
/// the fill to <c>tag-hover-*</c>. There is no high-contrast or outline type (they render gray). For navigation use a
/// link; to remove a value use <see cref="CdsDismissibleTag"/>; to toggle a choice use <see cref="CdsSelectableTag"/>.
/// </summary>
public class CdsOperationalTag : CdsTag
{
    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool?), typeof(CdsOperationalTag), null,
        propertyChanged: static (b, _, _) => ((CdsOperationalTag)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsOperationalTag), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsOperationalTag), null, propertyChanged: OnCommandChanged);

    CdsHitTarget? _hit;
    CdsIcon _iconView = null!;
    CdsText _labelView = null!;
    CdsFocusRing _ring = null!;

    /// <summary>Creates a medium gray operational tag.</summary>
    public CdsOperationalTag()
    {
        Pill.MinimumWidthRequest = 32;
    }

    /// <summary>Creates an operational tag.</summary>
    public CdsOperationalTag(string text, CdsTagType type = CdsTagType.Gray) : this()
    {
        Text = text;
        Type = type;
    }

    /// <summary>Raised on activation (click, tap, Enter or Space), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>
    /// When the tag opens something (a popover, a modal), whether it is open, so the state is announced
    /// (<c>aria-expanded</c>). <c>null</c> when it does not open anything.
    /// </summary>
    public bool? IsExpanded
    {
        get => (bool?)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Command run on activation.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc/>
    protected override View BuildChrome(Border pill)
    {
        CdsTagParts.Rebuild(pill, Row, out _iconView, out _labelView);
        AutomationProperties.SetExcludedWithChildren(pill, true);
        pill.InputTransparent = true;
        var hit = CreateHitTarget();
        _hit = hit;
        _ring = new CdsFocusRing(CdsFocusRingStyle.Offset, 16);
        var root = new Grid();
        root.Add(pill);
        root.Add(_ring);
        root.Add(hit);
        return root;
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_iconView is null) return;
        var v = CdsInteractiveTagTokens.Operational(Type, State);
        SetColor(Pill, BackgroundColorProperty, v.Background);
        SetBrush(Pill, Border.StrokeProperty, v.Border);
        Pill.StrokeThickness = 1;
        SetColor(_labelView, Label.TextColorProperty, v.Foreground);
        SetColor(_iconView, CdsIcon.ColorProperty, v.Foreground);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
        if (_hit is not null)
        {
            SemanticProperties.SetDescription(_hit, Text);
            _hit.IsExpanded = IsExpanded;
        }
    }

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var tag = (CdsOperationalTag)b;
        tag.SetCommand(tag.Command, tag.CommandParameter);
    }
}
