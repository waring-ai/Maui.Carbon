using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>Pure width math of <see cref="CdsEqualWidthRow"/>: equal cells sized to the widest content.</summary>
internal static class EqualWidthModel
{
    /// <summary>The cell width wanted: the widest natural width, at most <paramref name="maxCell"/>.</summary>
    public static double DesiredCell(IReadOnlyList<double> naturalWidths, double maxCell)
    {
        var widest = 0d;
        foreach (var w in naturalWidths) widest = Math.Max(widest, w);
        return Math.Min(widest, maxCell);
    }

    /// <summary>The cell width when measured: <see cref="DesiredCell"/>, shrunk so that all cells fit a finite constraint.</summary>
    public static double MeasuredCell(IReadOnlyList<double> naturalWidths, int count, double widthConstraint, double maxCell)
    {
        var cell = DesiredCell(naturalWidths, maxCell);
        if (count > 0 && double.IsFinite(widthConstraint)) cell = Math.Min(cell, Math.Max(0, widthConstraint) / count);
        return cell;
    }

    /// <summary>The cell width when arranged: the share of the given width, at most <paramref name="maxCell"/>.</summary>
    public static double ArrangedCell(double width, int count, double maxCell) =>
        count <= 0 ? 0 : Math.Min(width / count, maxCell);
}

/// <summary>
/// A horizontal row of equal-width cells. Each cell is as wide as the widest child needs (so no label truncates while
/// there is room), at most <see cref="MaxCellWidth"/>; when arranged wider, the cells share the width equally. Children
/// with <see cref="SeparatorForProperty"/> set are not cells: they are overlaid on the leading edge of that cell.
/// </summary>
internal sealed class CdsEqualWidthRow : Layout
{
    /// <summary>Marks a child as the separator of the cell with that index.</summary>
    public static readonly BindableProperty SeparatorForProperty = BindableProperty.CreateAttached(
        "SeparatorFor", typeof(int), typeof(CdsEqualWidthRow), -1);

    public static readonly BindableProperty MaxCellWidthProperty = BindableProperty.Create(
        nameof(MaxCellWidth), typeof(double), typeof(CdsEqualWidthRow), double.PositiveInfinity,
        propertyChanged: static (b, _, _) => ((CdsEqualWidthRow)b).InvalidateMeasure());

    public static readonly BindableProperty FixedCellWidthProperty = BindableProperty.Create(
        nameof(FixedCellWidth), typeof(double), typeof(CdsEqualWidthRow), -1d,
        propertyChanged: static (b, _, _) => ((CdsEqualWidthRow)b).InvalidateMeasure());

    public static int GetSeparatorFor(BindableObject view) => (int)view.GetValue(SeparatorForProperty);

    public static void SetSeparatorFor(BindableObject view, int value) => view.SetValue(SeparatorForProperty, value);

    public double MaxCellWidth
    {
        get => (double)GetValue(MaxCellWidthProperty);
        set => SetValue(MaxCellWidthProperty, value);
    }

    /// <summary>When positive, every cell has exactly this width (icon-only segments).</summary>
    public double FixedCellWidth
    {
        get => (double)GetValue(FixedCellWidthProperty);
        set => SetValue(FixedCellWidthProperty, value);
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    sealed class Manager(CdsEqualWidthRow row) : LayoutManager(row)
    {
        static bool IsSeparator(IView v) => GetSeparatorFor((BindableObject)v) >= 0;

        List<IView> Cells() => row.Where(c => c.Visibility != Visibility.Collapsed && !IsSeparator(c)).ToList();

        public override Size Measure(double widthConstraint, double heightConstraint)
        {
            var cells = Cells();
            double cell;
            if (row.FixedCellWidth > 0)
            {
                cell = row.FixedCellWidth;
            }
            else
            {
                var naturals = cells.Select(c => c.Measure(double.PositiveInfinity, heightConstraint).Width).ToArray();
                cell = EqualWidthModel.MeasuredCell(naturals, cells.Count, widthConstraint, row.MaxCellWidth);
            }
            var height = 0d;
            foreach (var c in cells) height = Math.Max(height, c.Measure(cell, heightConstraint).Height);
            foreach (var s in row.Where(c => c.Visibility != Visibility.Collapsed && IsSeparator(c)))
                s.Measure(cell, height);
            return new Size(cell * cells.Count, height);
        }

        public override Size ArrangeChildren(Rect bounds)
        {
            var cells = Cells();
            var cell = row.FixedCellWidth > 0
                ? row.FixedCellWidth
                : EqualWidthModel.ArrangedCell(bounds.Width, cells.Count, row.MaxCellWidth);
            for (var i = 0; i < cells.Count; i++)
                cells[i].Arrange(new Rect(bounds.X + i * cell, bounds.Y, cell, bounds.Height));
            foreach (var s in row.Where(c => c.Visibility != Visibility.Collapsed && IsSeparator(c)))
            {
                var index = GetSeparatorFor((BindableObject)s);
                s.Arrange(new Rect(bounds.X + index * cell - (index > 0 ? 1 : 0), bounds.Y, 1, bounds.Height));
            }
            return new Size(cell * cells.Count, bounds.Height);
        }
    }
}
