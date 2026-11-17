namespace Maui.Carbon;

/// <summary>What a row of a menu is.</summary>
internal enum MenuRowKind
{
    Item,
    Selectable,
    Radio,
    Divider,
}

/// <summary>One flattened row of a menu level: groups and radio groups are expanded into their rows.</summary>
internal sealed class MenuRowInfo
{
    public MenuRowKind Kind { get; init; }

    /// <summary>The source item (<see cref="CdsMenuItem"/>, <see cref="CdsMenuItemSelectable"/>, the divider or the radio group).</summary>
    public CdsMenuItemBase Source { get; init; } = null!;

    public string Text { get; init; } = string.Empty;
    public CdsIconName? Icon { get; init; }
    public string? Shortcut { get; init; }
    public bool IsEnabled { get; init; } = true;
    public bool IsDanger { get; init; }

    /// <summary>Selected (checkmark) state of selectable and radio rows.</summary>
    public bool IsSelected { get; init; }

    /// <summary>The radio choice this row stands for.</summary>
    public object? RadioValue { get; init; }

    /// <summary>The accessible name of the enclosing group, if any.</summary>
    public string? GroupLabel { get; init; }

    /// <summary>Items of the submenu this row opens (empty for plain rows).</summary>
    public IReadOnlyList<CdsMenuItemBase> Children { get; init; } = [];

    public bool HasSubmenu => Children.Count > 0;

    /// <summary>The keyboard can land on this row.</summary>
    public bool IsFocusable => Kind != MenuRowKind.Divider && IsEnabled;
}

/// <summary>What a key press asks the menu to do.</summary>
internal enum MenuAction
{
    None,
    Activate,
    OpenSubmenu,
    CloseSubmenu,
    Close,
}

/// <summary>The outcome of <see cref="MenuModel.HandleKey"/>.</summary>
internal readonly record struct MenuKeyResult(bool Handled, MenuAction Action)
{
    public static MenuKeyResult Ignored => new(false, MenuAction.None);
    public static MenuKeyResult Moved => new(true, MenuAction.None);
}

/// <summary>
/// The pure logic of one menu level: flattening items into rows and virtual-focus keyboard movement. Up/Down wrap and
/// skip disabled rows and dividers, Home/End jump, Enter/Space activate, Right opens a submenu, Left/Escape close it.
/// </summary>
internal sealed class MenuModel
{
    public MenuModel(IEnumerable<CdsMenuItemBase> items)
    {
        var rows = new List<MenuRowInfo>();
        Flatten(items, null, rows);
        Rows = rows;
        HasIcons = rows.Any(r => r.Kind != MenuRowKind.Divider && r.Icon is not null);
        HasSelection = rows.Any(r => r.Kind is MenuRowKind.Selectable or MenuRowKind.Radio);
    }

    /// <summary>Builds a model from prepared rows (tests, custom sources).</summary>
    internal MenuModel(IReadOnlyList<MenuRowInfo> rows)
    {
        Rows = rows;
        HasIcons = rows.Any(r => r.Kind != MenuRowKind.Divider && r.Icon is not null);
        HasSelection = rows.Any(r => r.Kind is MenuRowKind.Selectable or MenuRowKind.Radio);
    }

    public IReadOnlyList<MenuRowInfo> Rows { get; }

    /// <summary>True when any row has an icon: every row then reserves the 16px icon column.</summary>
    public bool HasIcons { get; }

    /// <summary>True when any row is selectable: every row then reserves the 16px checkmark column.</summary>
    public bool HasSelection { get; }

    /// <summary>The highlighted row (virtual focus), or -1.</summary>
    public int Highlighted { get; private set; } = -1;

    public MenuRowInfo? HighlightedRow => Highlighted >= 0 ? Rows[Highlighted] : null;

    /// <summary>True when at least one row can be highlighted.</summary>
    public bool HasFocusableRows => Rows.Any(r => r.IsFocusable);

    /// <summary>Highlights a row; ignored when the row cannot take focus. Returns whether it changed anything.</summary>
    public bool SetHighlight(int index)
    {
        if (index < 0 || index >= Rows.Count || !Rows[index].IsFocusable || index == Highlighted) return false;
        Highlighted = index;
        return true;
    }

    /// <summary>Removes the highlight.</summary>
    public void Clear() => Highlighted = -1;

    /// <summary>Highlights the first focusable row.</summary>
    public bool MoveFirst() => Land(Next(-1, 1, wrap: false));

    /// <summary>Highlights the last focusable row.</summary>
    public bool MoveLast() => Land(Next(Rows.Count, -1, wrap: false));

    /// <summary>Highlights the next focusable row, wrapping from the end to the start. With no highlight it lands on the first.</summary>
    public bool MoveNext() => Land(Next(Highlighted, 1, wrap: true));

    /// <summary>Highlights the previous focusable row, wrapping from the start to the end. With no highlight it lands on the last.</summary>
    public bool MovePrevious() => Land(Highlighted < 0 ? Next(Rows.Count, -1, wrap: false) : Next(Highlighted, -1, wrap: true));

    /// <summary>Applies a key. <paramref name="isRoot"/> is false for a submenu level.</summary>
    public MenuKeyResult HandleKey(CdsKey key, bool isRoot)
    {
        switch (key)
        {
            case CdsKey.Down:
                MoveNext();
                return MenuKeyResult.Moved;
            case CdsKey.Up:
                MovePrevious();
                return MenuKeyResult.Moved;
            case CdsKey.Home:
                MoveFirst();
                return MenuKeyResult.Moved;
            case CdsKey.End:
                MoveLast();
                return MenuKeyResult.Moved;
            case CdsKey.Enter:
            case CdsKey.Space:
                if (HighlightedRow is not { } row) return MenuKeyResult.Moved;
                return new(true, row.HasSubmenu ? MenuAction.OpenSubmenu : MenuAction.Activate);
            case CdsKey.Right:
                return HighlightedRow is { HasSubmenu: true }
                    ? new(true, MenuAction.OpenSubmenu)
                    : MenuKeyResult.Ignored;
            case CdsKey.Left:
                return isRoot ? MenuKeyResult.Ignored : new(true, MenuAction.CloseSubmenu);
            case CdsKey.Escape:
                return new(true, isRoot ? MenuAction.Close : MenuAction.CloseSubmenu);
            case CdsKey.Tab:
                // Tab leaves the menu and closes it; the key still moves focus.
                return new(false, MenuAction.Close);
            default:
                return MenuKeyResult.Ignored;
        }
    }

    bool Land(int index)
    {
        if (index < 0) return false;
        var changed = index != Highlighted;
        Highlighted = index;
        return changed;
    }

    /// <summary>Finds the next focusable row from <paramref name="from"/> (exclusive) in a direction, or -1.</summary>
    int Next(int from, int step, bool wrap)
    {
        var count = Rows.Count;
        var i = from;
        for (var n = 0; n <= count; n++)
        {
            i += step;
            if (i < 0 || i >= count)
            {
                if (!wrap) return -1;
                i = step > 0 ? 0 : count - 1;
            }
            if (Rows.Count > 0 && Rows[i].IsFocusable) return i;
        }
        return -1;
    }

    static void Flatten(IEnumerable<CdsMenuItemBase> items, string? groupLabel, List<MenuRowInfo> rows)
    {
        foreach (var item in items)
        {
            switch (item)
            {
                case CdsMenuItem mi:
                    rows.Add(new MenuRowInfo
                    {
                        Kind = MenuRowKind.Item,
                        Source = mi,
                        Text = mi.Text ?? string.Empty,
                        Icon = mi.Icon,
                        Shortcut = mi.Shortcut,
                        IsEnabled = mi.IsActionable,
                        IsDanger = mi.Kind == CdsMenuItemKind.Danger,
                        GroupLabel = groupLabel,
                        Children = new List<CdsMenuItemBase>(mi.Items),
                    });
                    break;
                case CdsMenuItemSelectable sel:
                    rows.Add(new MenuRowInfo
                    {
                        Kind = MenuRowKind.Selectable,
                        Source = sel,
                        Text = sel.Text ?? string.Empty,
                        Icon = sel.Icon,
                        Shortcut = sel.Shortcut,
                        IsEnabled = sel.IsEnabled,
                        IsSelected = sel.IsSelected,
                        GroupLabel = groupLabel,
                    });
                    break;
                case CdsMenuItemRadioGroup radio:
                    foreach (var choice in radio.Items)
                    {
                        rows.Add(new MenuRowInfo
                        {
                            Kind = MenuRowKind.Radio,
                            Source = radio,
                            Text = radio.GetLabel(choice),
                            RadioValue = choice,
                            IsSelected = Equals(choice, radio.SelectedItem),
                            GroupLabel = radio.Label,
                        });
                    }
                    break;
                case CdsMenuItemGroup group:
                    Flatten(group.Items, group.Label, rows);
                    break;
                case CdsMenuItemDivider divider:
                    rows.Add(new MenuRowInfo { Kind = MenuRowKind.Divider, Source = divider });
                    break;
            }
        }
    }
}
