using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The open list of a list box: a <c>layer-01</c> surface with a shadow that holds the rows in a scroll view (about five and
/// a half rows tall). It only renders; the highlighted index, the choices and the keyboard live in the owning control.
/// </summary>
internal sealed class CdsListBoxList
{
    readonly ScrollView _scroll;
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly List<CdsListBoxRow> _rows = [];
    readonly bool _inline;
    readonly bool _multi;
    int _hovered = -1;

    public CdsListBoxList(CdsSize size, bool inline, bool multi, string? accessibleName)
        : this(inline, multi, accessibleName, CdsListBoxTokens.RowHeight(size), CdsListBoxTokens.MaxListHeight(size), CdsListBoxTokens.RowHeight(size))
    {
    }

    /// <summary>Creates a list with explicit metrics (fluid list boxes: 64px rows, content in the top 40px, at most 220px).</summary>
    public CdsListBoxList(bool inline, bool multi, string? accessibleName, double rowHeight, double maxHeight, double contentHeight)
    {
        _inline = inline;
        _multi = multi;
        RowHeight = rowHeight;
        MaxHeight = maxHeight;
        ContentHeight = Math.Min(contentHeight, rowHeight);

        _scroll = new ScrollView { Content = _stack, MaximumHeightRequest = MaxHeight, VerticalScrollBarVisibility = ScrollBarVisibility.Never };
        Surface = new Border
        {
            StrokeShape = new Rectangle(),
            StrokeThickness = 0,
            Padding = 0,
            Content = _scroll,
        };
        CdsTokens.SetColor(Surface, VisualElement.BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(Surface, CdsColorKeys.Shadow, offsetY: 2, radius: 6);
        CdsSemantics.SetGroup(Surface, accessibleName, CdsSemanticGroupKind.List);
    }

    /// <summary>The list surface, ready to hand to the overlay service.</summary>
    public Border Surface { get; }

    /// <summary>Row height (equal to the field height).</summary>
    public double RowHeight { get; }

    /// <summary>The list's maximum height (5.5 rows; 220 in fluid list boxes).</summary>
    public double MaxHeight { get; }

    /// <summary>Height of the band at the top of each row that holds its content (the row height, or 40 in 64px fluid rows).</summary>
    public double ContentHeight { get; }

    /// <summary>The number of rows.</summary>
    public int Count => _rows.Count;

    /// <summary>Raised when a row is chosen by tap or click (row index).</summary>
    public event EventHandler<int>? RowChosen;

    /// <summary>Raised when the pointer enters a row (row index) or leaves every row (-1).</summary>
    public event EventHandler<int>? RowHovered;

    /// <summary>Rebuilds the rows. <paramref name="content"/> may supply a template view for a row.</summary>
    public void Build(IReadOnlyList<ListBoxEntry> rows, Func<int, bool> isChosen, Func<int, ListBoxEntry, View?>? content)
    {
        _stack.Clear();
        _rows.Clear();
        _hovered = -1;
        for (var i = 0; i < rows.Count; i++)
        {
            var index = i;
            var row = new CdsListBoxRow(rows[i], RowHeight, _inline, _multi, isChosen(i), content?.Invoke(i, rows[i]), ContentHeight);
            row.Chosen += (_, _) => RowChosen?.Invoke(this, index);
            row.HoverChanged += (_, entered) =>
            {
                _hovered = entered ? index : _hovered == index ? -1 : _hovered;
                RowHovered?.Invoke(this, _hovered);
            };
            _rows.Add(row);
            _stack.Add(row);
        }
    }

    /// <summary>Re-applies highlight, ring, selection marks and separators.</summary>
    public void Refresh(int highlighted, bool keyboard, Func<int, bool> isChosen)
    {
        for (var i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i];
            row.IsChosen = isChosen(i);
            row.IsHighlighted = i == highlighted;
            row.ShowFocusRing = keyboard;
            row.SeparatorVisible = ListBoxLogic.SeparatorVisible(i, highlighted, _hovered, i > 0 && isChosen(i - 1), _multi);
        }
    }

    /// <summary>Scrolls the highlighted row into view with the least movement.</summary>
    public void ScrollTo(int index)
    {
        if (index < 0 || index >= _rows.Count) return;
        var viewport = _scroll.Height > 0 ? _scroll.Height : Math.Min(_rows.Count * RowHeight, MaxHeight);
        var target = ListBoxLogic.ScrollTarget(_scroll.ScrollY, viewport, index * RowHeight, RowHeight);
        if (Math.Abs(target - _scroll.ScrollY) > 0.5) _ = _scroll.ScrollToAsync(0, target, false);
    }

    /// <summary>The scroll offset (for tests and previews).</summary>
    public double ScrollOffset => _scroll.ScrollY;
}
