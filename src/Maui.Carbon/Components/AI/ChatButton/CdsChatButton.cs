using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A rounded (pill) button for chat and AI assistant surfaces: send, stop generating, attach, regenerate. It has the same colors
/// and layout as <see cref="CdsButton"/> with a pill radius (24 at lg, 20 at md, 16 at sm; the label is centered when there is no
/// icon). <see cref="IsQuickAction"/> forces a small ghost button drawn as an outlined pill for suggested prompts and follow-ups
/// (two to four after an assistant message, at most one <see cref="IsSelected"/>); a selected quick action reports itself as
/// pressed to screen readers. Keep one primary action (Send) per composer; use <see cref="CdsButton"/> outside chat and AI surfaces,
/// <see cref="CdsIconButton"/> for icon-only actions and a selectable tag for filters.
/// </summary>
public class CdsChatButton : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsChatButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsChatButtonKind), typeof(CdsChatButton), CdsChatButtonKind.Primary, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsChatButton), CdsSize.Lg,
        coerceValue: static (_, v) => CdsChatButtonTokens.Coerce((CdsSize)v), propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsChatButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsQuickAction"/>.</summary>
    public static readonly BindableProperty IsQuickActionProperty = BindableProperty.Create(
        nameof(IsQuickAction), typeof(bool), typeof(CdsChatButton), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(CdsChatButton), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsChatButton), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsChatButton), null, propertyChanged: OnCommandChanged);

    const double MaxWidth = 320;

    readonly Grid _root = new();
    readonly Border _box = new() { StrokeThickness = 1, Padding = 0 };
    readonly Grid _layout = new() { ColumnSpacing = 0 };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsIcon _icon = new() { IsVisible = false };
    readonly CdsHitTarget _hit;
    CdsFocusRing _ring;
    CdsFocusRingStyle _ringStyle = CdsFocusRingStyle.Button;
    double _ringRadius;

    /// <summary>Creates a large primary chat button.</summary>
    public CdsChatButton()
    {
        _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _layout.Add(_label, 0);
        _layout.Add(_icon, 1);
        _box.Content = _layout;
        _box.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_box, true);

        _hit = CreateHitTarget();
        _ring = new CdsFocusRing(_ringStyle, 24);
        _ringRadius = 24;
        _root.Add(_box);
        _root.Add(_ring);
        _root.Add(_hit);
        _root.MaximumWidthRequest = MaxWidth;
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Creates a chat button with a label.</summary>
    public CdsChatButton(string text, CdsChatButtonKind kind = CdsChatButtonKind.Primary) : this()
    {
        Text = text;
        Kind = kind;
    }

    /// <summary>Raised on activation (click, tap, Enter or Space), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>The label: a short phrase, verb first ("Send", "Show nodes not ready").</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Visual emphasis. Default <see cref="CdsChatButtonKind.Primary"/>; ignored by quick actions.</summary>
    public CdsChatButtonKind Kind
    {
        get => (CdsChatButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Height: sm 32, md 40, lg 48 (default); other sizes fall back to lg. Ignored by quick actions (always sm).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The 16px icon after the label.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>A suggested prompt: forces a small ghost button drawn as an outlined pill.</summary>
    public bool IsQuickAction
    {
        get => (bool)GetValue(IsQuickActionProperty);
        set => SetValue(IsQuickActionProperty, value);
    }

    /// <summary>Marks the chosen quick action (no border, <c>background-selected</c> fill). Offer at most one.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Executed on activation; <c>CanExecute</c> drives <c>IsEnabled</c>.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
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
        var (kind, _) = CdsChatButtonTokens.Effective(Kind, Size, IsQuickAction);
        var v = CdsChatButtonTokens.Resolve(kind, State, IsQuickAction, IsSelected);
        SetColor(_box, BackgroundColorProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, v.Border);
        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void UpdateLayout()
    {
        var (kind, size) = CdsChatButtonTokens.Effective(Kind, Size, IsQuickAction);
        var height = CdsChatButtonTokens.Height(size);
        var radius = CdsChatButtonTokens.Radius(size);
        var ghost = kind == CdsChatButtonKind.Ghost;
        const double lineHeight = 18;
        const double iconSize = 16;
        var hasIcon = Icon is not null;

        _box.StrokeShape = new RoundRectangle { CornerRadius = radius };
        _label.TypeStyle = CdsTypeStyle.BodyCompact01;
        _label.Text = Text;
        _icon.Icon = Icon;
        _icon.Size = iconSize;
        _icon.IsVisible = hasIcon;

        var padTop = Math.Min((height - lineHeight) / 2 - 1, 14);
        var iconTop = Math.Min((height - iconSize) / 2 - 1, 14) + 1;
        _root.MinimumHeightRequest = height;
        _label.VerticalOptions = LayoutOptions.Start;
        if (ghost)
        {
            _label.Margin = new Thickness(15, padTop, hasIcon ? 0 : CdsChatButtonTokens.LabelEndPadding(false, true), padTop);
            Grid.SetColumn(_icon, 1);
            Grid.SetColumnSpan(_icon, 1);
            _icon.HorizontalOptions = LayoutOptions.Start;
            _icon.Margin = new Thickness(8, iconTop - 1, 15, 0);
        }
        else
        {
            _label.Margin = new Thickness(15, padTop, CdsChatButtonTokens.LabelEndPadding(hasIcon, false), padTop);
            Grid.SetColumn(_icon, 0);
            Grid.SetColumnSpan(_icon, 2);
            _icon.HorizontalOptions = LayoutOptions.End;
            _icon.Margin = new Thickness(0, iconTop - 1, 15, 0);
        }
        _icon.VerticalOptions = LayoutOptions.Start;

        var ringStyle = CdsChatButtonTokens.FocusRing(kind, IsQuickAction);
        if ((_ringStyle != ringStyle || Math.Abs(_ringRadius - radius) > 0.1) && _root.Children.IndexOf(_ring) is var index and >= 0)
        {
            _root.Children.RemoveAt(index);
            _ring = new CdsFocusRing(ringStyle, radius);
            _root.Children.Insert(index, _ring);
            _ringStyle = ringStyle;
            _ringRadius = radius;
        }

        SemanticProperties.SetDescription(_hit, Text);
        _hit.Role = IsQuickAction ? CdsAccessibilityRole.ToggleButton : CdsAccessibilityRole.Button;
        _hit.IsChecked = IsQuickAction && IsSelected;
        RefreshVisuals();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsChatButton)b).UpdateLayout();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var button = (CdsChatButton)b;
        button.SetCommand(button.Command, button.CommandParameter);
    }
}
