using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// One row of a <see cref="CdsContainedList"/>: 14px <c>text-primary</c> text (<see cref="Text"/>, or any view as
/// <see cref="CdsContainer.Body"/>), an optional decorative <see cref="Icon"/> (use it on every item or none) and an
/// optional <see cref="Action"/> at the end (usually an icon-only ghost button; the item then reserves 5rem at the end).
/// Set <see cref="IsClickable"/> (or a <see cref="Command"/>) to make the whole item a button: it hovers to
/// <c>layer-hover-01</c>, presses to <c>layer-active-01</c> and shows a 2px inset focus ring. <c>IsEnabled="False"</c>
/// dims it and removes it from the tab order. Its action is a separate tab stop after the item.
/// </summary>
public class CdsContainedListItem : CdsContainer
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsContainedListItem), null,
        propertyChanged: static (b, _, _) => ((CdsContainedListItem)b).Rebuild());

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsContainedListItem), null,
        propertyChanged: static (b, _, _) => ((CdsContainedListItem)b).Rebuild());

    /// <summary>Bindable property for <see cref="Action"/>.</summary>
    public static readonly BindableProperty ActionProperty = BindableProperty.Create(
        nameof(Action), typeof(View), typeof(CdsContainedListItem), null,
        propertyChanged: static (b, _, _) => ((CdsContainedListItem)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsClickable"/>.</summary>
    public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(
        nameof(IsClickable), typeof(bool), typeof(CdsContainedListItem), false,
        propertyChanged: static (b, _, _) => ((CdsContainedListItem)b).Rebuild());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsContainedListItem), null,
        propertyChanged: static (b, _, n) =>
        {
            var item = (CdsContainedListItem)b;
            item.SetCommand((ICommand?)n, item.CommandParameter);
            item.Rebuild();
        });

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsContainedListItem), null,
        propertyChanged: static (b, _, n) => ((CdsContainedListItem)b).SetCommand(((CdsContainedListItem)b).Command, n));

    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly Grid _content = new() { ColumnSpacing = CdsSpacing.Spacing04 };
    readonly CdsText _text = new() { TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.WordWrap };
    readonly ContentView _bodySlot = new();
    readonly CdsIcon _icon = new() { VerticalOptions = LayoutOptions.Start, InputTransparent = true };
    readonly BoxView _divider = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
    readonly ContentView _actionSlot = new() { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    CdsSize _size = CdsSize.Lg;
    bool _inset;
    bool _isLast = true;

    /// <summary>Creates an empty item.</summary>
    public CdsContainedListItem()
    {
        HorizontalOptions = LayoutOptions.Fill;
        _hit = CreateHitTarget();
        AutomationProperties.SetIsInAccessibleTree(_divider, false);
        Rebuild();
    }

    /// <summary>Raised when a clickable item is activated, before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>The item text (14px, <c>text-primary</c>): concise, similar in structure to the other items.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A decorative 16px icon before the text. Use it on every item or none.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The action at the end: usually an icon-only ghost button whose description names the item ("Revoke ci-deploy").</summary>
    public View? Action
    {
        get => (View?)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    /// <summary>Makes the whole item a button (<c>onClick</c>). Also on when a <see cref="Command"/> is set.</summary>
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    /// <summary>Runs when a clickable item is activated.</summary>
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

    bool Clickable => IsClickable || Command is not null;

    internal void Configure(CdsSize size, bool inset, bool isLast)
    {
        _size = size;
        _inset = inset;
        _isLast = isLast;
        Rebuild();
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        _bodySlot.Content = newBody;
        Rebuild();
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (!Clickable) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_fill, BoxView.ColorProperty, CdsContainedListTokens.ItemFill(State, Clickable));
        SetColor(_text, CdsText.TextColorProperty, CdsContainedListTokens.ItemText(disabled));
        SetColor(_icon, CdsIcon.ColorProperty, CdsContainedListTokens.ItemIcon(disabled));
        SetColor(_divider, BoxView.ColorProperty, CdsContainedListTokens.Divider);
        _ring.IsActive = Clickable && Is(CdsInteractionState.FocusVisible);
    }

    void Rebuild()
    {
        var clickable = Clickable;
        var hasAction = Action is not null;
        var hasIcon = Icon is not null;
        var height = CdsContainedListMetrics.Height(_size);
        var pad = CdsContainedListMetrics.ItemPadding(_size);

        _root.Children.Clear();
        _content.Children.Clear();
        _content.ColumnDefinitions.Clear();
        _actionSlot.Content = null;

        View body = Body is not null ? _bodySlot : _text;
        _text.Text = Text;
        if (hasIcon)
        {
            _icon.Icon = Icon;
            _content.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CdsSizing.IconSize01)));
            _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _icon.Margin = new Thickness(0, 2, 0, 0);
            _content.Add(_icon, 0);
            _content.Add(body, 1);
        }
        else
        {
            _content.Add(body);
        }
        _content.Padding = new Thickness(CdsContainedListMetrics.SidePadding, pad, hasAction ? CdsContainedListMetrics.ActionReserve : CdsContainedListMetrics.SidePadding, pad);
        _content.MinimumHeightRequest = height;
        _content.InputTransparent = clickable;

        _root.Add(_fill);
        _root.Add(_content);
        _divider.IsVisible = !_isLast;
        _divider.Margin = _inset ? new Thickness(CdsContainedListMetrics.SidePadding, 0) : new Thickness(0);
        _root.Add(_divider);
        if (clickable)
        {
            _root.Add(_ring);
            _root.Add(_hit);
            SemanticProperties.SetDescription(_hit, Text);
            _hit.Role = CdsAccessibilityRole.Button;
        }
        if (hasAction)
        {
            _actionSlot.Content = Action;
            _actionSlot.MaximumHeightRequest = CdsContainedListMetrics.ActionHeight(_size);
            _root.Add(_actionSlot);
        }

        // Plain items are read as text; clickable ones are announced through their hit target.
        AutomationProperties.SetExcludedWithChildren(_content, clickable);
        SetChrome(_root);
        RefreshVisuals();
    }
}
