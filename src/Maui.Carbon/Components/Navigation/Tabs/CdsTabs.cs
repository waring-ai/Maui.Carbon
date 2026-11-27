using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Switches between a few related views of the same context without leaving the page. Add <see cref="CdsTab"/>s to
/// <see cref="Items"/>; each tab's <see cref="CdsTab.Body"/> is shown below the row while it is selected.
/// Line tabs (40) sit on a 2px rule with a 2px <c>border-interactive</c> underline under the selected tab; contained
/// tabs (48) are <c>layer-accent-01</c> blocks, the selected one in <c>layer-01</c> with a 2px top edge. Use at least
/// two and about six tabs; use a <see cref="CdsContentSwitcher"/> to switch the format of the same content and a progress
/// indicator for steps. Keyboard: the row is a single Tab stop (the selected tab; a roving tab index), so Tab moves from the row
/// into the panel; Left and Right move between enabled tabs (wrapping), Home and End jump to the first and last; arrows select in
/// <see cref="CdsActivation.Automatic"/> mode, Enter or Space select in <see cref="CdsActivation.Manual"/> mode. When the tabs
/// do not fit the row scrolls, with scroll buttons at the edges.
/// </summary>
[ContentProperty(nameof(Items))]
public partial class CdsTabs : CdsControl
{
    const double ScrollButtonWidth = CdsSpacing.Spacing07;

    readonly CdsItemCollection<CdsTab> _items = [];
    readonly List<CdsTabItemView> _views = [];
    readonly Grid _row = new() { HorizontalOptions = LayoutOptions.Start };
    readonly ScrollView _scroll;
    readonly ContentView _panel = new() { Padding = new Thickness(CdsSpacing.Spacing05) };
    readonly CdsTabScrollButton _left;
    readonly CdsTabScrollButton _right;
    readonly CdsRovingFocus _roving = new(CdsRovingOrientation.Horizontal, wraps: true) { RemembersFocus = false };
    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly Grid _strip = new();
    // Vertical tabs, built on first use: a layer-01 column with a 1px end rule, scrolling its stack of tabs.
    Grid? _column;
    ScrollView? _columnScroll;
    VerticalStackLayout? _list;
    int _applied = -2;
    bool _rebuilding;
    bool _fullWidthActive;

    /// <summary>Creates an empty line tabs control.</summary>
    public CdsTabs()
    {
        _scroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = _row,
        };
        _left = new CdsTabScrollButton(this, CdsIconName.ChevronLeft, -1) { HorizontalOptions = LayoutOptions.Start, IsVisible = false };
        _right = new CdsTabScrollButton(this, CdsIconName.ChevronRight, 1) { HorizontalOptions = LayoutOptions.End, IsVisible = false };

        _strip.Add(_scroll);
        _strip.Add(_left);
        _strip.Add(_right);

        HorizontalOptions = LayoutOptions.Fill;
        LayoutRoot();
        SetChrome(_root);

        _roving.CanFocus = i => i >= 0 && i < _items.Count && _items[i].IsEnabled;
        _roving.PreferredIndex = () => _applied;
        _roving.Navigated += OnRovingNavigated;
        _items.CollectionChanged += OnItemsChanged;
        _scroll.Scrolled += (_, _) => UpdateScrollButtons();
        _scroll.SizeChanged += OnStripSizeChanged;
        _row.SizeChanged += (_, _) => UpdateScrollButtons();
        Rebuild();
    }

    /// <summary>Raised when the selected tab changes (by the user or by <see cref="SelectedIndex"/>).</summary>
    public event EventHandler<CdsTabsSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised when the close icon of a tab is used (<see cref="IsDismissible"/>). Remove the tab from <see cref="Items"/> to close it.</summary>
    public event EventHandler<CdsTabCloseRequestedEventArgs>? TabCloseRequested;

    /// <summary>The tabs, in order.</summary>
    public IList<CdsTab> Items => _items;

    /// <summary>The selected tab, or <c>null</c> when there are no enabled tabs.</summary>
    public CdsTab? SelectedTab => _applied >= 0 && _applied < _items.Count ? _items[_applied] : null;

    internal bool HasSecondaryLabels => _items.Any(t => !string.IsNullOrEmpty(t.SecondaryLabel));

    internal bool IsFullWidthActive => _fullWidthActive;

    internal bool IsVertical => Orientation == CdsTabsOrientation.Vertical;

    /// <summary>The stack of vertical tabs, or <c>null</c> before the tabs were first vertical (for tests).</summary>
    internal VerticalStackLayout? VerticalList => _list;

    /// <summary>The tab views in order (for tests).</summary>
    internal IReadOnlyList<CdsTabItemView> TabViews => _views;

    /// <summary>The roving focus orientation in effect (for tests).</summary>
    internal CdsRovingOrientation KeyOrientation => _roving.Orientation;

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e) => _views.Count > 0 && _roving.HandleKey(e);

    /// <summary>The index of the tab that Tab lands on (the selected tab, or the one arrowed to while focus is in the row).</summary>
    internal int TabStopIndex => _roving.TabStopIndex;

    internal void OnTabActivated(int index)
    {
        if (index < 0 || index >= _items.Count || !_items[index].IsEnabled) return;
        Select(index);
    }

    internal void RequestClose(int index)
    {
        if (index < 0 || index >= _items.Count || !_items[index].IsEnabled) return;
        TabCloseRequested?.Invoke(this, new CdsTabCloseRequestedEventArgs(index, _items[index]));
    }

    internal void ScrollByViewport(int direction)
    {
        var step = Math.Max(CdsTabsTokens.MinWidth, _scroll.Width * 0.5);
        var x = Math.Clamp(_scroll.ScrollX + direction * step, 0, Math.Max(0, _row.Width - _scroll.Width));
        _ = _scroll.ScrollToAsync(x, 0, !CdsMotion.IsReducedMotion);
    }

    IReadOnlyList<bool> Enabled() => _items.Select(t => t.IsEnabled).ToArray();

    void Select(int index)
    {
        if (SelectedIndex != index) SelectedIndex = index;
        else ApplySelection();
    }

    void OnRovingNavigated(object? sender, int index)
    {
        RevealTab(index);
        if (Activation == CdsActivation.Automatic) Select(index);
    }

    void RevealTab(int index)
    {
        if (index < 0 || index >= _views.Count) return;
        var view = _views[index];
        if (IsVertical)
        {
            if (_columnScroll is { } scroll)
            {
                var y = TabsModel.ScrollToReveal(view.Y, view.Y + view.Height, scroll.ScrollY, scroll.Height);
                if (Math.Abs(y - scroll.ScrollY) > 0.5) _ = scroll.ScrollToAsync(0, y, false);
            }
            return;
        }
        var x = TabsModel.ScrollToReveal(view.X, view.X + view.Width, _scroll.ScrollX, _scroll.Width);
        if (Math.Abs(x - _scroll.ScrollX) > 0.5) _ = _scroll.ScrollToAsync(x, 0, false);
    }

    void ApplySelection()
    {
        if (_rebuilding) return;
        var coerced = TabsModel.Coerce(Enabled(), SelectedIndex);
        if (coerced >= 0 && coerced != SelectedIndex)
        {
            SelectedIndex = coerced;
            return;
        }
        var previous = _applied;
        _applied = coerced;
        for (var i = 0; i < _views.Count; i++) _views[i].Update(_items[i], i == _applied);
        _roving.Refresh();
        _panel.Content = SelectedTab?.Body;
        if (previous != -2 && previous != _applied)
            SelectionChanged?.Invoke(this, new CdsTabsSelectionChangedEventArgs(previous, _applied));
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsTab tab in e.OldItems)
            {
                tab.PropertyChanged -= OnTabPropertyChanged;
                tab.Parent = null;
            }
        if (e.NewItems is not null)
            foreach (CdsTab tab in e.NewItems)
            {
                tab.Parent = this;
                tab.PropertyChanged += OnTabPropertyChanged;
            }

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex >= 0 && _applied >= 0)
        {
            var next = TabsModel.AfterRemoval(Enabled(), SelectedIndex, e.OldStartingIndex);
            _rebuilding = true;
            try
            {
                if (next >= 0) SelectedIndex = next;
            }
            finally
            {
                _rebuilding = false;
            }
            var wasSelected = e.OldStartingIndex == _applied;
            var old = _applied;
            Rebuild();
            if (wasSelected || old != _applied) SelectionChanged?.Invoke(this, new CdsTabsSelectionChangedEventArgs(old, _applied));
            return;
        }
        Rebuild();
    }

    void OnTabPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == CdsTab.BodyProperty.PropertyName && sender is CdsTab tab && tab == SelectedTab)
            _panel.Content = tab.Body;
        else
            Rebuild();
    }

    void ApplyOrientation()
    {
        LayoutRoot();
        Rebuild();
    }

    // Horizontal: the tab strip above the panel. Vertical: the tab column (a quarter of the width) beside the panel.
    void LayoutRoot()
    {
        var vertical = IsVertical;
        _root.Children.Clear();
        _root.RowDefinitions.Clear();
        _root.ColumnDefinitions.Clear();
        _roving.Orientation = vertical ? CdsRovingOrientation.Vertical : CdsRovingOrientation.Horizontal;
        if (!vertical)
        {
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _root.Add(_strip, 0, 0);
            _root.Add(_panel, 0, 1);
            return;
        }
        var column = EnsureColumn();
        var fraction = CdsTabsTokens.VerticalListFraction;
        _root.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(fraction, GridUnitType.Star)));
        _root.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1 - fraction, GridUnitType.Star)));
        _root.Add(column, 0, 0);
        _root.Add(_panel, 1, 0);
    }

    Grid EnsureColumn()
    {
        if (_column is not null) return _column;
        _list = new VerticalStackLayout { Spacing = 0 };
        _columnScroll = new ScrollView
        {
            Orientation = ScrollOrientation.Vertical,
            VerticalScrollBarVisibility = ScrollBarVisibility.Default,
            Content = _list,
        };
        // Carbon: the column is layer-01 with an inset 1px border-subtle rule at its end, under the tabs.
        var rule = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _column = new Grid();
        CdsTokens.SetColor(_column, BackgroundColorProperty, CdsColorKeys.Layer01);
        _column.Add(rule);
        _column.Add(_columnScroll);
        return _column;
    }

    void OnStripSizeChanged(object? sender, EventArgs e)
    {
        var full = IsFullWidth && Type == CdsTabsType.Contained && !IsIconOnly && _scroll.Width >= CdsBreakpoints.Md;
        if (full != _fullWidthActive) Rebuild();
        UpdateScrollButtons();
    }

    void Rebuild()
    {
        if (_scroll is null) return;
        _rebuilding = true;
        try
        {
            var vertical = IsVertical;
            _fullWidthActive = !vertical && IsFullWidth && Type == CdsTabsType.Contained && !IsIconOnly && _scroll.Width >= CdsBreakpoints.Md;
            _row.Children.Clear();
            _row.ColumnDefinitions.Clear();
            _list?.Children.Clear();
            _views.Clear();
            _row.HorizontalOptions = _fullWidthActive ? LayoutOptions.Fill : LayoutOptions.Start;
            for (var i = 0; i < _items.Count; i++)
            {
                var view = new CdsTabItemView(this, i);
                _views.Add(view);
                if (vertical)
                {
                    _list!.Add(view);
                    continue;
                }
                _row.ColumnDefinitions.Add(new ColumnDefinition(_fullWidthActive ? GridLength.Star : GridLength.Auto));
                _row.Add(view, i);
            }
            CdsSemantics.SetGroup(vertical ? _list! : _row, Label, CdsSemanticGroupKind.List);
            _left.Refresh();
            _right.Refresh();
        }
        finally
        {
            _rebuilding = false;
        }
        _applied = -2;
        var coerced = TabsModel.Coerce(Enabled(), SelectedIndex);
        if (coerced >= 0 && coerced != SelectedIndex)
        {
            _rebuilding = true;
            try
            {
                SelectedIndex = coerced;
            }
            finally
            {
                _rebuilding = false;
            }
        }
        _applied = coerced;
        for (var i = 0; i < _views.Count; i++) _views[i].Update(_items[i], i == _applied);
        _roving.SetItems(_views.Select(v => v.Hit));
        _panel.Content = SelectedTab?.Body;
        UpdateScrollButtons();
    }

    void UpdateScrollButtons()
    {
        var overflow = !IsVertical && _row.Width > _scroll.Width + 0.5 && _scroll.Width > 0;
        _left.IsVisible = overflow && _scroll.ScrollX > 0.5;
        _right.IsVisible = overflow && _scroll.ScrollX + _scroll.Width < _row.Width - 0.5;
        _left.Refresh();
        _right.Refresh();
    }

    /// <summary>A 32 wide chevron button at the end of a tab row that scrolls the row.</summary>
    sealed class CdsTabScrollButton : CdsControl
    {
        readonly CdsTabs _owner;
        readonly int _direction;
        readonly Grid _root = new() { WidthRequest = ScrollButtonWidth };
        readonly BoxView _fill = new() { InputTransparent = true };
        readonly CdsIcon _icon;
        readonly CdsHitTarget _hit;

        public CdsTabScrollButton(CdsTabs owner, CdsIconName icon, int direction)
        {
            _owner = owner;
            _direction = direction;
            _icon = new CdsIcon(icon) { InputTransparent = true };
            AutomationProperties.SetExcludedWithChildren(_icon, true);
            // Pointer only, as in Carbon (tabindex -1): the arrow keys already scroll the focused tab into view.
            _hit = CreateHitTarget();
            _hit.IsTabStop = false;
            SemanticProperties.SetDescription(_hit, direction < 0 ? "Scroll left" : "Scroll right");
            _root.Add(_fill);
            _root.Add(_icon);
            _root.Add(_hit);
            SetChrome(_root);
            VerticalOptions = LayoutOptions.Fill;
        }

        public void Refresh()
        {
            var contained = _owner.Type == CdsTabsType.Contained;
            SetColor(_fill, BoxView.ColorProperty, Is(CdsInteractionState.Hover)
                ? (contained ? CdsColorKeys.LayerAccentHover01 : CdsColorKeys.BackgroundHover)
                : (contained ? CdsColorKeys.LayerAccent01 : CdsColorKeys.Background));
            SetColor(_icon, CdsIcon.ColorProperty, CdsColorKeys.IconPrimary);
        }

        protected override void ApplyVisuals() => Refresh();

        protected override void OnActivated() => _owner.ScrollByViewport(_direction);
    }
}
