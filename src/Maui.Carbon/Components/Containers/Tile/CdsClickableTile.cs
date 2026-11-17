namespace Maui.Carbon;

/// <summary>
/// A tile that is one link or one button: the whole surface is the hit target, with <c>layer-hover-01</c> on hover,
/// <c>layer-active-01</c> while pressed, a 2px inset focus ring and a trailing 20px icon in <c>icon-interactive</c>
/// (an arrow by default, <see cref="CdsIconName.Launch"/> for external destinations). It has exactly one action: put no
/// buttons, links or inputs in its body. Its accessible name is all of its text, so lead with the destination.
/// </summary>
public class CdsClickableTile : CdsTileBase
{
    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsClickableTile), null,
        propertyChanged: static (b, _, _) => ((CdsClickableTile)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsClickableTile), CdsIconName.ArrowRight,
        propertyChanged: static (b, _, _) => ((CdsClickableTile)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(
        nameof(AccessibleName), typeof(string), typeof(CdsClickableTile), null,
        propertyChanged: static (b, _, _) => ((CdsClickableTile)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(System.Windows.Input.ICommand), typeof(CdsClickableTile), null,
        propertyChanged: static (b, _, _) => ((CdsClickableTile)b).SetCommand(((CdsClickableTile)b).Command, ((CdsClickableTile)b).CommandParameter));

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsClickableTile), null,
        propertyChanged: static (b, _, _) => ((CdsClickableTile)b).SetCommand(((CdsClickableTile)b).Command, ((CdsClickableTile)b).CommandParameter));

    readonly CdsIcon _icon = new() { Size = 20, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, Margin = new Thickness(0, 0, CdsSpacing.Spacing05, CdsSpacing.Spacing05) };
    readonly CdsHitTarget _hit;

    /// <summary>Creates a clickable tile.</summary>
    public CdsClickableTile()
    {
        SealBody();
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        Stack.Add(_icon);
        Stack.Add(_hit);
        Loaded += (_, _) => UpdateChrome(); // body text may be bound after construction
        UpdateChrome();
    }

    /// <summary>Raised on activation (click, tap, Enter or Space), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>An absolute URI opened with the system launcher on activation; makes the tile a link for screen readers.</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>
    /// The trailing icon at the bottom end (default <see cref="CdsIconName.ArrowRight"/>; <see cref="CdsIconName.Launch"/> for
    /// external destinations; <c>null</c> for none, which also drops the 48px reserved under the body).
    /// </summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Overrides the accessible name, which is otherwise all of the body's text.</summary>
    public string? AccessibleName
    {
        get => (string?)GetValue(AccessibleNameProperty);
        set => SetValue(AccessibleNameProperty, value);
    }

    /// <summary>Command run on activation.</summary>
    public System.Windows.Input.ICommand? Command
    {
        get => (System.Windows.Input.ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <inheritdoc/>
    protected override CdsTileKind TileKind => CdsTileKind.Clickable;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        if (!string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }

    /// <inheritdoc/>
    protected override void OnBodyReplaced() => UpdateChrome();

    /// <inheritdoc/>
    protected override void ApplyTileVisuals(CdsTileVisuals visuals)
    {
        SetColor(_icon, CdsIcon.ColorProperty, visuals.Icon);
        _hit.IsEnabled = !Is(CdsInteractionState.Disabled);
    }

    void UpdateChrome()
    {
        // The tile reserves 48px under the body for its 20px icon.
        BodyHost.Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05,
            Icon is null ? CdsSpacing.Spacing05 : CdsSpacing.Spacing09);
        _icon.Icon = Icon;
        _icon.IsVisible = Icon is not null;
        _hit.Role = string.IsNullOrEmpty(Href) ? CdsAccessibilityRole.Button : CdsAccessibilityRole.Link;
        SemanticProperties.SetDescription(_hit, AccessibleName ?? BodyText());
        RefreshVisuals();
    }
}
