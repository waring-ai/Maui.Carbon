using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// The side-by-side row of a fluid <see cref="CdsButtonSet"/>. It is as wide as the space it is offered and gives each
/// button its Carbon fluid width (<see cref="ButtonSetModel.FluidWidths"/>: a quarter of the set between 11 and 14.5rem,
/// ghost buttons sharing the rest, 50/50 or full width in narrow sets), right-aligned. The widths are decided while
/// measuring and arranging, so they always match the width the row is given. Children marked with
/// <see cref="CdsEqualWidthRow.SeparatorForProperty"/> are hairlines on the leading edge of that button.
/// </summary>
internal sealed class CdsFluidButtonRow : Layout
{
    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    static bool IsSeparator(IView view) => CdsEqualWidthRow.GetSeparatorFor((BindableObject)view) >= 0;

    static bool IsGhost(IView view) => view is CdsButton { Kind: CdsButtonKind.Ghost or CdsButtonKind.DangerGhost };

    /// <summary>The buttons' widths and the right-aligning offset for a row <paramref name="width"/> wide.</summary>
    internal double[] Widths(double width, out double offset, out List<IView> cells)
    {
        cells = this.Where(c => c.Visibility != Visibility.Collapsed && !IsSeparator(c)).ToList();
        return ButtonSetModel.FluidWidths(width, cells.Select(IsGhost).ToArray(), out offset);
    }

    sealed class Manager(CdsFluidButtonRow row) : LayoutManager(row)
    {
        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            double width;
            double[] widths;
            List<IView> cells;
            if (double.IsFinite(widthConstraint))
            {
                width = Math.Max(0, widthConstraint);
                widths = row.Widths(width, out _, out cells);
            }
            else
            {
                // Unbounded (a horizontal stack): each button at its fluid minimum, or its label if wider.
                cells = row.Where(c => c.Visibility != Visibility.Collapsed && !IsSeparator(c)).ToList();
                widths = cells.Select(c => Math.Max(ButtonSetModel.FluidMinButtonWidth, c.Measure(double.PositiveInfinity, heightConstraint).Width)).ToArray();
                width = widths.Sum();
            }
            var height = 0d;
            for (var i = 0; i < cells.Count; i++) height = Math.Max(height, cells[i].Measure(widths[i], heightConstraint).Height);
            foreach (var s in row.Where(c => c.Visibility != Visibility.Collapsed && IsSeparator(c))) s.Measure(1, height);
            return new Size(width, height);
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            var widths = row.Widths(bounds.Width, out var offset, out var cells);
            var starts = new double[cells.Count];
            var x = bounds.X + offset;
            for (var i = 0; i < cells.Count; i++)
            {
                starts[i] = x;
                cells[i].Arrange(new Rect(x, bounds.Y, widths[i], bounds.Height));
                x += widths[i];
            }
            foreach (var s in row.Where(c => c.Visibility != Visibility.Collapsed && IsSeparator(c)))
            {
                var index = CdsEqualWidthRow.GetSeparatorFor((BindableObject)s);
                if (index <= 0 || index >= cells.Count) continue;
                s.Arrange(new Rect(starts[index] - 1, bounds.Y, 1, bounds.Height));
            }
            return bounds.Size;
        }
    }
}
