namespace Maui.Carbon;

/// <summary>
/// One tappable row of an open list box. Keyboard focus never moves into the list (the field keeps it): the row is
/// "highlighted" by the list's virtual focus or by the pointer. Rows match the field height, draw a 1px
/// <c>border-subtle-01</c> rule above them, fill with the hover and selected layers, and show a check mark (single select)
/// or a checkbox glyph (multi-select).
/// </summary>
internal sealed class CdsListBoxRow : CdsControl
{
    readonly Grid _root = new();
    readonly BoxView _separator = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
    readonly CdsText? _text;
    readonly CdsIcon? _check;
    readonly CdsCheckGlyph? _box;
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    readonly bool _multi;
    bool _selected;
    bool _highlighted;
    bool _showRing;
    bool _separatorVisible;

    public CdsListBoxRow(ListBoxEntry entry, double height, bool inline, bool multi, bool isSelected, View? content)
    {
        Entry = entry;
        _multi = multi;
        _selected = isSelected;
        HorizontalOptions = LayoutOptions.Fill;

        var inset = CdsListBoxTokens.RowInset(inline);
        _root.HeightRequest = height;
        _root.Padding = new Thickness(inset, 0);
        _root.ColumnSpacing = multi ? CdsSpacing.Spacing04 : CdsSpacing.Spacing03;

        var column = 0;
        if (multi)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(16));
            _box = new CdsCheckGlyph { VerticalOptions = LayoutOptions.Center };
            _root.Add(_box, column++);
        }
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        if (content is not null)
        {
            content.VerticalOptions = LayoutOptions.Center;
            content.InputTransparent = true;
            _root.Add(content, column++);
        }
        else
        {
            _text = new CdsText(entry.Text) { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };
            _root.Add(_text, column++);
        }
        if (!multi)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(16));
            _check = new CdsIcon(CdsIconName.Checkmark, 16) { IsVisible = isSelected };
            _root.Add(_check, column);
        }

        _separator.Margin = new Thickness(inset, 0);
        foreach (var child in _root.Children.OfType<View>()) child.InputTransparent = true;
        _ring.InputTransparent = true;

        _hit = CreateHitTarget(CdsAccessibilityRole.Option);
        _hit.IsTabStop = false;
        _hit.IsSelected = isSelected;
        SemanticProperties.SetDescription(_hit, entry.Text);
        IsEnabled = !entry.IsDisabled;

        var cell = new Grid();
        cell.Add(_root);
        cell.Add(_separator);
        cell.Add(_ring);
        cell.Add(_hit);
        SetChrome(cell);
    }

    /// <summary>The entry this row shows.</summary>
    public ListBoxEntry Entry { get; }

    /// <summary>Raised when the pointer enters (<c>true</c>) or leaves (<c>false</c>) an enabled row.</summary>
    public event EventHandler<bool>? HoverChanged;

    /// <summary>Raised when the row is chosen by tap or click.</summary>
    public event EventHandler? Chosen;

    /// <summary>The row is the chosen item (single) or checked (multi).</summary>
    public bool IsChosen
    {
        get => _selected;
        set
        {
            if (_selected == value) return;
            _selected = value;
            _hit.IsSelected = value;
            if (_check is not null) _check.IsVisible = value;
            RefreshVisuals();
        }
    }

    /// <summary>The list's virtual focus is on this row.</summary>
    public bool IsHighlighted
    {
        get => _highlighted;
        set
        {
            if (_highlighted == value) return;
            _highlighted = value;
            RefreshVisuals();
        }
    }

    /// <summary>Show the inset focus ring (the highlight came from the keyboard).</summary>
    public bool ShowFocusRing
    {
        get => _showRing;
        set
        {
            if (_showRing == value) return;
            _showRing = value;
            RefreshVisuals();
        }
    }

    /// <summary>Whether the rule above the row is drawn.</summary>
    public bool SeparatorVisible
    {
        get => _separatorVisible;
        set
        {
            if (_separatorVisible == value) return;
            _separatorVisible = value;
            RefreshVisuals();
        }
    }

    /// <summary>The pointer is over the row.</summary>
    public bool IsPointerOver => Is(CdsInteractionState.Hover) && Entry is { IsDisabled: false };

    /// <inheritdoc/>
    protected override void OnActivated() => Chosen?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (((previous ^ State) & CdsInteractionState.Hover) != 0 && !Entry.IsDisabled)
            HoverChanged?.Invoke(this, Is(CdsInteractionState.Hover));
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Entry.IsDisabled;
        var hover = Is(CdsInteractionState.Hover) && !disabled;
        var v = CdsListBoxTokens.ResolveRow(_selected, _highlighted, hover, Is(CdsInteractionState.Pressed), disabled, _multi);
        SetColor(_root, BackgroundColorProperty, v.Background);
        if (_text is not null) SetColor(_text, Label.TextColorProperty, v.Text);
        SetColor(_separator, BoxView.ColorProperty, _separatorVisible ? CdsColorKeys.BorderSubtle01 : null);
        if (_check is not null) SetColor(_check, CdsIcon.ColorProperty, disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary);
        if (_box is not null)
        {
            var state = disabled ? CdsInteractionState.Disabled : CdsInteractionState.None;
            var c = CdsCheckboxTokens.Resolve(state, _selected);
            SetColor(_box, CdsCheckGlyph.BorderColorProperty, c.Border);
            SetColor(_box, CdsCheckGlyph.FillColorProperty, c.Fill);
            SetColor(_box, CdsCheckGlyph.MarkColorProperty, c.Mark);
            _box.Mark = _selected ? CdsCheckMark.Check : CdsCheckMark.None;
        }
        _ring.IsActive = !disabled && _highlighted && _showRing;
    }
}
