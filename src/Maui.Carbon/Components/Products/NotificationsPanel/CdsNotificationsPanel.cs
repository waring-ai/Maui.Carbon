using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Carbon;

/// <summary>
/// The history of system notifications, opened from a <see cref="CdsHeaderGlobalAction"/> in the UI shell header
/// (Carbon for IBM Products' <c>NotificationsPanel</c>). Declare it in the page (it draws nothing inline); while
/// <see cref="IsOpen"/> is true it hangs in the top end corner under the header, 616 high, grouped into Today, Yesterday
/// and Previous, newest first. Each notification can be activated, expanded and dismissed; the header can dismiss all
/// and hold a "Do not disturb" toggle; an optional footer links to the full list and settings. A tap outside or Escape
/// closes it. Pair the trigger with a <see cref="CdsBadgeIndicator"/> for unread counts. For a message that needs a
/// response now, use a toast or an inline notification instead.
/// </summary>
public partial class CdsNotificationsPanel : CdsControl
{
    /// <summary>The panel height (<c>38.5rem</c>).</summary>
    internal const double PanelHeight = 616;

    /// <summary>The panel width (<c>max-inline-size: 22.75rem</c>; at least 20rem).</summary>
    internal const double PanelWidth = 364;

    readonly ConditionalWeakTable<CdsNotificationsPanelItem, object> _expanded = [];
    readonly CdsCoalescedAction _rebuild;
    readonly List<CdsNotificationsPanelItem> _watched = [];
    CdsOverlay? _overlay;
    bool _hiding;
    INotifyCollectionChanged? _source;

    Grid? _surface;
    VerticalStackLayout? _list;
    CdsButton? _dismissAll;
    CdsToggle? _toggle;
    Grid? _footer;

    /// <summary>Creates a closed notifications panel.</summary>
    public CdsNotificationsPanel()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        WidthRequest = 0;
        HeightRequest = 0;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new ContentView());
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        Loaded += (_, _) => { if (IsOpen && _overlay is null) Show(); };
        Unloaded += (_, _) => Hide();
    }

    /// <summary>Raised when a notification is activated (click, tap, Enter), before <see cref="NotificationCommand"/>.</summary>
    public event EventHandler<CdsNotificationsPanelItem>? NotificationClicked;

    /// <summary>Raised when a notification's dismiss button is activated. Remove the item from your collection here.</summary>
    public event EventHandler<CdsNotificationsPanelItem>? NotificationDismissed;

    /// <summary>Raised when "Dismiss all" is activated. Clear your collection here.</summary>
    public event EventHandler? DismissAll;

    /// <summary>Raised when the "Do not disturb" toggle changes.</summary>
    public event EventHandler? DoNotDisturbChanged;

    /// <summary>Raised when "View all" is activated.</summary>
    public event EventHandler? ViewAll;

    /// <summary>Raised when the settings button is activated.</summary>
    public event EventHandler? SettingsClicked;

    /// <summary>Raised when the panel closes from a tap outside or Escape (Carbon: <c>onClickOutside</c>).</summary>
    public event EventHandler? ClickOutside;

    /// <summary>True while shown (for tests).</summary>
    internal bool IsShown => _overlay is not null;

    /// <summary>The rows shown, top to bottom (for tests).</summary>
    internal IReadOnlyList<View> Rows => _list?.Children.OfType<View>().ToList() ?? [];

    /// <summary>The "Dismiss all" button, once built (for tests).</summary>
    internal CdsButton? DismissAllButton => _dismissAll;

    IEnumerable<CdsNotificationsPanelItem> Items => ItemsSource?.OfType<CdsNotificationsPanelItem>() ?? [];

    void OnIsOpenChanged()
    {
        if (IsOpen)
        {
            if (IsLoaded) Show();
        }
        else
        {
            Hide();
        }
    }

    void OnItemsSourceChanged()
    {
        if (_overlay is not null) Watch(true);
        Rebuild();
    }

    void SyncDoNotDisturb()
    {
        if (_toggle is not null && _toggle.IsToggled != IsDoNotDisturb) _toggle.IsToggled = IsDoNotDisturb;
    }

    void Watch(bool on)
    {
        if (_source is not null) _source.CollectionChanged -= OnSourceChanged;
        foreach (var item in _watched) item.PropertyChanged -= OnItemChanged;
        _watched.Clear();
        _source = null;
        if (!on) return;
        _source = ItemsSource as INotifyCollectionChanged;
        if (_source is not null) _source.CollectionChanged += OnSourceChanged;
        foreach (var item in Items)
        {
            item.PropertyChanged += OnItemChanged;
            _watched.Add(item);
        }
    }

    void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Watch(true);
        _rebuild.Request();
    }

    void OnItemChanged(object? sender, PropertyChangedEventArgs e) => _rebuild.Request();

    void EnsureSurface()
    {
        if (_surface is not null) return;
        _list = new VerticalStackLayout { Spacing = 0 };
        _surface = new Grid
        {
            RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)],
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
        };
        CdsTokens.SetColor(_surface, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(_surface, CdsColorKeys.Overlay, 2, 4);
    }

    void Rebuild()
    {
        _rebuild.Cancel();
        if (_surface is null || _list is null) return;
        _surface.Children.Clear();

        // Header: title and Dismiss all, then the optional Do not disturb toggle; a rule under it.
        var title = new CdsText { Text = Title, TypeStyle = CdsTypeStyle.HeadingCompact01, VerticalOptions = LayoutOptions.Center };
        SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level2);
        _dismissAll = new CdsButton(DismissAllLabel, CdsButtonKind.Ghost) { Size = CdsSize.Sm, HorizontalOptions = LayoutOptions.End };
        _dismissAll.Clicked += (_, _) =>
        {
            DismissAll?.Invoke(this, EventArgs.Empty);
            if (DismissAllCommand?.CanExecute(null) == true) DismissAllCommand.Execute(null);
        };
        var headerRow = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)] };
        headerRow.Add(title, 0);
        headerRow.Add(_dismissAll, 1);
        var header = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03) };
        header.Add(headerRow);
        _toggle = null;
        if (HasDoNotDisturb)
        {
            _toggle = new CdsToggle { Size = CdsToggleSize.Sm, LabelText = DoNotDisturbLabel, HideLabel = true, LabelA = DoNotDisturbLabel, LabelB = DoNotDisturbLabel, IsToggled = IsDoNotDisturb, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
            _toggle.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != CdsToggle.IsToggledProperty.PropertyName || _toggle is null) return;
                if (IsDoNotDisturb == _toggle.IsToggled) return;
                IsDoNotDisturb = _toggle.IsToggled;
                DoNotDisturbChanged?.Invoke(this, EventArgs.Empty);
            };
            header.Add(_toggle);
        }
        var headerBox = new Grid();
        headerBox.Add(header);
        headerBox.Add(Rule(LayoutOptions.End));
        _surface.Add(headerBox, 0, 0);

        // Groups.
        _list.Children.Clear();
        var items = Items.ToList();
        if (items.Count == 0)
        {
            // Carbon: a NotificationsEmptyState with the label as its subtitle, 160 below the header.
            _list.Add(new CdsNotificationsEmptyState
            {
                Title = "",
                Subtitle = EmptyStateLabel,
                Size = CdsEmptyStateSize.Sm,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing13, CdsSpacing.Spacing05, 0),
            });
        }
        else
        {
            foreach (var (group, members) in NotificationsPanelModel.Group(items, i => i.Timestamp, DateTimeOffset.Now))
            {
                var label = new CdsText
                {
                    Text = group switch { NotificationsPanelGroup.Today => TodayLabel, NotificationsPanelGroup.Yesterday => YesterdayLabel, _ => PreviousLabel },
                    TypeStyle = CdsTypeStyle.Label01,
                    Tone = CdsTextTone.Secondary,
                    FontFamily = CdsFonts.SansSemiBold,
                    Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03),
                };
                SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level3);
                _list.Add(label);
                for (var i = 0; i < members.Count; i++)
                    _list.Add(new NotificationRow(this, members[i], first: i == 0));
            }
        }
        _surface.Add(new ScrollView { Orientation = ScrollOrientation.Vertical, Content = _list }, 0, 1);

        // Footer: View all (n) and settings.
        _footer = null;
        if (HasFooterActions && items.Count > 0)
        {
            var viewAll = new CdsButton(string.Format(System.Globalization.CultureInfo.CurrentCulture, ViewAllLabelFormat, items.Count), CdsButtonKind.Ghost) { Size = CdsSize.Md, HorizontalOptions = LayoutOptions.Fill };
            viewAll.Clicked += (_, _) =>
            {
                ViewAll?.Invoke(this, EventArgs.Empty);
                if (ViewAllCommand?.CanExecute(null) == true) ViewAllCommand.Execute(null);
            };
            var settings = new CdsIconButton(CdsIconName.Settings, SettingsLabel, CdsIconButtonKind.Ghost) { Size = CdsSize.Md, TooltipPlacement = CdsPlacement.Left };
            settings.Clicked += (_, _) =>
            {
                SettingsClicked?.Invoke(this, EventArgs.Empty);
                if (SettingsCommand?.CanExecute(null) == true) SettingsCommand.Execute(null);
            };
            var divider = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true };
            CdsTokens.SetColor(divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
            _footer = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], HeightRequest = CdsSpacing.Spacing08 };
            _footer.Add(viewAll, 0);
            _footer.Add(divider, 0);
            _footer.Add(settings, 1);
            _footer.Add(Rule(LayoutOptions.Start));
            Grid.SetColumnSpan(_footer.Children[^1] as BindableObject ?? _footer, 2);
            _surface.Add(_footer, 0, 2);
        }

        // Start and bottom edges: 1px border-subtle.
        var start = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
        CdsTokens.SetColor(start, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _surface.Add(start, 0, 0);
        Grid.SetRowSpan(start, 3);
        var bottom = Rule(LayoutOptions.End);
        _surface.Add(bottom, 0, 2);
        CdsSemantics.SetGroup(_surface, Title, CdsSemanticGroupKind.Group);
        ApplyViewport();
    }

    static BoxView Rule(LayoutOptions vertical)
    {
        var rule = new BoxView { HeightRequest = 1, VerticalOptions = vertical, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        return rule;
    }

    void ApplyViewport()
    {
        if (_surface is null) return;
        var host = CdsOverlayHost.Find(this) ?? CdsOverlayService.FindCurrentHost();
        var top = CdsShellContext.FindShell(this) is not null ? UIShellModel.HeaderHeight : 0;
        var width = host is { Width: > 0 } ? Math.Min(PanelWidth, host.Width) : PanelWidth;
        var height = host is { Height: > 0 } ? Math.Min(PanelHeight, host.Height - top) : PanelHeight;
        _surface.WidthRequest = width;
        _surface.HeightRequest = Math.Max(0, height);
        _surface.Margin = new Thickness(0, top, 0, 0);
    }

    internal bool IsExpanded(CdsNotificationsPanelItem item) => _expanded.TryGetValue(item, out _);

    internal void ToggleExpanded(CdsNotificationsPanelItem item)
    {
        if (!_expanded.Remove(item)) _expanded.Add(item, true);
        _rebuild.Request();
    }

    internal string FormatTime(DateTimeOffset time) =>
        TimeFormatter?.Invoke(time) ?? NotificationsPanelModel.TimeAgo(time, DateTimeOffset.Now);

    internal void Activate(CdsNotificationsPanelItem item)
    {
        NotificationClicked?.Invoke(this, item);
        if (NotificationCommand?.CanExecute(item) == true) NotificationCommand.Execute(item);
    }

    internal void Dismiss(CdsNotificationsPanelItem item)
    {
        NotificationDismissed?.Invoke(this, item);
        if (DismissNotificationCommand?.CanExecute(item) == true) DismissNotificationCommand.Execute(item);
    }

    internal void Show()
    {
        if (_overlay is not null) return;
        EnsureSurface();
        Watch(true);
        Rebuild();
        var surface = _surface!;
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Dock = CdsOverlayDock.TopEnd,
            LightDismiss = true,
            CloseOnEscape = false,
            ReturnFocusTo = TriggerElement,
            InitialFocus = _dismissAll,
            KeyHandler = e =>
            {
                if (e.Key != CdsKey.Escape) return false;
                _overlay?.Close();
                return true;
            },
        }, this);
        _overlay.Closed += OnOverlayClosed;
        void Refocus(object? s, EventArgs e)
        {
            surface.Loaded -= Refocus;
            _dismissAll?.Focus();
        }
        surface.Loaded += Refocus;
    }

    internal void Hide()
    {
        if (_overlay is not { } overlay) return;
        _hiding = true;
        overlay.Close();
        _hiding = false;
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        Watch(false);
        if (_hiding) return;
        ClickOutside?.Invoke(this, EventArgs.Empty);
        if (IsOpen) IsOpen = false;
    }

    /// <summary>One notification: status icon, time, title, description with Read more, link and a dismiss button.</summary>
    internal sealed class NotificationRow : CdsControl
    {
        readonly Grid _root = new() { MinimumHeightRequest = 100 };
        readonly BoxView _fill = new() { InputTransparent = true };
        readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
        readonly CdsIconButton _dismiss;
        readonly BoxView? _divider;

        public NotificationRow(CdsNotificationsPanel owner, CdsNotificationsPanelItem item, bool first)
        {
            Item = item;
            var hit = CreateHitTarget();
            var time = owner.FormatTime(item.Timestamp);
            SemanticProperties.SetDescription(hit, string.Join(", ", new[] { item.Title, time, item.Description }.Where(s => !string.IsNullOrEmpty(s))));
            hit.Clicked += (_, _) => owner.Activate(item);

            var content = new VerticalStackLayout { Spacing = 0, InputTransparent = true, CascadeInputTransparent = false };
            var timeText = new CdsText { Text = time, TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
            var titleText = new CdsText
            {
                Text = item.Title,
                TypeStyle = CdsTypeStyle.Heading01,
                Tone = CdsTextTone.Secondary,
                FontFamily = item.IsUnread ? CdsFonts.SansSemiBold : CdsFonts.Sans,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing02),
            };
            content.Add(timeText);
            content.Add(titleText);
            AutomationProperties.SetExcludedWithChildren(timeText, true);
            AutomationProperties.SetExcludedWithChildren(titleText, true);
            if (!string.IsNullOrEmpty(item.Description))
            {
                var expanded = owner.IsExpanded(item);
                var description = new CdsText { Text = item.Description, TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.WordWrap };
                if (!expanded && NotificationsPanelModel.NeedsReadMore(item.Description))
                {
                    description.MaxLines = 2;
                    description.LineBreakMode = LineBreakMode.TailTruncation;
                }
                AutomationProperties.SetExcludedWithChildren(description, true);
                content.Add(description);
                if (NotificationsPanelModel.NeedsReadMore(item.Description))
                {
                    var more = new CdsButton(expanded ? owner.ReadLessLabel : owner.ReadMoreLabel, CdsButtonKind.Ghost)
                    {
                        Size = CdsSize.Sm,
                        Icon = expanded ? CdsIconName.ChevronUp : CdsIconName.ChevronDown,
                        Margin = new Thickness(-CdsSpacing.Spacing05, 0, 0, 0),
                    };
                    more.Clicked += (_, _) => owner.ToggleExpanded(item);
                    content.Add(more);
                }
            }
            if (!string.IsNullOrEmpty(item.LinkText) && !string.IsNullOrEmpty(item.LinkUrl))
                content.Add(new CdsLink { Text = item.LinkText, Href = item.LinkUrl, Size = CdsLinkSize.Sm });

            var row = new Grid
            {
                ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)],
                Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing07, CdsSpacing.Spacing05),
                InputTransparent = true,
                CascadeInputTransparent = false,
            };
            if (item.Kind is { } kind)
            {
                var (glyph, color) = kind switch
                {
                    CdsNotificationKind.Error => (CdsIconName.ErrorFilled, CdsColorKeys.SupportError),
                    CdsNotificationKind.Success => (CdsIconName.CheckmarkFilled, CdsColorKeys.SupportSuccess),
                    CdsNotificationKind.Warning or CdsNotificationKind.WarningAlt => (CdsIconName.WarningAltFilled, CdsColorKeys.SupportWarning),
                    _ => (CdsIconName.InformationSquareFilled, CdsColorKeys.SupportInfo),
                };
                var icon = new CdsIcon(glyph, 16) { VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, CdsSpacing.Spacing03, 0), InputTransparent = true };
                CdsTokens.SetColor(icon, CdsIcon.ColorProperty, color);
                row.Add(icon, 0);
            }
            row.Add(content, 1);

            _dismiss = new CdsIconButton(CdsIconName.Close, owner.DismissLabel, CdsIconButtonKind.Ghost)
            {
                Size = CdsSize.Sm,
                TooltipPlacement = CdsPlacement.Left,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
            };
            _dismiss.Clicked += (_, _) => owner.Dismiss(item);

            AutomationProperties.SetExcludedWithChildren(_fill, true);
            _root.Add(_fill);
            _root.Add(hit);
            _root.Add(_ring);
            _root.Add(row);
            _root.Add(_dismiss);
            if (!first)
            {
                _divider = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
                _root.Add(_divider);
            }
            _ring.InputTransparent = true;
            TrackHover();
            SetChrome(_root, fillsWidth: true);
            RefreshVisuals();
        }

        public CdsNotificationsPanelItem Item { get; }

        protected override void ApplyVisuals()
        {
            var active = Is(CdsInteractionState.Hover) || Is(CdsInteractionState.FocusVisible);
            SetColor(_fill, BoxView.ColorProperty, active ? CdsColorKeys.LayerAccent01 : CdsColorKeys.Layer01);
            if (_divider is not null) SetColor(_divider, BoxView.ColorProperty, active ? null : CdsColorKeys.BorderSubtle01);
            _ring.IsActive = Is(CdsInteractionState.FocusVisible);
            // The dismiss button shows on hover and focus with a pointer; touch screens have no hover, so it always shows there.
            var touch = DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet;
            _dismiss.Opacity = touch || active || _dismiss.Is(CdsInteractionState.Focused) ? 1 : 0;
        }
    }
}
