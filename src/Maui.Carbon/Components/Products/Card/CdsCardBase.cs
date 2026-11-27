using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>The options only one of the two card kinds exposes, read by the shared card when it lays itself out.</summary>
internal readonly record struct CardOptions(
    CdsCardPlacement ActionsPlacement,
    CdsCardPlacement PrimaryPlacement,
    CdsCardPlacement SecondaryPlacement,
    CdsCardClickZone ClickZone,
    CdsCardTitleSize TitleSize,
    CdsCardPrimaryButtonKind PrimaryKind,
    CdsCardSecondaryButtonKind SecondaryKind,
    View? Media,
    CdsCardMediaPosition MediaPosition,
    CdsIconName? Pictogram,
    IList<CdsMenuItemBase>? OverflowActions,
    string? OverflowDescription);

/// <summary>
/// The shared card of Carbon for IBM Products (<c>Card</c>), behind <see cref="CdsProductiveCard"/> and
/// <see cref="CdsExpressiveCard"/>: a <c>layer-01</c> surface with an optional label, title and description, the
/// consumer's <see cref="CdsContainer.Body"/>, icon actions, a primary and a secondary button and an optional
/// decorator (an <see cref="CdsAILabel"/> gives the card the AI treatment). Handling <see cref="Clicked"/> or setting
/// <see cref="Command"/> makes the card clickable. Apps use the two concrete cards; this base cannot be derived from
/// outside the library.
/// </summary>
public abstract partial class CdsCardBase : CdsContainer
{
    readonly bool _productive;
    readonly Border _surface = new() { StrokeShape = new Rectangle(), StrokeThickness = 1, Padding = 0 };
    readonly Grid _stack = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly BoxView _zoneFill = new() { InputTransparent = true, IsVisible = false, Color = Colors.Transparent };
    readonly ContentView _mediaHost = new() { IsVisible = false };
    readonly Grid _header = new() { ColumnSpacing = 0, InputTransparent = true, CascadeInputTransparent = false };
    readonly VerticalStackLayout _titles = new() { Spacing = 0, InputTransparent = true };
    readonly HorizontalStackLayout _headerActions = new() { Spacing = 0, IsVisible = false };
    readonly ContentView _bodyHost = new();
    readonly Grid _footer = new() { ColumnSpacing = 0, RowSpacing = 0, IsVisible = false, InputTransparent = true, CascadeInputTransparent = false };
    readonly ContentView _decoratorHost = new() { IsVisible = false, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
    readonly CdsItemCollection<CdsCardAction> _actionIcons = [];
    readonly CdsCoalescedAction _rebuild;
    CdsText? _label;
    CdsText? _title;
    CdsText? _description;
    CdsIcon? _pictogram;
    Grid? _hitBox;
    CdsHitTarget? _hit;
    CdsFocusRing? _ring;
    CdsOverflowMenu? _overflow;
    EventHandler? _clicked;
    CardLayout _layout;
    bool _hitHover;
    bool _ai;

    /// <summary>Builds the card surface; <paramref name="productive"/> picks the productive rules.</summary>
    private protected CdsCardBase(bool productive)
    {
        _productive = productive;
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        _stack.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // media (top)
        _stack.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // pictogram
        _stack.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // header
        _stack.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // body: takes extra height so the bar stays at the bottom
        _stack.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // bottom bar
        _stack.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0))); // media (left)
        _stack.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _header.Add(_titles, 0);
        _header.Add(_headerActions, 1);

        _stack.Add(_zoneFill);
        _stack.Add(_mediaHost);
        _stack.Add(_bodyHost, 1, 3);
        _stack.Add(_header, 1, 2);
        _stack.Add(_footer, 1, 4);
        _stack.Add(_decoratorHost, 1, 1);
        Grid.SetRowSpan(_decoratorHost, 2);
        _decoratorHost.Margin = new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0);

        _actionIcons.CollectionChanged += OnActionIconsChanged;
        _surface.Content = _stack;
        SetChrome(_surface);
        Loaded += (_, _) => RequestRebuild(); // body text bound after construction names the click target
    }

    /// <summary>The icon actions, in order (Carbon's <c>actionIcons</c>). Productive cards replace them with an overflow menu when it has items.</summary>
    public IList<CdsCardAction> ActionIcons => _actionIcons;

    /// <summary>
    /// Raised when the clickable card (or its click zone) is activated (click, tap, Enter or Space). Handling it makes the
    /// card clickable, as Carbon's <c>onClick</c> does.
    /// </summary>
    public event EventHandler? Clicked
    {
        add
        {
            _clicked += value;
            RequestRebuild();
        }
        remove
        {
            _clicked -= value;
            RequestRebuild();
        }
    }

    /// <summary>Raised when the primary button is activated, before <see cref="PrimaryButtonCommand"/> runs.</summary>
    public event EventHandler? PrimaryButtonClicked;

    /// <summary>Raised when the secondary button is activated, before <see cref="SecondaryButtonCommand"/> runs.</summary>
    public event EventHandler? SecondaryButtonClicked;

    /// <summary>Whether the card (or one of its zones) is clickable: <see cref="Clicked"/> is handled or <see cref="Command"/> is set.</summary>
    public bool IsClickable => _clicked is not null || Command is not null;

    /// <summary>The options of the concrete card.</summary>
    private protected abstract CardOptions Options { get; }

    /// <summary>The layout in effect (for tests).</summary>
    internal CardLayout CurrentLayout => _layout;

    /// <summary>The click target, when the card is clickable (for tests).</summary>
    internal CdsHitTarget? HitTarget => _hitBox is { IsVisible: true } ? _hit : null;

    /// <summary>Asks for a rebuild after the current batch of changes.</summary>
    private protected void RequestRebuild() => _rebuild.Request();

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        _bodyHost.Content = newBody;
        RequestRebuild();
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        if (_layout.ClickZone is CdsCardClickZone.Two or CdsCardClickZone.Three)
            state = _hitHover ? state | CdsInteractionState.Hover : state & ~CdsInteractionState.Hover;
        var v = CdsCardTokens.Resolve(_layout.ClickZone, state);
        if (!_ai)
        {
            SetColor(_surface, BackgroundColorProperty, v.Background);
            SetBrush(_surface, Border.StrokeProperty, v.Border);
        }
        SetColor(_zoneFill, BoxView.ColorProperty, v.ZoneBackground);
        if (_label is not null) SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, CdsColorKeys.TextPrimary);
        if (_title is not null) SetColor(_title, Microsoft.Maui.Controls.Label.TextColorProperty, CdsColorKeys.TextPrimary);
        if (_description is not null) SetColor(_description, Microsoft.Maui.Controls.Label.TextColorProperty, CdsColorKeys.TextPrimary);
        if (_pictogram is not null) SetColor(_pictogram, CdsIcon.ColorProperty, CdsColorKeys.IconPrimary);
        if (_ring is not null) _ring.IsActive = _layout.ClickZone is not null && Is(CdsInteractionState.FocusVisible);
    }

    void OnActionIconsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsCardAction action in e.OldItems) action.PropertyChanged -= OnPartChanged;
        if (e.NewItems is not null)
            foreach (CdsCardAction action in e.NewItems) action.PropertyChanged += OnPartChanged;
        RequestRebuild();
    }

    void OnPartChanged(object? sender, PropertyChangedEventArgs e) => RequestRebuild();

    /// <summary>Lays the card out again from its properties.</summary>
    private protected void Rebuild()
    {
        var o = Options;
        var inputs = new CardInputs(
            _productive, _actionIcons.Count, o.OverflowActions?.Count ?? 0,
            !string.IsNullOrEmpty(PrimaryButtonText), !string.IsNullOrEmpty(SecondaryButtonText),
            o.ActionsPlacement, o.PrimaryPlacement, o.SecondaryPlacement, IsClickable, o.ClickZone,
            !string.IsNullOrEmpty(Label), !string.IsNullOrEmpty(Title), !string.IsNullOrEmpty(Description),
            o.TitleSize, Decorator is not null);
        _layout = CardModel.Layout(inputs);

        BuildMedia(o);
        BuildPictogram(o);
        BuildTitles(o);
        BuildHeaderActions(o);
        BuildFooter(o);
        BuildDecorator();
        BuildClickZone();
        RefreshVisuals();
    }

    void BuildMedia(CardOptions o)
    {
        var media = _productive ? null : o.Media;
        _mediaHost.Content = media;
        _mediaHost.IsVisible = media is not null;
        var left = media is not null && o.MediaPosition == CdsCardMediaPosition.Left;
        _stack.ColumnDefinitions[0].Width = left ? GridLength.Auto : new GridLength(0);
        Grid.SetRow(_mediaHost, 0);
        Grid.SetColumn(_mediaHost, 0);
        Grid.SetRowSpan(_mediaHost, left ? 5 : 1);
        Grid.SetColumnSpan(_mediaHost, left ? 1 : 2);
    }

    void BuildPictogram(CardOptions o)
    {
        var icon = _productive ? null : o.Pictogram;
        if (icon is null)
        {
            if (_pictogram is not null) _pictogram.IsVisible = false;
            return;
        }
        if (_pictogram is null)
        {
            _pictogram = new CdsIcon { Size = 32, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
            _pictogram.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0, 0);
            _stack.Insert(_stack.Children.IndexOf(_bodyHost), _pictogram);
            Grid.SetRow(_pictogram, 1);
            Grid.SetColumn(_pictogram, 1);
        }
        _pictogram.Icon = icon;
        _pictogram.IsVisible = true;
    }

    void BuildTitles(CardOptions o)
    {
        var label = Label;
        var title = Title;
        var description = Description;
        if (!string.IsNullOrEmpty(label) && _label is null)
        {
            _label = new CdsText { TypeStyle = CdsTypeStyle.Label01 };
            _titles.Insert(0, _label);
        }
        if (!string.IsNullOrEmpty(title) && _title is null)
        {
            _title = new CdsText();
            SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level3);
            _titles.Insert(_label is null ? 0 : 1, _title);
        }
        if (!string.IsNullOrEmpty(description) && _description is null)
        {
            _description = new CdsText { TypeStyle = CdsTypeStyle.Label01, Margin = new Thickness(0, CdsSpacing.Spacing01, 0, 0) };
            _titles.Add(_description);
        }
        if (_label is not null)
        {
            _label.Text = label;
            _label.IsVisible = !string.IsNullOrEmpty(label);
            _label.Margin = new Thickness(0, 0, 0, _layout.LabelOnly ? 0 : CdsSpacing.Spacing01);
        }
        if (_title is not null)
        {
            _title.Text = title;
            _title.IsVisible = !string.IsNullOrEmpty(title);
            _title.TypeStyle = CdsCardTokens.TitleStyle(_productive, o.TitleSize);
        }
        if (_description is not null)
        {
            _description.Text = description;
            _description.IsVisible = !string.IsNullOrEmpty(description);
        }
        _titles.IsVisible = _titles.Children.OfType<View>().Any(v => v.IsVisible);
        _header.Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, _layout.HeaderEndPadding, _layout.HeaderBottomPadding);
        // Productive headers align their actions with the top of the title; expressive ones center them.
        _headerActions.VerticalOptions = _productive ? LayoutOptions.Start : LayoutOptions.Center;
        _headerActions.Margin = new Thickness(0, _layout.HeaderActionsTopMargin, 0, 0);
        _bodyHost.Padding = new Thickness(CdsSpacing.Spacing05, _productive ? CdsSpacing.Spacing03 : 0, CdsSpacing.Spacing05, CdsSpacing.Spacing05);
    }

    void BuildHeaderActions(CardOptions o)
    {
        DetachOverflow();
        _headerActions.Clear();
        _headerActions.IsVisible = _layout.HasHeaderActions;
        if (!_layout.HasHeaderActions) return;
        var parts = new List<View>();
        AddActions(parts, CdsCardPlacement.Top, o);
        var ghostStart = parts.Count;
        if (_layout.SecondaryInHeader) parts.Add(CreateButton(primary: false, CdsButtonKind.Ghost, CdsSize.Sm));
        if (_layout.PrimaryInHeader) parts.Add(CreateButton(primary: true, CdsButtonKind.Ghost, CdsSize.Sm));
        for (var i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            if (i >= ghostStart && CardModel.ShowsSeparator(i, parts.Count))
            {
                var cell = new Grid { InputTransparent = true, CascadeInputTransparent = false };
                cell.Add(part);
                var separator = new BoxView { WidthRequest = 1, HeightRequest = CdsSpacing.Spacing06, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
                CdsTokens.SetColor(separator, BoxView.ColorProperty, CdsCardTokens.HeaderSeparator);
                cell.Add(separator);
                part = cell;
            }
            _headerActions.Add(part);
        }
    }

    void BuildFooter(CardOptions o)
    {
        _footer.Clear();
        _footer.ColumnDefinitions.Clear();
        _footer.RowDefinitions.Clear();
        _footer.IsVisible = _layout.HasBottomBar;
        if (!_layout.HasBottomBar) return;

        var parts = new List<View>();
        if (_layout.SecondaryInFooter)
            parts.Add(CreateButton(primary: false, CardModel.FooterKind(_productive, false, o.PrimaryKind, o.SecondaryKind), CdsSize.Md));
        if (_layout.PrimaryInFooter)
            parts.Add(CreateButton(primary: true, CardModel.FooterKind(_productive, true, o.PrimaryKind, o.SecondaryKind), CdsSize.Md));
        if (_layout.HasFooterActions)
        {
            var actions = new List<View>();
            AddActions(actions, CdsCardPlacement.Bottom, o);
            if (actions.Count > 0)
            {
                var row = new HorizontalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, MinimumHeightRequest = _productive ? CdsSpacing.Spacing07 : 0 };
                foreach (var action in actions) row.Add(action);
                parts.Add(row);
            }
        }

        if (_productive)
        {
            // A 1px border-subtle-01 rule above the bar; parts are centered and spread (space-between), or end-aligned
            // when the bar has no button.
            _footer.RowDefinitions.Add(new RowDefinition(new GridLength(1)));
            _footer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var columns = CardModel.FooterColumns(parts.Count, _layout.HasFooterButton);
            foreach (var width in columns) _footer.ColumnDefinitions.Add(new ColumnDefinition(width));
            var rule = new BoxView { HeightRequest = 1, InputTransparent = true };
            CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsCardTokens.FooterRule);
            _footer.Add(rule, 0, 0);
            if (columns.Count > 1) Grid.SetColumnSpan(rule, columns.Count);
            for (var i = 0; i < parts.Count; i++)
            {
                parts[i].VerticalOptions = LayoutOptions.Center;
                _footer.Add(parts[i], CardModel.FooterColumn(i, parts.Count, _layout.HasFooterButton), 1);
            }
        }
        else
        {
            // Expressive: the buttons and icons sit flush at the end edge (justify-content: flex-end).
            var row = new HorizontalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.End };
            foreach (var part in parts) row.Add(part);
            _footer.Add(row);
        }
        _footer.VerticalOptions = LayoutOptions.End;
    }

    void AddActions(List<View> parts, CdsCardPlacement placement, CardOptions o)
    {
        if (_layout.UsesOverflow && o.OverflowActions is { } overflowItems)
        {
            _overflow ??= new CdsOverflowMenu();
            _overflow.Size = CardModel.ActionSize(_productive, placement);
            _overflow.Direction = placement == CdsCardPlacement.Top ? CdsOverflowMenuDirection.Bottom : CdsOverflowMenuDirection.Top;
            _overflow.IsFlipped = true;
            _overflow.IconDescription = string.IsNullOrEmpty(o.OverflowDescription) ? "Options" : o.OverflowDescription;
            var items = _overflow.Items;
            if (!items.SequenceEqual(overflowItems))
            {
                items.Clear();
                foreach (var item in overflowItems) items.Add(item);
            }
            parts.Add(_overflow);
            return;
        }
        var size = CardModel.ActionSize(_productive, placement);
        foreach (var action in _actionIcons)
        {
            var current = action;
            if (_productive)
            {
                var button = new CdsButton
                {
                    Kind = CdsButtonKind.Ghost,
                    IsIconOnly = true,
                    Icon = action.Icon,
                    IconDescription = action.IconDescription,
                    Size = size,
                    Href = action.Href,
                };
                button.Clicked += (_, _) => current.Activate(openHref: false);
                parts.Add(button);
            }
            else
            {
                var button = new CdsIconButton { Kind = CdsIconButtonKind.Ghost, Size = size, Icon = action.Icon, Label = action.IconDescription };
                button.Clicked += (_, _) => current.Activate(openHref: true);
                parts.Add(button);
            }
        }
    }

    CdsButton CreateButton(bool primary, CdsButtonKind kind, CdsSize size)
    {
        var button = new CdsButton
        {
            Kind = kind,
            Size = size,
            Text = primary ? PrimaryButtonText : SecondaryButtonText,
            Icon = primary ? PrimaryButtonIcon : SecondaryButtonIcon,
            Href = primary ? PrimaryButtonHref : SecondaryButtonHref,
            IsEnabled = primary ? IsPrimaryButtonEnabled : IsSecondaryButtonEnabled,
        };
        if (primary)
        {
            button.Command = PrimaryButtonCommand;
            button.Clicked += (_, _) => PrimaryButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            button.Command = SecondaryButtonCommand;
            button.Clicked += (_, _) => SecondaryButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        return button;
    }

    void DetachOverflow()
    {
        if (_overflow?.Parent is Layout parent) parent.Remove(_overflow);
    }

    void BuildDecorator()
    {
        var decorator = Decorator;
        if (!ReferenceEquals(_decoratorHost.Content, decorator)) _decoratorHost.Content = decorator;
        _decoratorHost.IsVisible = decorator is not null;
        // On a clickable card the decorator is a picture, not a second target (pointer-events: none).
        _decoratorHost.InputTransparent = _layout.ClickZone is not null;
        var ai = decorator is CdsAILabel;
        if (ai == _ai) return;
        _ai = ai;
        if (ai)
        {
            CdsAiTreatment.Apply(_surface);
        }
        else
        {
            CdsAiTreatment.Remove(_surface);
            ForgetColors(new HashSet<BindableObject> { _surface });
        }
    }

    void BuildClickZone()
    {
        var zone = _layout.ClickZone;
        if (zone is null)
        {
            if (_hitBox is not null) _hitBox.IsVisible = false;
            _zoneFill.IsVisible = false;
            SealBody(false);
            AutomationProperties.SetExcludedWithChildren(_titles, false);
            return;
        }
        if (_hitBox is null || _hit is null)
        {
            // The ring and the native target share a box that tracks the pointer over the zone (zones two and three hover
            // only their zone, not the whole card).
            _ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
            _hit = CreateHitTarget(CdsAccessibilityRole.Button);
            _hitBox = new Grid();
            _hitBox.Add(_ring);
            _hitBox.Add(_hit);
            var pointer = new PointerGestureRecognizer();
            pointer.PointerEntered += (_, _) =>
            {
                _hitHover = true;
                RefreshVisuals();
            };
            pointer.PointerExited += (_, _) =>
            {
                _hitHover = false;
                RefreshVisuals();
            };
            _hitBox.GestureRecognizers.Add(pointer);
            // Above the content it covers, below the header actions, decorator and bottom bar.
            _stack.Insert(_stack.Children.IndexOf(_header), _hitBox);
        }
        var (row, rowSpan, column, columnSpan) = zone switch
        {
            CdsCardClickZone.One => (0, 5, 0, 2),
            CdsCardClickZone.Two => (2, 2, 1, 1),
            _ => (3, 1, 1, 1),
        };
        foreach (var view in new View[] { _hitBox, _zoneFill })
        {
            Grid.SetRow(view, row);
            Grid.SetRowSpan(view, rowSpan);
            Grid.SetColumn(view, column);
            Grid.SetColumnSpan(view, columnSpan);
        }
        _hitBox.IsVisible = true;
        _zoneFill.IsVisible = zone != CdsCardClickZone.One;
        SealBody(true);
        // The header text is part of the target's name in zones one and two.
        AutomationProperties.SetExcludedWithChildren(_titles, zone != CdsCardClickZone.Three);
        SemanticProperties.SetDescription(_hit, CardModel.AccessibleName(zone, Label, Title, CdsTileText.Collect(Body)));
        SemanticProperties.SetHint(_hit, zone == CdsCardClickZone.Three ? null : Description);
    }

    void SealBody(bool sealedBody)
    {
        _bodyHost.InputTransparent = sealedBody;
        AutomationProperties.SetExcludedWithChildren(_bodyHost, sealedBody);
    }
}
