using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsSearch
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSearch), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsSearch)b).OnTextPropertyChanged((string?)n));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSearch), null, propertyChanged: OnSearchChanged);

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsSearch), "Search", propertyChanged: OnSearchChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsSearch), CdsSize.Md, propertyChanged: OnSearchChanged);

    /// <summary>Bindable property for <see cref="CloseButtonLabel"/>.</summary>
    public static readonly BindableProperty CloseButtonLabelProperty = BindableProperty.Create(
        nameof(CloseButtonLabel), typeof(string), typeof(CdsSearch), "Clear search input", propertyChanged: OnSearchChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName), typeof(CdsSearch), CdsIconName.Search, propertyChanged: OnSearchChanged);

    /// <summary>Bindable property for <see cref="IsExpandable"/>.</summary>
    public static readonly BindableProperty IsExpandableProperty = BindableProperty.Create(
        nameof(IsExpandable), typeof(bool), typeof(CdsSearch), false, propertyChanged: OnSearchChanged);

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsSearch), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsSearch)b).OnExpandedChanged((bool)n));

    /// <summary>Bindable property for <see cref="SearchCommand"/>.</summary>
    public static readonly BindableProperty SearchCommandProperty = BindableProperty.Create(
        nameof(SearchCommand), typeof(ICommand), typeof(CdsSearch), null);

    /// <summary>The query (two-way).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The input's accessible name ("Search clusters"). Required; it is never shown.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>A hint at what can be found ("Search by name or region"). Default "Search".</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Field height: sm 32, md 40 (default), lg 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Accessible name of the clear button. Default "Clear search input".</summary>
    public string? CloseButtonLabel
    {
        get => (string?)GetValue(CloseButtonLabelProperty);
        set => SetValue(CloseButtonLabelProperty, value);
    }

    /// <summary>The magnifier icon. Default <see cref="CdsIconName.Search"/>.</summary>
    public CdsIconName Icon
    {
        get => (CdsIconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Collapses to the magnifier alone until activated (toolbars where space is tight).</summary>
    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, value);
    }

    /// <summary>Whether an expandable search is expanded (two-way).</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Command run with the query when the user presses Return.</summary>
    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    static void OnSearchChanged(BindableObject b, object? o, object? n) => ((CdsSearch)b).UpdateLayout();
}
