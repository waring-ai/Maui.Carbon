using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A row of tags that shows as many as fit its width and collapses the rest into a "+N" overflow tag (Carbon for IBM
/// Products' TagSet). The overflow tag opens a high-contrast popover listing the hidden tags; with more than 10 hidden,
/// the popover offers "View all tags", a searchable modal of every tag. Use it for an item's labels in a width that
/// cannot grow (a table cell, a card). Declare <see cref="CdsTagSetItem"/> children; <see cref="IsMultiline"/> wraps
/// instead, and <see cref="MaxVisible"/> caps the shown tags. Handle <see cref="OverflowClicked"/> (or set
/// <see cref="OverflowCommand"/>) to show the hidden tags your own way.
/// </summary>
[ContentProperty(nameof(Items))]
public partial class CdsTagSet : CdsControl
{
    sealed record Entry(CdsTagSetItem Item, CdsTag Sizer, CdsTag Tag);

    readonly CdsItemCollection<CdsTagSetItem> _items = [];
    readonly List<Entry> _entries = [];
    readonly Grid _chrome = new();
    readonly CdsTagSetPanel _panel = new();
    readonly CdsCoalescedAction _rebuild;
    readonly CdsTag _overflowSizer = new() { Opacity = 0, InputTransparent = true, IsEnabled = false };
    readonly CdsOperationalTag _overflowTag = new() { IsExpanded = false };
    readonly CdsPopover _popover;
    IReadOnlyList<CdsTagSetItem> _overflowItems = [];
    int _count = -1;
    int _requestedCount = -1;
    bool _applyPending;
    bool _syncingPopover;
    VerticalStackLayout? _popoverContent;
    VerticalStackLayout? _popoverList;
    CdsLink? _showAll;
    CdsModal? _modal;
    CdsSearch? _search;
    CdsTagSetPanel? _modalPanel;
    readonly List<(CdsTag Tag, CdsTagSetItem Item)> _modalTags = [];

    /// <summary>Creates an empty tag set.</summary>
    public CdsTagSet()
    {
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        _items.CollectionChanged += OnItemsChanged;

        AutomationProperties.SetExcludedWithChildren(_overflowSizer, true);
        _overflowTag.Clicked += OnOverflowClicked;
        _popover = new CdsPopover { Body = _overflowTag, IsHighContrast = true, HasDropShadow = true, IsVisible = false };
        _popover.Closed += OnPopoverClosed;

        _panel.OverflowSizer = _overflowSizer;
        _panel.Overflow = _popover;
        _panel.Children.Add(_overflowSizer);
        _panel.Children.Add(_popover);
        _panel.CountNeeded += RequestCount;
        _chrome.Add(_panel);

        VerticalOptions = LayoutOptions.Center;
        SetChrome(_chrome, fillsWidth: true);
        ApplySettings();
        Rebuild();
    }

    /// <summary>
    /// Raised when the "+N" tag is activated, before the popover opens. Set
    /// <see cref="CdsTagSetOverflowEventArgs.Handled"/> to show the hidden tags your own way instead.
    /// </summary>
    public event EventHandler<CdsTagSetOverflowEventArgs>? OverflowClicked;

    /// <summary>Raised when the split between shown and hidden tags changes (<c>onOverflowTagChange</c>); read <see cref="OverflowItems"/>.</summary>
    public event EventHandler? OverflowItemsChanged;

    /// <summary>The tags, in order (<c>tags</c>).</summary>
    public IList<CdsTagSetItem> Items => _items;

    /// <summary>The tags hidden in the overflow, in order.</summary>
    public IReadOnlyList<CdsTagSetItem> OverflowItems => _overflowItems;

    /// <summary>How many tags are shown in the row.</summary>
    public int VisibleCount => Math.Max(0, _count);

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_popoverList is null) return;
        var v = CdsTagSetTokens.Entry(OverflowType);
        foreach (var child in _popoverList.Children)
        {
            if (child is Border pill)
            {
                SetColor(pill, BackgroundColorProperty, v.Background);
                if (pill.Content is CdsText t) SetColor(t, Label.TextColorProperty, v.Foreground);
            }
            else if (child is CdsText text)
            {
                SetColor(text, Label.TextColorProperty, v.Foreground);
            }
        }
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsTagSetItem item in e.OldItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
                item.Parent = null;
            }
        if (e.NewItems is not null)
            foreach (CdsTagSetItem item in e.NewItems)
            {
                item.Parent = this;
                item.PropertyChanged += OnItemPropertyChanged;
            }
        _rebuild.Request();
    }

    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => _rebuild.Request();

    void ApplySettings()
    {
        _panel.Align = Align;
        _panel.Multiline = IsMultiline;
        _panel.MaxVisible = MaxVisible;
        _overflowTag.Size = Size;
        _overflowSizer.Size = Size;
        _popover.Placement = OverflowPlacement;
        ((IView)_panel).InvalidateMeasure();
    }

    // Builds the shown tags and their invisible measuring copies, keeping the views of items still present.
    void Rebuild()
    {
        var old = _entries.ToList();
        _entries.Clear();
        foreach (var item in _items)
        {
            var index = old.FindIndex(e => e.Item == item && e.Tag is CdsDismissibleTag == item.IsDismissible);
            Entry entry;
            if (index >= 0)
            {
                entry = old[index];
                old.RemoveAt(index);
            }
            else
            {
                entry = Create(item);
                _panel.Children.Insert(0, entry.Sizer);
                _panel.Children.Add(entry.Tag);
            }
            Sync(entry);
            _entries.Add(entry);
        }
        foreach (var gone in old)
        {
            if (gone.Tag is CdsDismissibleTag d) d.Dismissed -= OnTagDismissed;
            _panel.Children.Remove(gone.Sizer);
            _panel.Children.Remove(gone.Tag);
        }

        _panel.Sizers.Clear();
        _panel.Tags.Clear();
        foreach (var e in _entries)
        {
            _panel.Sizers.Add(e.Sizer);
            _panel.Tags.Add(e.Tag);
        }
        _overflowSizer.Text = TagSetModel.OverflowText(_items.Count);

        // Show everything until the next layout decides the fit (arrange keeps tags past the fit off screen meanwhile).
        _count = -1;
        ApplyCount(Math.Min(_items.Count, MaxVisible > 0 ? MaxVisible : _items.Count));
        if (_modalPanel is not null) FillModal();
        ((IView)_panel).InvalidateMeasure();
    }

    Entry Create(CdsTagSetItem item)
    {
        CdsTag sizer, tag;
        if (item.IsDismissible)
        {
            sizer = new CdsDismissibleTag();
            var d = new CdsDismissibleTag();
            d.Dismissed += OnTagDismissed;
            tag = d;
        }
        else
        {
            sizer = new CdsTag();
            tag = new CdsTag();
        }
        sizer.Opacity = 0;
        sizer.InputTransparent = true;
        sizer.IsEnabled = false;
        AutomationProperties.SetExcludedWithChildren(sizer, true);
        return new Entry(item, sizer, tag);
    }

    void Sync(Entry e)
    {
        foreach (var tag in new[] { e.Sizer, e.Tag })
        {
            tag.Text = e.Item.Text;
            tag.Type = e.Item.Type;
            tag.Icon = e.Item.Icon;
            tag.Size = Size;
            if (tag is CdsDismissibleTag d) d.DismissLabel = e.Item.DismissLabel;
        }
        e.Tag.IsEnabled = e.Item.IsEnabled;
    }

    void OnTagDismissed(object? sender, EventArgs e)
    {
        var entry = _entries.FirstOrDefault(x => x.Tag == sender);
        if (entry is null) return;
        // A shown tag closed: close the popover, as Carbon for IBM Products does.
        if (_entries.IndexOf(entry) < VisibleCount) IsOverflowOpen = false;
        entry.Item.RaiseDismissed();
    }

    void RequestCount(int count)
    {
        _requestedCount = count;
        if (count == _count || _applyPending) return;
        if (Dispatcher is { } dispatcher && Handler is not null)
        {
            _applyPending = true;
            dispatcher.Dispatch(() =>
            {
                _applyPending = false;
                ApplyCount(_requestedCount);
            });
        }
        else
        {
            ApplyCount(count);
        }
    }

    void ApplyCount(int count)
    {
        count = Math.Clamp(count, 0, _items.Count);
        if (count == _count) return;
        _count = count;
        for (var i = 0; i < _entries.Count; i++) _entries[i].Tag.IsVisible = i < count;
        var hidden = _items.Count - count;
        _overflowItems = _items.Skip(count).ToList();
        _overflowTag.Text = TagSetModel.OverflowText(hidden);
        _popover.IsVisible = hidden > 0;
        if (_popoverContent is not null) FillPopover();
        SyncPopover();
        OverflowItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    void OnOverflowClicked(object? sender, EventArgs e)
    {
        var args = new CdsTagSetOverflowEventArgs(_overflowItems);
        OverflowClicked?.Invoke(this, args);
        if (OverflowCommand is { } command)
        {
            if (command.CanExecute(_overflowItems)) command.Execute(_overflowItems);
            return;
        }
        if (args.Handled) return;
        IsOverflowOpen = !IsOverflowOpen;
    }

    void SyncPopover()
    {
        var open = IsOverflowOpen && _overflowItems.Count > 0;
        _overflowTag.IsExpanded = open;
        if (open) EnsurePopover();
        _syncingPopover = true;
        try
        {
            _popover.IsOpen = open;
        }
        finally
        {
            _syncingPopover = false;
        }
    }

    void OnPopoverClosed(object? sender, EventArgs e)
    {
        if (_syncingPopover) return;
        if (IsOverflowOpen && _overflowItems.Count > 0) IsOverflowOpen = false;
    }

    void EnsurePopover()
    {
        if (_popoverContent is not null) return;
        _popoverList = new VerticalStackLayout();
        CdsSemantics.SetGroup(_popoverList, null, CdsSemanticGroupKind.List);
        _showAll = new CdsLink { IsInverse = true, Margin = new Thickness(0, CdsTagSetTokens.ShowAllGap, 0, 0), HorizontalOptions = LayoutOptions.Start };
        _showAll.Clicked += OnShowAllClicked;
        _popoverContent = new VerticalStackLayout { Padding = new Thickness(CdsTagSetTokens.PopoverPadding) };
        _popoverContent.Add(_popoverList);
        _popoverContent.Add(_showAll);
        _popover.PopoverContent = _popoverContent;
        FillPopover();
    }

    void FillPopover()
    {
        if (_popoverList is null || _showAll is null) return;
        var (listed, showsAll) = TagSetModel.PopoverList(_overflowItems.Count);
        _popoverList.Children.Clear();
        _popoverList.Spacing = CdsTagSetTokens.EntrySpacing(OverflowType);
        for (var i = 0; i < listed; i++) _popoverList.Children.Add(CreateEntry(_overflowItems[i]));
        _showAll.Text = ShowAllTagsLabel;
        _showAll.IsVisible = showsAll;
        RefreshVisuals();
    }

    View CreateEntry(CdsTagSetItem item)
    {
        if (OverflowType == CdsTagSetOverflowType.Default)
        {
            return new CdsText
            {
                Text = item.Text,
                TypeStyle = CdsTypeStyle.BodyCompact01,
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaximumWidthRequest = CdsTagSetTokens.EntryMaxWidth,
                HorizontalOptions = LayoutOptions.Start,
            };
        }
        var height = CdsTagTokens.Height(Size);
        var padding = Size == CdsTagSize.Lg ? CdsSpacing.Spacing05 : CdsSpacing.Spacing03;
        return new Border
        {
            StrokeThickness = 0,
            HeightRequest = height,
            Padding = new Thickness(padding, 0),
            HorizontalOptions = LayoutOptions.Start,
            StrokeShape = new RoundRectangle { CornerRadius = CdsRadius.Circle(height) },
            Content = new CdsText
            {
                Text = item.Text,
                TypeStyle = CdsTypeStyle.Label01,
                MaxLines = 1,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaximumWidthRequest = CdsTagSetTokens.EntryMaxWidth - 2 * padding,
                VerticalOptions = LayoutOptions.Center,
            },
        };
    }

    void OnShowAllClicked(object? sender, EventArgs e)
    {
        IsOverflowOpen = false;
        IsAllTagsModalOpen = true;
    }

    void EnsureModal()
    {
        if (_modal is not null) return;
        _search = new CdsSearch { Size = CdsSize.Lg, Margin = new Thickness(0, 0, 0, CdsTagSetTokens.ModalSearchGap) };
        _search.TextChanged += (_, _) => FilterModal();
        _modalPanel = new CdsTagSetPanel();
        var body = new VerticalStackLayout();
        body.Add(_search);
        body.Add(_modalPanel);
        _modal = new CdsModal
        {
            Size = CdsModalSize.Sm,
            IsPassive = true,
            HasForm = true,
            HasScrollingContent = true,
            Body = body,
            LauncherElement = _overflowTag,
            InitialFocusElement = _search,
        };
        _modal.PropertyChanged += OnModalPropertyChanged;
        _chrome.Add(_modal);
        ApplyModalTexts();
        FillModal();
    }

    void ApplyModalTexts()
    {
        if (_modal is null || _search is null || _modalPanel is null) return;
        _modal.Heading = AllTagsModalTitle;
        _modal.CloseButtonLabel = AllTagsModalCloseLabel;
        _search.Label = AllTagsModalSearchLabel;
        _search.Placeholder = AllTagsModalSearchPlaceholderText;
        _search.CloseButtonLabel = AllTagsModalSearchClearLabel;
        CdsSemantics.SetGroup(_modalPanel, AllTagsModalAriaLabel, CdsSemanticGroupKind.List);
    }

    void FillModal()
    {
        if (_modalPanel is null) return;
        _modalPanel.Children.Clear();
        _modalPanel.Tags.Clear();
        _modalTags.Clear();
        foreach (var item in _items)
        {
            var tag = new CdsTag { Text = item.Text, Type = item.Type, Icon = item.Icon, Size = Size, IsEnabled = item.IsEnabled };
            _modalPanel.Children.Add(tag);
            _modalPanel.Tags.Add(tag);
            _modalTags.Add((tag, item));
        }
        FilterModal();
    }

    void FilterModal()
    {
        if (_modalPanel is null) return;
        var query = _search?.Text;
        foreach (var (tag, item) in _modalTags) tag.IsVisible = TagSetModel.Matches(item.Text, item.SearchText, query);
        ((IView)_modalPanel).InvalidateMeasure();
    }

    void OnModalPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == CdsModal.IsOpenProperty.PropertyName && _modal is { IsOpen: false } && IsAllTagsModalOpen)
            IsAllTagsModalOpen = false;
    }

    void SyncModal()
    {
        if (IsAllTagsModalOpen) EnsureModal();
        if (_modal is null) return;
        if (IsAllTagsModalOpen && _search is not null) _search.Text = null;
        _modal.IsOpen = IsAllTagsModalOpen;
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsTagSet)b).ApplySettings();

    static void OnItemsLayoutChanged(BindableObject b, object? o, object? n)
    {
        var set = (CdsTagSet)b;
        set.ApplySettings();
        set._rebuild.Request();
    }

    static void OnPopoverChanged(BindableObject b, object? o, object? n)
    {
        var set = (CdsTagSet)b;
        set.ApplySettings();
        set.FillPopover();
    }

    static void OnModalChanged(BindableObject b, object? o, object? n) => ((CdsTagSet)b).ApplyModalTexts();

    static void OnPopoverOpenChanged(BindableObject b, object? o, object? n) => ((CdsTagSet)b).SyncPopover();

    static void OnModalOpenChanged(BindableObject b, object? o, object? n) => ((CdsTagSet)b).SyncModal();
}
