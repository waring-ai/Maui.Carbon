namespace Maui.Carbon;

/// <summary>
/// A convenience wrapper for a grid child: it carries the span and offset values of <see cref="CdsGrid"/> as its own
/// properties (<c>Span</c>, <c>SpanSm</c>…<c>SpanMax</c>, <c>Offset</c>, <c>OffsetSm</c>…<c>OffsetMax</c>) and hosts one
/// view. Put content inside columns, not on the grid; any view can be a grid child through the attached properties
/// <c>CdsGrid.Span</c> and friends. The column box is the area inside the gutters.
/// </summary>
[ContentProperty(nameof(Content))]
public class CdsColumn : ContentView
{
    /// <summary>Span at every breakpoint without an override (shares <see cref="CdsGrid.SpanProperty"/>).</summary>
    public static readonly BindableProperty SpanProperty = CdsGrid.SpanProperty;
    /// <summary>Span from sm up (shares <see cref="CdsGrid.SpanSmProperty"/>).</summary>
    public static readonly BindableProperty SpanSmProperty = CdsGrid.SpanSmProperty;
    /// <summary>Span from md up (shares <see cref="CdsGrid.SpanMdProperty"/>).</summary>
    public static readonly BindableProperty SpanMdProperty = CdsGrid.SpanMdProperty;
    /// <summary>Span from lg up (shares <see cref="CdsGrid.SpanLgProperty"/>).</summary>
    public static readonly BindableProperty SpanLgProperty = CdsGrid.SpanLgProperty;
    /// <summary>Span from xlg up (shares <see cref="CdsGrid.SpanXlgProperty"/>).</summary>
    public static readonly BindableProperty SpanXlgProperty = CdsGrid.SpanXlgProperty;
    /// <summary>Span from max up (shares <see cref="CdsGrid.SpanMaxProperty"/>).</summary>
    public static readonly BindableProperty SpanMaxProperty = CdsGrid.SpanMaxProperty;
    /// <summary>Offset at every breakpoint without an override (shares <see cref="CdsGrid.OffsetProperty"/>).</summary>
    public static readonly BindableProperty OffsetProperty = CdsGrid.OffsetProperty;
    /// <summary>Offset from sm up (shares <see cref="CdsGrid.OffsetSmProperty"/>).</summary>
    public static readonly BindableProperty OffsetSmProperty = CdsGrid.OffsetSmProperty;
    /// <summary>Offset from md up (shares <see cref="CdsGrid.OffsetMdProperty"/>).</summary>
    public static readonly BindableProperty OffsetMdProperty = CdsGrid.OffsetMdProperty;
    /// <summary>Offset from lg up (shares <see cref="CdsGrid.OffsetLgProperty"/>).</summary>
    public static readonly BindableProperty OffsetLgProperty = CdsGrid.OffsetLgProperty;
    /// <summary>Offset from xlg up (shares <see cref="CdsGrid.OffsetXlgProperty"/>).</summary>
    public static readonly BindableProperty OffsetXlgProperty = CdsGrid.OffsetXlgProperty;
    /// <summary>Offset from max up (shares <see cref="CdsGrid.OffsetMaxProperty"/>).</summary>
    public static readonly BindableProperty OffsetMaxProperty = CdsGrid.OffsetMaxProperty;

    /// <summary>Columns spanned at every breakpoint without an override (null: the full row, 0: hidden).</summary>
    public int? Span { get => CdsGrid.GetSpan(this); set => CdsGrid.SetSpan(this, value); }
    /// <summary>Columns spanned from the sm breakpoint (4 columns) up.</summary>
    public int? SpanSm { get => CdsGrid.GetSpanSm(this); set => CdsGrid.SetSpanSm(this, value); }
    /// <summary>Columns spanned from the md breakpoint (8 columns) up.</summary>
    public int? SpanMd { get => CdsGrid.GetSpanMd(this); set => CdsGrid.SetSpanMd(this, value); }
    /// <summary>Columns spanned from the lg breakpoint (16 columns) up.</summary>
    public int? SpanLg { get => CdsGrid.GetSpanLg(this); set => CdsGrid.SetSpanLg(this, value); }
    /// <summary>Columns spanned from the xlg breakpoint up.</summary>
    public int? SpanXlg { get => CdsGrid.GetSpanXlg(this); set => CdsGrid.SetSpanXlg(this, value); }
    /// <summary>Columns spanned from the max breakpoint up.</summary>
    public int? SpanMax { get => CdsGrid.GetSpanMax(this); set => CdsGrid.SetSpanMax(this, value); }
    /// <summary>The column this one starts at, counted from the start of its row, at every breakpoint without an override.</summary>
    public int? Offset { get => CdsGrid.GetOffset(this); set => CdsGrid.SetOffset(this, value); }
    /// <summary>Offset from the sm breakpoint up.</summary>
    public int? OffsetSm { get => CdsGrid.GetOffsetSm(this); set => CdsGrid.SetOffsetSm(this, value); }
    /// <summary>Offset from the md breakpoint up.</summary>
    public int? OffsetMd { get => CdsGrid.GetOffsetMd(this); set => CdsGrid.SetOffsetMd(this, value); }
    /// <summary>Offset from the lg breakpoint up.</summary>
    public int? OffsetLg { get => CdsGrid.GetOffsetLg(this); set => CdsGrid.SetOffsetLg(this, value); }
    /// <summary>Offset from the xlg breakpoint up.</summary>
    public int? OffsetXlg { get => CdsGrid.GetOffsetXlg(this); set => CdsGrid.SetOffsetXlg(this, value); }
    /// <summary>Offset from the max breakpoint up.</summary>
    public int? OffsetMax { get => CdsGrid.GetOffsetMax(this); set => CdsGrid.SetOffsetMax(this, value); }
}
