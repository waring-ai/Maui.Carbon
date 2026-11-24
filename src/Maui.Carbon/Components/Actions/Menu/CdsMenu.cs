using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A floating list of actions: items, dividers, groups, selectable items, radio groups and one level of nested
/// submenus. It is the shared primitive behind <see cref="CdsMenuButton"/>, <see cref="CdsOverflowMenu"/> and
/// <see cref="CdsComboButton"/>, and is meant to be reused for row actions, header menus and breadcrumb overflow.
/// <para>
/// A <see cref="CdsMenu"/> is not a view: declare it (or build it in code), then show it next to any element with
/// <see cref="Open"/>. It uses <b>virtual focus</b>: focus stays on the anchor while Up/Down/Home/End move a
/// highlight (skipping disabled items and dividers, wrapping at the ends), Enter or Space chooses, Right opens a
/// submenu, Left or Escape closes it, and Escape on the root closes the menu and returns focus to the anchor.
/// Choosing any item closes the whole menu first, then runs the item. Because focus stays on the anchor, the highlighted
/// item is announced to screen readers (label, state and position) when the menu opens and as the highlight moves.
/// </para>
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsMenu : BindableObject
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsMenu), null);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsMenu), CdsSize.Sm,
        coerceValue: static (_, v) => CdsMenuTokens.Coerce((CdsSize)v));

    /// <summary>Bindable property for <see cref="HasBorder"/>.</summary>
    public static readonly BindableProperty HasBorderProperty = BindableProperty.Create(
        nameof(HasBorder), typeof(bool), typeof(CdsMenu), false);

    /// <summary>Bindable property for <see cref="BackgroundToken"/>.</summary>
    public static readonly BindableProperty BackgroundTokenProperty = BindableProperty.Create(
        nameof(BackgroundToken), typeof(CdsMenuBackgroundToken), typeof(CdsMenu), CdsMenuBackgroundToken.Layer);

    readonly CdsItemCollection<CdsMenuItemBase> _items = [];
    Level? _root;

    /// <summary>Creates an empty menu.</summary>
    public CdsMenu()
    {
        _items.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
                foreach (CdsMenuItemBase item in e.NewItems) SetInheritedBindingContext(item, BindingContext);
        };
    }

    /// <summary>Raised after the menu opened.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised after the menu closed (an item was chosen, Escape, light dismiss, or <see cref="Close"/>).</summary>
    public event EventHandler? Closed;

    /// <summary>The accessible name of the menu ("Object actions").</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Row height: xs 24, sm 32 (default), md 40, lg 48. Submenus inherit it. Match the trigger.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Draws a 1px <c>border-subtle-01</c> outline around the surface (menus over a busy background).</summary>
    public bool HasBorder
    {
        get => (bool)GetValue(HasBorderProperty);
        set => SetValue(HasBorderProperty, value);
    }

    /// <summary>
    /// The surface fill: <see cref="CdsMenuBackgroundToken.Layer"/> (<c>layer-01</c>, default) or
    /// <see cref="CdsMenuBackgroundToken.Background"/> (<c>background</c>), for a menu that opens over a <c>layer-01</c>
    /// surface. Pair it with <see cref="HasBorder"/> so the edge stays visible. Submenus use the same fill.
    /// </summary>
    public CdsMenuBackgroundToken BackgroundToken
    {
        get => (CdsMenuBackgroundToken)GetValue(BackgroundTokenProperty);
        set => SetValue(BackgroundTokenProperty, value);
    }

    /// <summary>The items, in order. Order by frequency or task sequence; destructive items last, after a divider.</summary>
    public IList<CdsMenuItemBase> Items => _items;

    /// <summary>True while the menu is shown.</summary>
    public bool IsOpen => _root?.Overlay is { IsOpen: true };

    /// <summary>
    /// Shows the menu next to <paramref name="anchor"/> as an overlay and highlights its first enabled item. Returns the
    /// open overlay when the menu is already open. The anchor must be loaded.
    /// </summary>
    /// <param name="anchor">The element the menu is attached to; it keeps keyboard focus and gets it back on close.</param>
    /// <param name="placement">Where the menu opens. Default <see cref="CdsPlacement.BottomStart"/>; flips when it does not fit.</param>
    public CdsOverlay Open(VisualElement anchor, CdsPlacement placement = CdsPlacement.BottomStart) => OpenCore(anchor, placement, 0);

    /// <summary>Opens the menu with a minimum width (the trigger's width for <c>Bottom</c>/<c>Top</c> alignment).</summary>
    internal CdsOverlay OpenCore(VisualElement anchor, CdsPlacement placement, double minimumWidth, VisualElement? returnFocusTo = null)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        if (_root?.Overlay is { IsOpen: true } existing) return existing;
        if (!anchor.IsLoaded)
            throw new InvalidOperationException("The anchor must be loaded before a CdsMenu can open next to it.");

        var level = BuildLevel(_items, parent: null, parentIndex: -1, minimumWidth);
        level.Model.MoveFirst();
        level.KeyboardFocus = true;
        Refresh(level);
        _root = level;

        var overlay = CdsOverlayService.Open(level.Surface, new CdsOverlayOptions
        {
            Anchor = anchor,
            Placement = placement,
            ReturnFocusTo = returnFocusTo,
            LightDismiss = true,
            CloseOnEscape = false,
            Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation,
            KeyHandler = OnKey,
        });
        level.Overlay = overlay;
        overlay.Closed += (_, _) =>
        {
            if (ReferenceEquals(_root, level)) _root = null;
            CloseChild(level);
            Closed?.Invoke(this, EventArgs.Empty);
        };
        Opened?.Invoke(this, EventArgs.Empty);
        // Opening focuses the first item (Carbon); with virtual focus that item is announced instead.
        Announce(level, previous: -1, CdsAnnouncePriority.Polite);
        return overlay;
    }

    /// <summary>Closes the menu and any open submenu; focus returns to the anchor.</summary>
    public void Close() => _root?.Overlay?.Close();

    /// <summary>
    /// Closes the menu because keyboard focus left its trigger (Tab, or a click elsewhere that took focus): Carbon closes
    /// a menu when focus leaves it. Focus is not returned to the anchor, so it stays where it went.
    /// </summary>
    internal void CloseForFocusLoss()
    {
        if (_root?.Overlay is not { IsOpen: true } overlay) return;
        overlay.Options.ReturnFocus = false;
        overlay.Close();
    }

    /// <summary>The rows of the open root menu (dividers are null), for tests.</summary>
    internal CdsMenuRow?[] OpenRows => _root?.Rows ?? [];

    /// <summary>The highlighted row index of the innermost open level, or -1.</summary>
    internal int ActiveHighlight => ActiveLevel()?.Model.Highlighted ?? -1;

    /// <summary>
    /// Builds the menu surface as an ordinary view without showing an overlay: for documentation, previews and
    /// custom hosts. Rows respond to the pointer; keyboard handling needs <see cref="Open"/>.
    /// </summary>
    /// <param name="highlightedIndex">Index of the row to show highlighted (counting dividers), or -1.</param>
    public View CreatePreview(int highlightedIndex = -1)
    {
        var level = BuildLevel(_items, parent: null, parentIndex: -1, 0);
        if (highlightedIndex >= 0 && level.Model.SetHighlight(highlightedIndex)) level.KeyboardFocus = true;
        Refresh(level);
        level.Surface.HorizontalOptions = LayoutOptions.Start;
        level.Surface.VerticalOptions = LayoutOptions.Start;
        return level.Surface;
    }

    sealed class Level
    {
        public required MenuModel Model { get; init; }
        public required Border Surface { get; init; }
        public required CdsMenuRow?[] Rows { get; init; }
        public Level? Parent { get; init; }
        public int ParentIndex { get; init; } = -1;
        public CdsOverlay? Overlay { get; set; }
        public Level? Child { get; set; }
        public bool KeyboardFocus { get; set; }
    }

    Level BuildLevel(IEnumerable<CdsMenuItemBase> items, Level? parent, int parentIndex, double minimumWidth)
    {
        var model = new MenuModel(items);
        var height = CdsMenuTokens.RowHeight(Size);
        var stack = new VerticalStackLayout { Spacing = 0 };
        var rows = new CdsMenuRow?[model.Rows.Count];

        var surface = new Border
        {
            StrokeShape = new Rectangle(),
            StrokeThickness = HasBorder ? 1 : 0,
            Padding = new Thickness(0, CdsMenuTokens.SurfacePadding),
            MinimumWidthRequest = CdsMenuTokens.MinimumWidth(model.HasIcons, minimumWidth),
            MaximumWidthRequest = CdsMenuTokens.MaxWidth,
            Content = stack,
        };
        CdsTokens.SetColor(surface, VisualElement.BackgroundColorProperty, CdsMenuTokens.SurfaceBackground(BackgroundToken));
        if (HasBorder) CdsTokens.SetBrush(surface, Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetShadow(surface, CdsColorKeys.Shadow, offsetY: 2, radius: 6);
        CdsSemantics.SetGroup(surface, Label, CdsSemanticGroupKind.List);

        var level = new Level { Model = model, Surface = surface, Rows = rows, Parent = parent, ParentIndex = parentIndex };

        for (var i = 0; i < model.Rows.Count; i++)
        {
            var info = model.Rows[i];
            if (info.Kind == MenuRowKind.Divider)
            {
                var line = new BoxView { HeightRequest = 1, InputTransparent = true };
                CdsTokens.SetColor(line, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
                stack.Add(new ContentView { Padding = new Thickness(0, CdsSpacing.Spacing02), Content = line, InputTransparent = true });
                continue;
            }

            var index = i;
            var row = new CdsMenuRow(info, height, model.HasIcons, model.HasSelection);
            row.HoverEntered += (_, _) => OnRowHover(level, index);
            row.Chosen += (_, _) => OnRowChosen(level, index);
            rows[i] = row;
            stack.Add(row);
        }
        return level;
    }

    static void Refresh(Level level)
    {
        for (var i = 0; i < level.Rows.Length; i++)
        {
            if (level.Rows[i] is not { } row) continue;
            row.ShowFocusRing = level.KeyboardFocus;
            row.IsHighlighted = i == level.Model.Highlighted;
        }
    }

    void OnRowHover(Level level, int index)
    {
        if (level.Child is { } child && child.ParentIndex != index) CloseChild(level);
        if (level.Model.SetHighlight(index) || level.KeyboardFocus)
        {
            level.KeyboardFocus = false;
            Refresh(level);
        }
        if (level.Model.Rows[index].HasSubmenu && level.Child is null && level.Rows[index] is { } row)
        {
            // Submenus open on hover after 100 ms, as in Carbon.
            _ = Task.Delay(100).ContinueWith(_ => row.Dispatcher.Dispatch(() =>
            {
                if (level.Model.Highlighted == index && level.Child is null && IsLevelOpen(level)) OpenSubmenu(level, index, keyboard: false);
            }), TaskScheduler.Default);
        }
    }

    bool IsLevelOpen(Level level) => level.Overlay is { IsOpen: true } && ReferenceEquals(Root(level), _root);

    static Level Root(Level level)
    {
        while (level.Parent is { } p) level = p;
        return level;
    }

    void OnRowChosen(Level level, int index)
    {
        var info = level.Model.Rows[index];
        if (info.HasSubmenu)
        {
            level.Model.SetHighlight(index);
            OpenSubmenu(level, index, keyboard: false);
        }
        else
        {
            Activate(info);
        }
    }

    void OpenSubmenu(Level level, int index, bool keyboard)
    {
        var info = level.Model.Rows[index];
        if (!info.HasSubmenu || level.Rows[index] is not { } anchorRow) return;
        if (level.Child is { } existing)
        {
            if (existing.ParentIndex == index) return;
            CloseChild(level);
        }

        var child = BuildLevel(info.Children, level, index, 0);
        child.Model.MoveFirst();
        child.KeyboardFocus = keyboard;
        Refresh(child);
        level.Child = child;
        anchorRow.IsSubmenuOpen = true;

        // The submenu sits beside its parent item, overlapping the parent's edge by 6 and aligned with the surface padding.
        child.Surface.Margin = new Thickness(0, -CdsMenuTokens.SurfacePadding, 0, 0);
        var overlay = CdsOverlayService.Open(child.Surface, new CdsOverlayOptions
        {
            Anchor = anchorRow,
            Placement = CdsPlacement.RightTop,
            Offset = -6,
            LightDismiss = false,
            CloseOnEscape = false,
            ReturnFocus = false,
            Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation,
            KeyHandler = OnKey,
        });
        child.Overlay = overlay;
        overlay.Closed += (_, _) =>
        {
            if (ReferenceEquals(level.Child, child)) level.Child = null;
            anchorRow.IsSubmenuOpen = false;
        };
    }

    static void CloseChild(Level level)
    {
        if (level.Child is not { } child) return;
        level.Child = null;
        CloseChild(child);
        child.Overlay?.Close();
    }

    void Activate(MenuRowInfo info)
    {
        // Close first so an action that opens a dialog is not undone by focus returning to the anchor.
        Close();
        switch (info.Source)
        {
            case CdsMenuItem item:
                item.Invoke();
                break;
            case CdsMenuItemSelectable selectable:
                selectable.IsSelected = !selectable.IsSelected;
                break;
            case CdsMenuItemRadioGroup radio:
                radio.SelectedItem = info.RadioValue;
                break;
        }
    }

    Level? ActiveLevel()
    {
        var level = _root;
        while (level?.Child is { } child) level = child;
        return level;
    }

    bool OnKey(CdsKeyEventArgs e)
    {
        if (ActiveLevel() is not { } level) return false;
        var before = level.Model.Highlighted;
        var result = level.Model.HandleKey(e.Key, level.Parent is null);
        switch (result.Action)
        {
            case MenuAction.Activate:
                if (level.Model.HighlightedRow is { } row) Activate(row);
                return true;
            case MenuAction.OpenSubmenu:
                var existing = level.Child;
                if (level.Model.Highlighted >= 0) OpenSubmenu(level, level.Model.Highlighted, keyboard: true);
                if (level.Child is { } child)
                {
                    Refresh(child);
                    if (!ReferenceEquals(child, existing)) Announce(child, previous: -1, CdsAnnouncePriority.Assertive);
                }
                return result.Handled;
            case MenuAction.CloseSubmenu:
                if (level.Parent is { } parent)
                {
                    CloseChild(parent);
                    parent.KeyboardFocus = true;
                    Refresh(parent);
                    Announce(parent, previous: -1, CdsAnnouncePriority.Assertive);
                }
                return true;
            case MenuAction.Close:
                Close();
                return result.Handled;
        }
        if (result.Handled)
        {
            level.KeyboardFocus = true;
            Refresh(level);
            // Moving the highlight stands in for moving focus, which a screen reader speaks at once.
            if (level.Model.Highlighted != before) Announce(level, before, CdsAnnouncePriority.Assertive);
        }
        return result.Handled;
    }

    /// <summary>Tells screen readers which row of a level is highlighted: its label, state and position.</summary>
    static void Announce(Level level, int previous, CdsAnnouncePriority priority)
    {
        if (level.Model.Highlighted < 0) return;
        CdsAnnouncer.Announce(level.Model.Describe(level.Model.Highlighted, previous), priority);
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var item in _items) SetInheritedBindingContext(item, BindingContext);
    }
}
