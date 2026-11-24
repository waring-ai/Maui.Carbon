using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>Where the parts of an actionable notification go (pure, tested).</summary>
internal static class ActionableLayoutModel
{
    /// <summary>Space between the status icon and the text (Carbon: the icon's 1rem end margin).</summary>
    public const double IconGap = 16;

    /// <summary>Space between the text and the action on a single row (Carbon: the details' 0.8125rem end margin).</summary>
    public const double ActionGap = 13;

    /// <summary>The width the text gets inside <paramref name="innerWidth"/> (padding already removed).</summary>
    public static double TextWidth(double innerWidth, double iconWidth, double actionWidth, bool hasAction, bool singleRow)
    {
        if (!double.IsFinite(innerWidth)) return double.PositiveInfinity;
        var rest = innerWidth - iconWidth - IconGap - (singleRow && hasAction ? ActionGap + actionWidth : 0);
        return Math.Max(0, rest);
    }

    /// <summary>
    /// The height of the content (padding excluded): one row as high as its tallest part, or the icon and text row with the
    /// action under it.
    /// </summary>
    public static double Height(Size icon, Size text, Size action, bool hasAction, bool singleRow)
    {
        var row = Math.Max(icon.Height, text.Height);
        if (!hasAction) return row;
        return singleRow ? Math.Max(row, action.Height) : row + action.Height;
    }

    /// <summary>
    /// The frames of the icon, the text and the action inside <paramref name="inner"/> (padding already removed). On a
    /// single row the action sits at the end, before the close button's slot (which the padding reserves), so it never
    /// overlaps the close button or leaves the box. Stacked, the action takes a row of its own under the text.
    /// </summary>
    public static (Rect Icon, Rect Text, Rect Action) Arrange(Rect inner, Size icon, Size text, Size action, bool hasAction, bool singleRow)
    {
        var textX = inner.X + icon.Width + IconGap;
        var textWidth = TextWidth(inner.Width, icon.Width, action.Width, hasAction, singleRow);
        var iconRect = new Rect(inner.X, inner.Y, icon.Width, icon.Height);
        if (singleRow)
        {
            var height = Height(icon, text, action, hasAction, true);
            var actionWidth = hasAction ? Math.Min(action.Width, Math.Max(0, inner.Width - icon.Width - IconGap)) : 0;
            return (iconRect,
                new Rect(textX, inner.Y, textWidth, height),
                new Rect(inner.Right - actionWidth, inner.Y, actionWidth, hasAction ? action.Height : 0));
        }
        var row = Math.Max(icon.Height, text.Height);
        return (iconRect,
            new Rect(textX, inner.Y, textWidth, text.Height),
            new Rect(inner.X, inner.Y + row, inner.Width, hasAction ? action.Height : 0));
    }
}

/// <summary>
/// Lays out an actionable notification's status icon, text and action. It decides between one row and the stacked layout
/// while it measures, from <see cref="SingleRow"/>, and arranges with the same decision. A layout switched from a size
/// callback after arranging (as a Grid whose rows were changed in <c>SizeChanged</c>) kept the measure of the old layout,
/// which put the action over the close button and past the edge of the box and left an empty band under the text.
/// </summary>
internal sealed class ActionableNotificationLayout : Layout
{
    readonly View _icon;
    readonly View _text;
    readonly View _action;
    Thickness _singleRowPadding;
    Thickness _stackedPadding;
    Thickness _singleRowActionInsets;
    Thickness _stackedActionInsets;
    bool _measuredSingleRow;

    public ActionableNotificationLayout(View icon, View text, View action)
    {
        _icon = icon;
        _text = text;
        _action = action;
        Add(icon);
        Add(text);
        Add(action);
    }

    /// <summary>Decides, from the width constraint of a measure pass, whether the action shares the row with the text.</summary>
    public Func<double, bool> SingleRow { get; set; } = static _ => false;

    /// <summary>The single-row decision of the last measure pass.</summary>
    public bool IsSingleRow => _measuredSingleRow;

    /// <summary>Sets the padding and the action's insets of both layouts, then measures again.</summary>
    public void Configure(Thickness singleRowPadding, Thickness stackedPadding, Thickness singleRowActionInsets, Thickness stackedActionInsets)
    {
        _singleRowPadding = singleRowPadding;
        _stackedPadding = stackedPadding;
        _singleRowActionInsets = singleRowActionInsets;
        _stackedActionInsets = stackedActionInsets;
        InvalidateMeasure();
    }

    /// <summary>Measures again (the single-row decision may have changed).</summary>
    public void Invalidate() => InvalidateMeasure();

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(ActionableNotificationLayout layout) : LayoutManager(layout)
    {
        static Size MeasureChild(View view, double width) =>
            view.IsVisible ? ((IView)view).Measure(width, double.PositiveInfinity) : Size.Zero;

        static Size Grow(Size size, Thickness insets) =>
            new(size.Width + insets.HorizontalThickness, size.Height + insets.VerticalThickness);

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var single = layout.SingleRow(widthConstraint);
            layout._measuredSingleRow = single;
            var padding = single ? layout._singleRowPadding : layout._stackedPadding;
            var insets = single ? layout._singleRowActionInsets : layout._stackedActionInsets;
            var inner = double.IsFinite(widthConstraint) ? Math.Max(0, widthConstraint - padding.HorizontalThickness) : double.PositiveInfinity;

            var hasAction = layout._action.IsVisible;
            var icon = MeasureChild(layout._icon, double.PositiveInfinity);
            var action = hasAction ? Grow(MeasureChild(layout._action, double.PositiveInfinity), insets) : Size.Zero;
            var textWidth = ActionableLayoutModel.TextWidth(inner, icon.Width, action.Width, hasAction, single);
            var text = MeasureChild(layout._text, textWidth);

            var height = ActionableLayoutModel.Height(icon, text, action, hasAction, single) + padding.VerticalThickness;
            var width = double.IsFinite(widthConstraint)
                ? widthConstraint
                : padding.HorizontalThickness + Math.Max(
                    icon.Width + ActionableLayoutModel.IconGap + text.Width + (single && hasAction ? ActionableLayoutModel.ActionGap + action.Width : 0),
                    single ? 0 : action.Width);
            return new Size(width, height);
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            var single = layout._measuredSingleRow;
            var padding = single ? layout._singleRowPadding : layout._stackedPadding;
            var insets = single ? layout._singleRowActionInsets : layout._stackedActionInsets;
            var inner = new Rect(
                bounds.X + padding.Left, bounds.Y + padding.Top,
                Math.Max(0, bounds.Width - padding.HorizontalThickness), Math.Max(0, bounds.Height - padding.VerticalThickness));

            var hasAction = layout._action.IsVisible;
            var icon = layout._icon.IsVisible ? ((IView)layout._icon).DesiredSize : Size.Zero;
            var text = layout._text.IsVisible ? ((IView)layout._text).DesiredSize : Size.Zero;
            var action = hasAction ? Grow(((IView)layout._action).DesiredSize, insets) : Size.Zero;
            var (iconRect, textRect, actionRect) = ActionableLayoutModel.Arrange(inner, icon, text, action, hasAction, single);

            ((IView)layout._icon).Arrange(iconRect);
            ((IView)layout._text).Arrange(textRect);
            if (hasAction)
            {
                ((IView)layout._action).Arrange(new Rect(
                    actionRect.X + insets.Left, actionRect.Y + insets.Top,
                    Math.Max(0, actionRect.Width - insets.HorizontalThickness), Math.Max(0, actionRect.Height - insets.VerticalThickness)));
            }
            return bounds.Size;
        }
    }
}
