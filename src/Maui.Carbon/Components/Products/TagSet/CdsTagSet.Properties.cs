using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsTagSet
{
    /// <summary>Bindable property for <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(
        nameof(Align), typeof(CdsTagSetAlign), typeof(CdsTagSet), CdsTagSetAlign.Start, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="MaxVisible"/>.</summary>
    public static readonly BindableProperty MaxVisibleProperty = BindableProperty.Create(
        nameof(MaxVisible), typeof(int), typeof(CdsTagSet), 0, propertyChanged: OnItemsLayoutChanged);

    /// <summary>Bindable property for <see cref="IsMultiline"/>.</summary>
    public static readonly BindableProperty IsMultilineProperty = BindableProperty.Create(
        nameof(IsMultiline), typeof(bool), typeof(CdsTagSet), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsTagSize), typeof(CdsTagSet), CdsTagSize.Md, propertyChanged: OnItemsLayoutChanged);

    /// <summary>Bindable property for <see cref="OverflowPlacement"/>.</summary>
    public static readonly BindableProperty OverflowPlacementProperty = BindableProperty.Create(
        nameof(OverflowPlacement), typeof(CdsPlacement), typeof(CdsTagSet), CdsPlacement.Bottom, propertyChanged: OnPopoverChanged);

    /// <summary>Bindable property for <see cref="OverflowType"/>.</summary>
    public static readonly BindableProperty OverflowTypeProperty = BindableProperty.Create(
        nameof(OverflowType), typeof(CdsTagSetOverflowType), typeof(CdsTagSet), CdsTagSetOverflowType.Default, propertyChanged: OnPopoverChanged);

    /// <summary>Bindable property for <see cref="ShowAllTagsLabel"/>.</summary>
    public static readonly BindableProperty ShowAllTagsLabelProperty = BindableProperty.Create(
        nameof(ShowAllTagsLabel), typeof(string), typeof(CdsTagSet), "View all tags", propertyChanged: OnPopoverChanged);

    /// <summary>Bindable property for <see cref="AllTagsModalTitle"/>.</summary>
    public static readonly BindableProperty AllTagsModalTitleProperty = BindableProperty.Create(
        nameof(AllTagsModalTitle), typeof(string), typeof(CdsTagSet), "All tags", propertyChanged: OnModalChanged);

    /// <summary>Bindable property for <see cref="AllTagsModalSearchLabel"/>.</summary>
    public static readonly BindableProperty AllTagsModalSearchLabelProperty = BindableProperty.Create(
        nameof(AllTagsModalSearchLabel), typeof(string), typeof(CdsTagSet), "Search all tags", propertyChanged: OnModalChanged);

    /// <summary>Bindable property for <see cref="AllTagsModalSearchPlaceholderText"/>.</summary>
    public static readonly BindableProperty AllTagsModalSearchPlaceholderTextProperty = BindableProperty.Create(
        nameof(AllTagsModalSearchPlaceholderText), typeof(string), typeof(CdsTagSet), "Search all tags", propertyChanged: OnModalChanged);

    /// <summary>Bindable property for <see cref="AllTagsModalAriaLabel"/>.</summary>
    public static readonly BindableProperty AllTagsModalAriaLabelProperty = BindableProperty.Create(
        nameof(AllTagsModalAriaLabel), typeof(string), typeof(CdsTagSet), "List of all tags", propertyChanged: OnModalChanged);

    /// <summary>Bindable property for <see cref="AllTagsModalCloseLabel"/>.</summary>
    public static readonly BindableProperty AllTagsModalCloseLabelProperty = BindableProperty.Create(
        nameof(AllTagsModalCloseLabel), typeof(string), typeof(CdsTagSet), "Close", propertyChanged: OnModalChanged);

    /// <summary>Bindable property for <see cref="AllTagsModalSearchClearLabel"/>.</summary>
    public static readonly BindableProperty AllTagsModalSearchClearLabelProperty = BindableProperty.Create(
        nameof(AllTagsModalSearchClearLabel), typeof(string), typeof(CdsTagSet), "Clear search input", propertyChanged: OnModalChanged);

    /// <summary>Bindable property for <see cref="IsOverflowOpen"/>.</summary>
    public static readonly BindableProperty IsOverflowOpenProperty = BindableProperty.Create(
        nameof(IsOverflowOpen), typeof(bool), typeof(CdsTagSet), false, BindingMode.TwoWay, propertyChanged: OnPopoverOpenChanged);

    /// <summary>Bindable property for <see cref="IsAllTagsModalOpen"/>.</summary>
    public static readonly BindableProperty IsAllTagsModalOpenProperty = BindableProperty.Create(
        nameof(IsAllTagsModalOpen), typeof(bool), typeof(CdsTagSet), false, BindingMode.TwoWay, propertyChanged: OnModalOpenChanged);

    /// <summary>Bindable property for <see cref="OverflowCommand"/>.</summary>
    public static readonly BindableProperty OverflowCommandProperty = BindableProperty.Create(
        nameof(OverflowCommand), typeof(ICommand), typeof(CdsTagSet), null);

    /// <summary>Where the row sits in the width: start (default), center or end.</summary>
    public CdsTagSetAlign Align
    {
        get => (CdsTagSetAlign)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>The most tags shown before the rest go into the overflow (<c>maxVisible</c>). 0 (default) shows as many as fit.</summary>
    public int MaxVisible
    {
        get => (int)GetValue(MaxVisibleProperty);
        set => SetValue(MaxVisibleProperty, value);
    }

    /// <summary>Wrap the tags onto more lines instead of collapsing them (<c>multiline</c>); <see cref="MaxVisible"/> still caps them.</summary>
    public bool IsMultiline
    {
        get => (bool)GetValue(IsMultilineProperty);
        set => SetValue(IsMultilineProperty, value);
    }

    /// <summary>The size of every tag and of the overflow tag. Default md (24).</summary>
    public CdsTagSize Size
    {
        get => (CdsTagSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Where the overflow popover opens (<c>overflowAlign</c>). Default <see cref="CdsPlacement.Bottom"/>; flips when it does not fit.</summary>
    public CdsPlacement OverflowPlacement
    {
        get => (CdsPlacement)GetValue(OverflowPlacementProperty);
        set => SetValue(OverflowPlacementProperty, value);
    }

    /// <summary>How the popover lists the hidden tags: text lines (default) or tags.</summary>
    public CdsTagSetOverflowType OverflowType
    {
        get => (CdsTagSetOverflowType)GetValue(OverflowTypeProperty);
        set => SetValue(OverflowTypeProperty, value);
    }

    /// <summary>The popover link that opens the "All tags" modal, offered when more than 10 tags are hidden.</summary>
    public string ShowAllTagsLabel
    {
        get => (string)GetValue(ShowAllTagsLabelProperty);
        set => SetValue(ShowAllTagsLabelProperty, value);
    }

    /// <summary>The "All tags" modal heading.</summary>
    public string AllTagsModalTitle
    {
        get => (string)GetValue(AllTagsModalTitleProperty);
        set => SetValue(AllTagsModalTitleProperty, value);
    }

    /// <summary>The label of the modal's search field.</summary>
    public string AllTagsModalSearchLabel
    {
        get => (string)GetValue(AllTagsModalSearchLabelProperty);
        set => SetValue(AllTagsModalSearchLabelProperty, value);
    }

    /// <summary>The placeholder of the modal's search field.</summary>
    public string AllTagsModalSearchPlaceholderText
    {
        get => (string)GetValue(AllTagsModalSearchPlaceholderTextProperty);
        set => SetValue(AllTagsModalSearchPlaceholderTextProperty, value);
    }

    /// <summary>The accessible name of the modal's list of tags.</summary>
    public string AllTagsModalAriaLabel
    {
        get => (string)GetValue(AllTagsModalAriaLabelProperty);
        set => SetValue(AllTagsModalAriaLabelProperty, value);
    }

    /// <summary>The accessible name of the modal's close button.</summary>
    public string AllTagsModalCloseLabel
    {
        get => (string)GetValue(AllTagsModalCloseLabelProperty);
        set => SetValue(AllTagsModalCloseLabelProperty, value);
    }

    /// <summary>The accessible name of the search field's clear button.</summary>
    public string AllTagsModalSearchClearLabel
    {
        get => (string)GetValue(AllTagsModalSearchClearLabelProperty);
        set => SetValue(AllTagsModalSearchClearLabelProperty, value);
    }

    /// <summary>
    /// Whether the overflow popover listing the hidden tags is open (two-way). It opens from the "+N" tag and closes on
    /// Escape, a tap outside, or when a shown tag is dismissed; it stays closed while no tag is hidden.
    /// </summary>
    public bool IsOverflowOpen
    {
        get => (bool)GetValue(IsOverflowOpenProperty);
        set => SetValue(IsOverflowOpenProperty, value);
    }

    /// <summary>Whether the searchable "All tags" modal is open (two-way). "View all tags" in the popover opens it.</summary>
    public bool IsAllTagsModalOpen
    {
        get => (bool)GetValue(IsAllTagsModalOpenProperty);
        set => SetValue(IsAllTagsModalOpenProperty, value);
    }

    /// <summary>
    /// Command run when the "+N" tag is activated, with <see cref="OverflowItems"/> as its parameter. Setting it replaces
    /// the popover and modal with your own handling (<c>onOverflowClick</c>), as does setting
    /// <see cref="CdsTagSetOverflowEventArgs.Handled"/> in <see cref="OverflowClicked"/>.
    /// </summary>
    public ICommand? OverflowCommand
    {
        get => (ICommand?)GetValue(OverflowCommandProperty);
        set => SetValue(OverflowCommandProperty, value);
    }
}
