using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsCodeSnippet
{
    /// <summary>Bindable property for <see cref="Code"/>.</summary>
    public static readonly BindableProperty CodeProperty = BindableProperty.Create(
        nameof(Code), typeof(string), typeof(CdsCodeSnippet), null,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).Rebuild());

    /// <summary>Bindable property for <see cref="CopyText"/>.</summary>
    public static readonly BindableProperty CopyTextProperty = BindableProperty.Create(
        nameof(CopyText), typeof(string), typeof(CdsCodeSnippet), null);

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsCodeSnippetType), typeof(CdsCodeSnippet), CdsCodeSnippetType.Single,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).Rebuild());

    /// <summary>Bindable property for <see cref="HideCopyButton"/>.</summary>
    public static readonly BindableProperty HideCopyButtonProperty = BindableProperty.Create(
        nameof(HideCopyButton), typeof(bool), typeof(CdsCodeSnippet), false,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsWrapText"/>.</summary>
    public static readonly BindableProperty IsWrapTextProperty = BindableProperty.Create(
        nameof(IsWrapText), typeof(bool), typeof(CdsCodeSnippet), false,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).Rebuild());

    /// <summary>Bindable property for <see cref="Feedback"/>.</summary>
    public static readonly BindableProperty FeedbackProperty = BindableProperty.Create(
        nameof(Feedback), typeof(string), typeof(CdsCodeSnippet), "Copied!");

    /// <summary>Bindable property for <see cref="FeedbackTimeout"/>.</summary>
    public static readonly BindableProperty FeedbackTimeoutProperty = BindableProperty.Create(
        nameof(FeedbackTimeout), typeof(int), typeof(CdsCodeSnippet), 2000);

    /// <summary>Bindable property for <see cref="CopyButtonDescription"/>.</summary>
    public static readonly BindableProperty CopyButtonDescriptionProperty = BindableProperty.Create(
        nameof(CopyButtonDescription), typeof(string), typeof(CdsCodeSnippet), "Copy to clipboard",
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).ApplySemantics());

    /// <summary>Bindable property for <see cref="FeedbackPlacement"/>.</summary>
    public static readonly BindableProperty FeedbackPlacementProperty = BindableProperty.Create(
        nameof(FeedbackPlacement), typeof(CdsPlacement), typeof(CdsCodeSnippet), CdsPlacement.BottomEnd);

    /// <summary>Bindable property for <see cref="MinCollapsedNumberOfRows"/>.</summary>
    public static readonly BindableProperty MinCollapsedNumberOfRowsProperty = BindableProperty.Create(
        nameof(MinCollapsedNumberOfRows), typeof(int), typeof(CdsCodeSnippet), 3,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="MaxCollapsedNumberOfRows"/>.</summary>
    public static readonly BindableProperty MaxCollapsedNumberOfRowsProperty = BindableProperty.Create(
        nameof(MaxCollapsedNumberOfRows), typeof(int), typeof(CdsCodeSnippet), 15,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="MinExpandedNumberOfRows"/>.</summary>
    public static readonly BindableProperty MinExpandedNumberOfRowsProperty = BindableProperty.Create(
        nameof(MinExpandedNumberOfRows), typeof(int), typeof(CdsCodeSnippet), 16,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="MaxExpandedNumberOfRows"/>.</summary>
    public static readonly BindableProperty MaxExpandedNumberOfRowsProperty = BindableProperty.Create(
        nameof(MaxExpandedNumberOfRows), typeof(int), typeof(CdsCodeSnippet), 0,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="ShowMoreText"/>.</summary>
    public static readonly BindableProperty ShowMoreTextProperty = BindableProperty.Create(
        nameof(ShowMoreText), typeof(string), typeof(CdsCodeSnippet), "Show more",
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="ShowLessText"/>.</summary>
    public static readonly BindableProperty ShowLessTextProperty = BindableProperty.Create(
        nameof(ShowLessText), typeof(string), typeof(CdsCodeSnippet), "Show less",
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsCodeSnippet), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsCodeSnippet)b).UpdateMulti());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsCodeSnippet), null,
        propertyChanged: static (b, _, n) => ((CdsCodeSnippet)b).SetCommand((ICommand?)n, ((CdsCodeSnippet)b).CommandParameter));

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsCodeSnippet), null,
        propertyChanged: static (b, _, n) => ((CdsCodeSnippet)b).SetCommand(((CdsCodeSnippet)b).Command, n));

    /// <summary>The code (a string; newlines are kept in <see cref="CdsCodeSnippetType.Multi"/>). Copied code must run as is: no prompts like <c>$</c>.</summary>
    public string? Code
    {
        get => (string?)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>Replaces what is copied (otherwise the code itself).</summary>
    public string? CopyText
    {
        get => (string?)GetValue(CopyTextProperty);
        set => SetValue(CopyTextProperty, value);
    }

    /// <summary>Single (default), inline or multi.</summary>
    public CdsCodeSnippetType Type
    {
        get => (CdsCodeSnippetType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>Hide the copy button (and, for inline, the click-to-copy behavior): for output that should not be copied.</summary>
    public bool HideCopyButton
    {
        get => (bool)GetValue(HideCopyButtonProperty);
        set => SetValue(HideCopyButtonProperty, value);
    }

    /// <summary>Wrap long lines instead of scrolling sideways (multi only).</summary>
    public bool IsWrapText
    {
        get => (bool)GetValue(IsWrapTextProperty);
        set => SetValue(IsWrapTextProperty, value);
    }

    /// <summary>The feedback shown after a copy. Default <c>Copied!</c>; keep it short and action-based.</summary>
    public string Feedback
    {
        get => (string)GetValue(FeedbackProperty);
        set => SetValue(FeedbackProperty, value);
    }

    /// <summary>Milliseconds the feedback stays visible. Default 2000.</summary>
    public int FeedbackTimeout
    {
        get => (int)GetValue(FeedbackTimeoutProperty);
        set => SetValue(FeedbackTimeoutProperty, value);
    }

    /// <summary>Name and tooltip of the copy button. Default <c>Copy to clipboard</c>.</summary>
    public string CopyButtonDescription
    {
        get => (string)GetValue(CopyButtonDescriptionProperty);
        set => SetValue(CopyButtonDescriptionProperty, value);
    }

    /// <summary>Where the tooltip and feedback sit relative to the copy button. Default <see cref="CdsPlacement.BottomEnd"/>.</summary>
    public CdsPlacement FeedbackPlacement
    {
        get => (CdsPlacement)GetValue(FeedbackPlacementProperty);
        set => SetValue(FeedbackPlacementProperty, value);
    }

    /// <summary>Fewest rows a collapsed multi-line snippet shows. Default 3.</summary>
    public int MinCollapsedNumberOfRows
    {
        get => (int)GetValue(MinCollapsedNumberOfRowsProperty);
        set => SetValue(MinCollapsedNumberOfRowsProperty, value);
    }

    /// <summary>Most rows a collapsed snippet shows; longer code gets Show more. Default 15.</summary>
    public int MaxCollapsedNumberOfRows
    {
        get => (int)GetValue(MaxCollapsedNumberOfRowsProperty);
        set => SetValue(MaxCollapsedNumberOfRowsProperty, value);
    }

    /// <summary>Fewest rows an expanded snippet shows. Default 16.</summary>
    public int MinExpandedNumberOfRows
    {
        get => (int)GetValue(MinExpandedNumberOfRowsProperty);
        set => SetValue(MinExpandedNumberOfRowsProperty, value);
    }

    /// <summary>Most rows an expanded snippet shows; 0 (default) is unlimited.</summary>
    public int MaxExpandedNumberOfRows
    {
        get => (int)GetValue(MaxExpandedNumberOfRowsProperty);
        set => SetValue(MaxExpandedNumberOfRowsProperty, value);
    }

    /// <summary>Label of the expand button. Default <c>Show more</c>.</summary>
    public string ShowMoreText
    {
        get => (string)GetValue(ShowMoreTextProperty);
        set => SetValue(ShowMoreTextProperty, value);
    }

    /// <summary>Label of the collapse button. Default <c>Show less</c>.</summary>
    public string ShowLessText
    {
        get => (string)GetValue(ShowLessTextProperty);
        set => SetValue(ShowLessTextProperty, value);
    }

    /// <summary>Whether a multi-line snippet is expanded (two-way).</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Runs after a copy.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}
