using System.ComponentModel;
using System.Globalization;

namespace Maui.Carbon;

/// <summary>Tokens and measurements of <see cref="CdsPagination"/> (from <c>packages/styles/scss/components/pagination</c>).</summary>
public static class CdsPaginationTokens
{
    /// <summary>The bar fill: <c>layer-01</c>.</summary>
    public static string Background => CdsColorKeys.Layer01;

    /// <summary>The top rule and the dividers: <c>border-subtle-01</c>.</summary>
    public static string Rule => CdsColorKeys.BorderSubtle01;

    /// <summary>Hover fill of selects and arrows: <c>layer-hover-01</c>.</summary>
    public static string Hover => CdsColorKeys.LayerHover01;

    /// <summary>The "1–10 of 103 items" text: <c>text-secondary</c>.</summary>
    public static string RangeText => CdsColorKeys.TextSecondary;

    /// <summary>Labels ("Items per page:", "of 11 pages"): <c>text-primary</c>, <c>text-disabled</c> when disabled.</summary>
    public static string Label(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>The bar height: sm 32, md 40 (default), lg 48. Other sizes clamp to the nearest.</summary>
    public static double Height(CdsSize size) => Coerce(size).Height();

    /// <summary>Clamps a size to sm, md or lg.</summary>
    public static CdsSize Coerce(CdsSize size) => size <= CdsSize.Sm ? CdsSize.Sm : size >= CdsSize.Lg ? CdsSize.Lg : CdsSize.Md;

    /// <summary>The width below which only the item range and the arrows remain: 42rem = 672.</summary>
    public const double CompactWidth = 672;

    /// <summary>Whether a bar of this width collapses to the item range and the arrows.</summary>
    public static bool IsCompact(double width) => width > 0 && width < CompactWidth;

    /// <summary>Whether the 1px top rule shows: not when the bar sits directly under a data table.</summary>
    public static bool ShowsTopRule(bool attachedToTable) => !attachedToTable;
}

/// <summary>Converts "10, 20, 50" (XAML) to a list of page sizes.</summary>
public sealed class CdsPageSizesConverter : TypeConverter
{
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);

    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string text) return null;
        var sizes = new List<int>();
        foreach (var part in text.Split([',', ' ', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n <= 0)
                throw new FormatException($"'{part}' is not a page size.");
            sizes.Add(n);
        }
        return sizes;
    }
}

/// <summary>Event data of <see cref="CdsPagination.PageChanged"/> (<c>onChange({ page, pageSize })</c>).</summary>
public sealed class CdsPaginationChangedEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsPaginationChangedEventArgs(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>The new page (1-based).</summary>
    public int Page { get; }

    /// <summary>The page size (items per page).</summary>
    public int PageSize { get; }
}
