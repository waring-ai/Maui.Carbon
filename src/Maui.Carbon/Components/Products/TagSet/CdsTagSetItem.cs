using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// One tag of a <see cref="CdsTagSet"/>: its label, color and icon, whether it can be dismissed, and extra text the
/// "All tags" search matches (<c>data-search</c>).
/// </summary>
public class CdsTagSetItem : Element
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTagSetItem), null);

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsTagType), typeof(CdsTagSetItem), CdsTagType.Gray);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsTagSetItem), null);

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsTagSetItem), true);

    /// <summary>Bindable property for <see cref="IsDismissible"/>.</summary>
    public static readonly BindableProperty IsDismissibleProperty = BindableProperty.Create(
        nameof(IsDismissible), typeof(bool), typeof(CdsTagSetItem), false);

    /// <summary>Bindable property for <see cref="DismissLabel"/>.</summary>
    public static readonly BindableProperty DismissLabelProperty = BindableProperty.Create(
        nameof(DismissLabel), typeof(string), typeof(CdsTagSetItem), "Dismiss");

    /// <summary>Bindable property for <see cref="SearchText"/>.</summary>
    public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(
        nameof(SearchText), typeof(string), typeof(CdsTagSetItem), null);

    /// <summary>Bindable property for <see cref="DismissCommand"/>.</summary>
    public static readonly BindableProperty DismissCommandProperty = BindableProperty.Create(
        nameof(DismissCommand), typeof(ICommand), typeof(CdsTagSetItem), null);

    /// <summary>Creates an empty gray tag.</summary>
    public CdsTagSetItem()
    {
    }

    /// <summary>Creates a tag.</summary>
    public CdsTagSetItem(string text, CdsTagType type = CdsTagType.Gray)
    {
        Text = text;
        Type = type;
    }

    /// <summary>Raised when the shown tag's close button is activated (<see cref="IsDismissible"/>). Remove the item here.</summary>
    public event EventHandler? Dismissed;

    /// <summary>The label (<c>label</c>): a few words, sentence case unless it is a value.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The tag color. Default <see cref="CdsTagType.Gray"/>.</summary>
    public CdsTagType Type
    {
        get => (CdsTagType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>A decorative 16 icon before the label (not drawn at sm).</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether the tag is enabled (<c>disabled</c> when false).</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>Shows the tag as a dismissible tag with a close button (<c>onClose</c>); handle <see cref="Dismissed"/>.</summary>
    public bool IsDismissible
    {
        get => (bool)GetValue(IsDismissibleProperty);
        set => SetValue(IsDismissibleProperty, value);
    }

    /// <summary>The close button's label, said before the tag text. Default "Dismiss".</summary>
    public string DismissLabel
    {
        get => (string)GetValue(DismissLabelProperty);
        set => SetValue(DismissLabelProperty, value);
    }

    /// <summary>Extra text the "All tags" search matches besides <see cref="Text"/> (<c>data-search</c>), such as synonyms.</summary>
    public string? SearchText
    {
        get => (string?)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>Command run when the tag is dismissed, with the item as its parameter.</summary>
    public ICommand? DismissCommand
    {
        get => (ICommand?)GetValue(DismissCommandProperty);
        set => SetValue(DismissCommandProperty, value);
    }

    internal void RaiseDismissed()
    {
        Dismissed?.Invoke(this, EventArgs.Empty);
        if (DismissCommand is { } command && command.CanExecute(this)) command.Execute(this);
    }
}

/// <summary>Arguments of <see cref="CdsTagSet.OverflowClicked"/>.</summary>
public class CdsTagSetOverflowEventArgs(IReadOnlyList<CdsTagSetItem> overflowItems) : EventArgs
{
    /// <summary>The hidden tags, in order.</summary>
    public IReadOnlyList<CdsTagSetItem> OverflowItems { get; } = overflowItems;

    /// <summary>Set to true to replace the default popover with your own handling (<c>onOverflowClick</c>).</summary>
    public bool Handled { get; set; }
}
