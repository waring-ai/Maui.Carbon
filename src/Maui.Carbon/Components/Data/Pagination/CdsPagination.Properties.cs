using System.ComponentModel;

namespace Maui.Carbon;

public partial class CdsPagination
{
    /// <summary>Bindable property for <see cref="TotalItems"/>.</summary>
    public static readonly BindableProperty TotalItemsProperty = BindableProperty.Create(
        nameof(TotalItems), typeof(int), typeof(CdsPagination), 0,
        coerceValue: static (_, v) => Math.Max(0, (int)v), propertyChanged: OnPagingChanged);

    /// <summary>Bindable property for <see cref="PageSize"/>.</summary>
    public static readonly BindableProperty PageSizeProperty = BindableProperty.Create(
        nameof(PageSize), typeof(int), typeof(CdsPagination), 10, BindingMode.TwoWay, propertyChanged: OnPagingChanged);

    /// <summary>Bindable property for <see cref="PageSizes"/>.</summary>
    public static readonly BindableProperty PageSizesProperty = BindableProperty.Create(
        nameof(PageSizes), typeof(IEnumerable<int>), typeof(CdsPagination), null, propertyChanged: OnPagingChanged);

    /// <summary>Bindable property for <see cref="Page"/>.</summary>
    public static readonly BindableProperty PageProperty = BindableProperty.Create(
        nameof(Page), typeof(int), typeof(CdsPagination), 1, BindingMode.TwoWay, propertyChanged: OnPagingChanged);

    /// <summary>Bindable property for <see cref="IsPageInputDisabled"/>.</summary>
    public static readonly BindableProperty IsPageInputDisabledProperty = BindableProperty.Create(
        nameof(IsPageInputDisabled), typeof(bool), typeof(CdsPagination), false, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="IsPageSizeInputDisabled"/>.</summary>
    public static readonly BindableProperty IsPageSizeInputDisabledProperty = BindableProperty.Create(
        nameof(IsPageSizeInputDisabled), typeof(bool), typeof(CdsPagination), false, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="IsPagesUnknown"/>.</summary>
    public static readonly BindableProperty IsPagesUnknownProperty = BindableProperty.Create(
        nameof(IsPagesUnknown), typeof(bool), typeof(CdsPagination), false, propertyChanged: OnPagingChanged);

    /// <summary>Bindable property for <see cref="IsLastPage"/>.</summary>
    public static readonly BindableProperty IsLastPageProperty = BindableProperty.Create(
        nameof(IsLastPage), typeof(bool), typeof(CdsPagination), false, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsPagination), CdsSize.Md, propertyChanged: OnPagingChanged);

    /// <summary>Bindable property for <see cref="ItemsPerPageText"/>.</summary>
    public static readonly BindableProperty ItemsPerPageTextProperty = BindableProperty.Create(
        nameof(ItemsPerPageText), typeof(string), typeof(CdsPagination), "Items per page:", propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="BackwardText"/>.</summary>
    public static readonly BindableProperty BackwardTextProperty = BindableProperty.Create(
        nameof(BackwardText), typeof(string), typeof(CdsPagination), "Previous page", propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="ForwardText"/>.</summary>
    public static readonly BindableProperty ForwardTextProperty = BindableProperty.Create(
        nameof(ForwardText), typeof(string), typeof(CdsPagination), "Next page", propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="ItemRangeText"/>.</summary>
    public static readonly BindableProperty ItemRangeTextProperty = BindableProperty.Create(
        nameof(ItemRangeText), typeof(Func<int, int, int, string>), typeof(CdsPagination), null, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="ItemText"/>.</summary>
    public static readonly BindableProperty ItemTextProperty = BindableProperty.Create(
        nameof(ItemText), typeof(Func<int, int, string>), typeof(CdsPagination), null, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="PageRangeText"/>.</summary>
    public static readonly BindableProperty PageRangeTextProperty = BindableProperty.Create(
        nameof(PageRangeText), typeof(Func<int, int, string>), typeof(CdsPagination), null, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="PageText"/>.</summary>
    public static readonly BindableProperty PageTextProperty = BindableProperty.Create(
        nameof(PageText), typeof(Func<int, string>), typeof(CdsPagination), null, propertyChanged: OnVisualsChanged);

    /// <summary>Bindable property for <see cref="IsAttachedToTable"/>.</summary>
    public static readonly BindableProperty IsAttachedToTableProperty = BindableProperty.Create(
        nameof(IsAttachedToTable), typeof(bool?), typeof(CdsPagination), null, propertyChanged: OnVisualsChanged);

    /// <summary>The total number of items in the list being paged. Ignored for the count when <see cref="IsPagesUnknown"/>.</summary>
    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    /// <summary>The items per page (two-way). It follows <see cref="PageSizes"/>: a size that is not listed becomes the first size. When the user picks a size the page returns to 1.</summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>The choices of the items-per-page select (default 10, 20, 30, 40, 50). In XAML: <c>PageSizes="10, 20, 50"</c>.</summary>
    [TypeConverter(typeof(CdsPageSizesConverter))]
    public IEnumerable<int>? PageSizes
    {
        get => (IEnumerable<int>?)GetValue(PageSizesProperty);
        set => SetValue(PageSizesProperty, value);
    }

    /// <summary>The current page, 1-based (two-way). It is clamped to the page count.</summary>
    public int Page
    {
        get => (int)GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    /// <summary>Locks the page select (the page can still change with the arrows).</summary>
    public bool IsPageInputDisabled
    {
        get => (bool)GetValue(IsPageInputDisabledProperty);
        set => SetValue(IsPageInputDisabledProperty, value);
    }

    /// <summary>Locks the items-per-page select.</summary>
    public bool IsPageSizeInputDisabled
    {
        get => (bool)GetValue(IsPageSizeInputDisabledProperty);
        set => SetValue(IsPageSizeInputDisabledProperty, value);
    }

    /// <summary>The total is not known: shows "page N" instead of the page select and keeps next enabled (see <see cref="IsLastPage"/>).</summary>
    public bool IsPagesUnknown
    {
        get => (bool)GetValue(IsPagesUnknownProperty);
        set => SetValue(IsPagesUnknownProperty, value);
    }

    /// <summary>With <see cref="IsPagesUnknown"/>, disables next because there is no more data.</summary>
    public bool IsLastPage
    {
        get => (bool)GetValue(IsLastPageProperty);
        set => SetValue(IsLastPageProperty, value);
    }

    /// <summary>Bar height: sm 32, md 40 (default), lg 48. Use the same size as the table above it (md under md and lg tables, sm under xs and sm).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The label of the page size select. Default "Items per page:".</summary>
    public string? ItemsPerPageText
    {
        get => (string?)GetValue(ItemsPerPageTextProperty);
        set => SetValue(ItemsPerPageTextProperty, value);
    }

    /// <summary>The accessible name and tooltip of the previous button. Default "Previous page".</summary>
    public string? BackwardText
    {
        get => (string?)GetValue(BackwardTextProperty);
        set => SetValue(BackwardTextProperty, value);
    }

    /// <summary>The accessible name and tooltip of the next button. Default "Next page".</summary>
    public string? ForwardText
    {
        get => (string?)GetValue(ForwardTextProperty);
        set => SetValue(ForwardTextProperty, value);
    }

    /// <summary>Builds the range text from (first, last, total); default "1–10 of 103 items". Name the items ("buckets", "events").</summary>
    public Func<int, int, int, string>? ItemRangeText
    {
        get => (Func<int, int, int, string>?)GetValue(ItemRangeTextProperty);
        set => SetValue(ItemRangeTextProperty, value);
    }

    /// <summary>Builds the range text when the total is unknown, from (first, last); default "1–10 items".</summary>
    public Func<int, int, string>? ItemText
    {
        get => (Func<int, int, string>?)GetValue(ItemTextProperty);
        set => SetValue(ItemTextProperty, value);
    }

    /// <summary>Builds the text after the page select from (current, page count); default "of 11 pages".</summary>
    public Func<int, int, string>? PageRangeText
    {
        get => (Func<int, int, string>?)GetValue(PageRangeTextProperty);
        set => SetValue(PageRangeTextProperty, value);
    }

    /// <summary>Builds the text shown when the page count is unknown, from the page; default "page 3".</summary>
    public Func<int, string>? PageText
    {
        get => (Func<int, string>?)GetValue(PageTextProperty);
        set => SetValue(PageTextProperty, value);
    }

    /// <summary>
    /// Whether the bar sits directly under a <see cref="CdsDataTable"/> and so drops its top rule. <c>null</c> (default)
    /// detects it: the previous sibling in the same layout is a data table.
    /// </summary>
    public bool? IsAttachedToTable
    {
        get => (bool?)GetValue(IsAttachedToTableProperty);
        set => SetValue(IsAttachedToTableProperty, value);
    }

    static void OnPagingChanged(BindableObject b, object? o, object? n) => ((CdsPagination)b).OnPaging();

    static void OnVisualsChanged(BindableObject b, object? o, object? n) => ((CdsPagination)b).UpdateAll();
}
