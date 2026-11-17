namespace Maui.Carbon;

public partial class CdsExpandableTile
{
    /// <summary>Bindable property for <see cref="ExpandedContent"/>.</summary>
    public static readonly BindableProperty ExpandedContentProperty = BindableProperty.Create(
        nameof(ExpandedContent), typeof(View), typeof(CdsExpandableTile), null,
        propertyChanged: static (b, _, n) => ((CdsExpandableTile)b).OnExpandedContentChanged((View?)n));

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsExpandableTile), false, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsExpandableTile)b).OnExpandedChanged((bool)o, (bool)n));

    /// <summary>Bindable property for <see cref="CollapsedDescription"/>.</summary>
    public static readonly BindableProperty CollapsedDescriptionProperty = BindableProperty.Create(
        nameof(CollapsedDescription), typeof(string), typeof(CdsExpandableTile), "Interact to expand Tile",
        propertyChanged: static (b, _, _) => ((CdsExpandableTile)b).UpdateToggle());

    /// <summary>Bindable property for <see cref="ExpandedDescription"/>.</summary>
    public static readonly BindableProperty ExpandedDescriptionProperty = BindableProperty.Create(
        nameof(ExpandedDescription), typeof(string), typeof(CdsExpandableTile), "Interact to collapse Tile",
        propertyChanged: static (b, _, _) => ((CdsExpandableTile)b).UpdateToggle());

    /// <summary>Bindable property for <see cref="CollapsedText"/>.</summary>
    public static readonly BindableProperty CollapsedTextProperty = BindableProperty.Create(
        nameof(CollapsedText), typeof(string), typeof(CdsExpandableTile), null,
        propertyChanged: static (b, _, _) => ((CdsExpandableTile)b).UpdateToggle());

    /// <summary>Bindable property for <see cref="ExpandedText"/>.</summary>
    public static readonly BindableProperty ExpandedTextProperty = BindableProperty.Create(
        nameof(ExpandedText), typeof(string), typeof(CdsExpandableTile), null,
        propertyChanged: static (b, _, _) => ((CdsExpandableTile)b).UpdateToggle());

    /// <summary>Bindable property for <see cref="IsInteractive"/>.</summary>
    public static readonly BindableProperty IsInteractiveProperty = BindableProperty.Create(
        nameof(IsInteractive), typeof(bool?), typeof(CdsExpandableTile), null,
        propertyChanged: static (b, _, _) => ((CdsExpandableTile)b).UpdateVariant());

    /// <summary>
    /// The content shown only when the tile is expanded, below the always-visible <see cref="CdsContainer.Body"/>.
    /// Collapsed, it is hidden and skipped by Tab and screen readers.
    /// </summary>
    public View? ExpandedContent
    {
        get => (View?)GetValue(ExpandedContentProperty);
        set => SetValue(ExpandedContentProperty, value);
    }

    /// <summary>Whether the tile is expanded (two-way).</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Accessible name of the toggle while collapsed. Write it for the tile ("Show details for prod-eu-1").</summary>
    public string? CollapsedDescription
    {
        get => (string?)GetValue(CollapsedDescriptionProperty);
        set => SetValue(CollapsedDescriptionProperty, value);
    }

    /// <summary>Accessible name of the toggle while expanded ("Hide details for prod-eu-1").</summary>
    public string? ExpandedDescription
    {
        get => (string?)GetValue(ExpandedDescriptionProperty);
        set => SetValue(ExpandedDescriptionProperty, value);
    }

    /// <summary>Optional visible text beside the chevron while collapsed (<c>label-01</c>, <c>text-secondary</c>); non-interactive variant only.</summary>
    public string? CollapsedText
    {
        get => (string?)GetValue(CollapsedTextProperty);
        set => SetValue(CollapsedTextProperty, value);
    }

    /// <summary>Optional visible text beside the chevron while expanded; non-interactive variant only.</summary>
    public string? ExpandedText
    {
        get => (string?)GetValue(ExpandedTextProperty);
        set => SetValue(ExpandedTextProperty, value);
    }

    /// <summary>
    /// Forces the variant. By default (<c>null</c>) the tile detects it: when either fold contains interactive content
    /// (buttons, links, inputs) it is the interactive variant, otherwise the whole tile is one button.
    /// </summary>
    public bool? IsInteractive
    {
        get => (bool?)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }
}
