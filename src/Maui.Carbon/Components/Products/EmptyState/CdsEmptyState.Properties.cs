using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsEmptyState
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsEmptyStateSize), typeof(CdsEmptyState), CdsEmptyStateSize.Lg, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Illustration"/>.</summary>
    public static readonly BindableProperty IllustrationProperty = BindableProperty.Create(
        nameof(Illustration), typeof(ImageSource), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IllustrationDescription"/>.</summary>
    public static readonly BindableProperty IllustrationDescriptionProperty = BindableProperty.Create(
        nameof(IllustrationDescription), typeof(string), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IllustrationPosition"/>.</summary>
    public static readonly BindableProperty IllustrationPositionProperty = BindableProperty.Create(
        nameof(IllustrationPosition), typeof(CdsEmptyStateIllustrationPosition), typeof(CdsEmptyState), CdsEmptyStateIllustrationPosition.Top, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IllustrationTheme"/>.</summary>
    public static readonly BindableProperty IllustrationThemeProperty = BindableProperty.Create(
        nameof(IllustrationTheme), typeof(CdsIllustrationTheme), typeof(CdsEmptyState), CdsIllustrationTheme.Auto, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="HeadingLevel"/>.</summary>
    public static readonly BindableProperty HeadingLevelProperty = BindableProperty.Create(
        nameof(HeadingLevel), typeof(int?), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ActionText"/>.</summary>
    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ActionKind"/>.</summary>
    public static readonly BindableProperty ActionKindProperty = BindableProperty.Create(
        nameof(ActionKind), typeof(CdsButtonKind), typeof(CdsEmptyState), CdsButtonKind.Tertiary, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ActionIcon"/>.</summary>
    public static readonly BindableProperty ActionIconProperty = BindableProperty.Create(
        nameof(ActionIcon), typeof(CdsIconName?), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ActionCommand"/>.</summary>
    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ActionCommandParameter"/>.</summary>
    public static readonly BindableProperty ActionCommandParameterProperty = BindableProperty.Create(
        nameof(ActionCommandParameter), typeof(object), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="LinkText"/>.</summary>
    public static readonly BindableProperty LinkTextProperty = BindableProperty.Create(
        nameof(LinkText), typeof(string), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="LinkHref"/>.</summary>
    public static readonly BindableProperty LinkHrefProperty = BindableProperty.Create(
        nameof(LinkHref), typeof(string), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="LinkIcon"/>.</summary>
    public static readonly BindableProperty LinkIconProperty = BindableProperty.Create(
        nameof(LinkIcon), typeof(CdsIconName?), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="LinkCommand"/>.</summary>
    public static readonly BindableProperty LinkCommandProperty = BindableProperty.Create(
        nameof(LinkCommand), typeof(ICommand), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="LinkCommandParameter"/>.</summary>
    public static readonly BindableProperty LinkCommandParameterProperty = BindableProperty.Create(
        nameof(LinkCommandParameter), typeof(object), typeof(CdsEmptyState), null, propertyChanged: OnLayoutChanged);

    /// <summary>The heading: what is empty or what went wrong, in a few words ("No buckets yet"). Required.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>One or two sentences under the title: why it is empty and what to do next.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>lg (default) for page and panel areas; sm for small containers such as cards and side panels.</summary>
    public CdsEmptyStateSize Size
    {
        get => (CdsEmptyStateSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>A custom illustration (<c>illustration</c>), 80 wide at lg and 64 at sm. Pass the light or dark artwork for the app theme. The presets (<see cref="CdsErrorEmptyState"/>, <see cref="CdsNoDataEmptyState"/>…) draw their own and ignore it.</summary>
    public ImageSource? Illustration
    {
        get => (ImageSource?)GetValue(IllustrationProperty);
        set => SetValue(IllustrationProperty, value);
    }

    /// <summary>Alternative text for the illustration. Carbon hides the illustration from screen readers; set this to have it read as an image.</summary>
    public string? IllustrationDescription
    {
        get => (string?)GetValue(IllustrationDescriptionProperty);
        set => SetValue(IllustrationDescriptionProperty, value);
    }

    /// <summary>Where the illustration sits relative to the text. Default <see cref="CdsEmptyStateIllustrationPosition.Top"/>; it shows only with an illustration.</summary>
    public CdsEmptyStateIllustrationPosition IllustrationPosition
    {
        get => (CdsEmptyStateIllustrationPosition)GetValue(IllustrationPositionProperty);
        set => SetValue(IllustrationPositionProperty, value);
    }

    /// <summary>Light or dark variant of a preset illustration (<c>illustrationTheme</c>). Default <see cref="CdsIllustrationTheme.Auto"/> follows the theme zone.</summary>
    public CdsIllustrationTheme IllustrationTheme
    {
        get => (CdsIllustrationTheme)GetValue(IllustrationThemeProperty);
        set => SetValue(IllustrationThemeProperty, value);
    }

    /// <summary>Forces the title's heading level (<c>headingAs</c>): 1 when the empty state is the whole page. Unset, the title is one level below its context (<see cref="CdsSection"/>), level 2 at the top of a page.</summary>
    public int? HeadingLevel
    {
        get => (int?)GetValue(HeadingLevelProperty);
        set => SetValue(HeadingLevelProperty, value);
    }

    /// <summary>The action button label; the small button shows only with text. Use a verb ("Create bucket").</summary>
    public string? ActionText
    {
        get => (string?)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    /// <summary>The action button kind: primary, secondary or tertiary (default). Use primary only when it is the main action of the view.</summary>
    public CdsButtonKind ActionKind
    {
        get => (CdsButtonKind)GetValue(ActionKindProperty);
        set => SetValue(ActionKindProperty, value);
    }

    /// <summary>An icon after the action label (<c>renderIcon</c>), such as <see cref="CdsIconName.Add"/>.</summary>
    public CdsIconName? ActionIcon
    {
        get => (CdsIconName?)GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }

    /// <summary>Run when the action button is activated, after <see cref="ActionClicked"/>.</summary>
    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <summary>The parameter passed to <see cref="ActionCommand"/>.</summary>
    public object? ActionCommandParameter
    {
        get => (object?)GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    /// <summary>The link text; the link shows only with text. Name the destination ("View documentation").</summary>
    public string? LinkText
    {
        get => (string?)GetValue(LinkTextProperty);
        set => SetValue(LinkTextProperty, value);
    }

    /// <summary>An absolute address the link opens with the system launcher (<c>link.href</c>).</summary>
    public string? LinkHref
    {
        get => (string?)GetValue(LinkHrefProperty);
        set => SetValue(LinkHrefProperty, value);
    }

    /// <summary>An icon after the link text: <see cref="CdsIconName.ArrowRight"/> for internal, <see cref="CdsIconName.Launch"/> for external destinations.</summary>
    public CdsIconName? LinkIcon
    {
        get => (CdsIconName?)GetValue(LinkIconProperty);
        set => SetValue(LinkIconProperty, value);
    }

    /// <summary>Run when the link is activated, after <see cref="LinkClicked"/> and before <see cref="LinkHref"/> opens.</summary>
    public ICommand? LinkCommand
    {
        get => (ICommand?)GetValue(LinkCommandProperty);
        set => SetValue(LinkCommandProperty, value);
    }

    /// <summary>The parameter passed to <see cref="LinkCommand"/>.</summary>
    public object? LinkCommandParameter
    {
        get => (object?)GetValue(LinkCommandParameterProperty);
        set => SetValue(LinkCommandParameterProperty, value);
    }
}
