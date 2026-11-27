using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// Tells people why an area has no content and what to do next (Carbon for IBM Products <c>EmptyState</c>): an optional
/// illustration, a title, a subtitle, a small action button and a link. Use the presets for the common cases, each with
/// its light and dark artwork: <see cref="CdsErrorEmptyState"/>, <see cref="CdsNoDataEmptyState"/>,
/// <see cref="CdsNoTagsEmptyState"/>, <see cref="CdsNotFoundEmptyState"/>, <see cref="CdsNotificationsEmptyState"/> and
/// <see cref="CdsUnauthorizedEmptyState"/>; give this control your own <see cref="Illustration"/> otherwise. Keep the
/// title short, say what to do in the subtitle, and offer at most one button and one link.
/// </summary>
public partial class CdsEmptyState : CdsControl
{
    readonly IllustrationDrawing? _presetLight;
    readonly IllustrationDrawing? _presetDark;
    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly Grid _illustrationHost = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, IsVisible = false };
    readonly VerticalStackLayout _content = new() { Spacing = 0, VerticalOptions = LayoutOptions.Start };
    readonly CdsSection _section = new();
    readonly CdsHeading _title = new() { LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _subtitle = new() { Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.WordWrap, IsVisible = false };
    CdsIllustrationView? _art;
    Image? _image;
    CdsButton? _action;
    CdsLink? _link;

    /// <summary>Creates an empty state with no illustration (set <see cref="Illustration"/> for one).</summary>
    public CdsEmptyState() : this(null, null)
    {
    }

    private protected CdsEmptyState(IllustrationDrawing? presetLight, IllustrationDrawing? presetDark)
    {
        _presetLight = presetLight;
        _presetDark = presetDark;
        _title.Margin = new Thickness(0, 0, 0, CdsEmptyStateTokens.TitleGap);
        _subtitle.Margin = new Thickness(0, 0, 0, CdsEmptyStateTokens.SubtitleGap);
        _content.Add(_title);
        _content.Add(_subtitle);
        _section.Body = _content;
        _illustrationHost.Margin = new Thickness(0, 0, 0, CdsEmptyStateTokens.IllustrationGap);
        _root.Add(_illustrationHost);
        _root.Add(_section);
        SetChrome(_root, fillsWidth: true);
        UpdateLayout();
    }

    /// <summary>Raised when the action button is activated, before <see cref="ActionCommand"/>.</summary>
    public event EventHandler? ActionClicked;

    /// <summary>Raised when the link is activated, before <see cref="LinkCommand"/> and <see cref="LinkHref"/>.</summary>
    public event EventHandler? LinkClicked;

    /// <summary>True for the presets, which draw their own illustration.</summary>
    internal bool HasPresetIllustration => _presetLight is not null;

    /// <summary>The preset drawing shown now (for tests).</summary>
    internal IllustrationDrawing? CurrentPreset => _art?.Current;

    /// <summary>The action button, once it has text (for tests).</summary>
    internal CdsButton? ActionButton => _action;

    /// <summary>The link, once it has text (for tests).</summary>
    internal CdsLink? Link => _link;

    /// <summary>The title label (for tests).</summary>
    internal CdsHeading TitleLabel => _title;

    void UpdateLayout()
    {
        var size = Size;
        _title.TypeStyle = CdsEmptyStateTokens.TitleStyle(size);
        _title.Text = Title;
        _subtitle.TypeStyle = CdsEmptyStateTokens.SubtitleStyle(size);
        _subtitle.Text = Subtitle;
        _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
        _section.Level = HeadingLevel;
        UpdateIllustration(size);
        UpdateAction();
        UpdateLink();
        PlaceParts();
        RefreshVisuals();
    }

    void UpdateIllustration(CdsEmptyStateSize size)
    {
        var width = CdsEmptyStateTokens.IllustrationSize(size);
        var hasIllustration = false;
        if (_presetLight is not null)
        {
            _art ??= CreateArt();
            _art.Light = _presetLight;
            _art.Dark = _presetDark;
            _art.Theme = IllustrationTheme;
            _art.WidthRequest = width;
            _art.HeightRequest = width;
            _art.SetDescription(IllustrationDescription);
            hasIllustration = true;
        }
        else if (Illustration is { } source)
        {
            if (_image is null)
            {
                _image = new Image { Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
                _illustrationHost.Add(_image);
            }
            _image.Source = source;
            _image.WidthRequest = width;
            var named = !string.IsNullOrWhiteSpace(IllustrationDescription);
            AutomationProperties.SetIsInAccessibleTree(_image, named);
            SemanticProperties.SetDescription(_image, named ? IllustrationDescription : null);
            _image.IsVisible = true;
            hasIllustration = true;
        }
        else if (_image is not null)
        {
            _image.Source = null;
            _image.IsVisible = false;
        }
        _illustrationHost.IsVisible = hasIllustration;
    }

    CdsIllustrationView CreateArt()
    {
        var art = new CdsIllustrationView { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        _illustrationHost.Add(art);
        return art;
    }

    void UpdateAction()
    {
        var show = EmptyStateModel.ShowsAction(ActionText);
        if (!show)
        {
            if (_action is not null) _action.IsVisible = false;
            return;
        }
        if (_action is null)
        {
            _action = new CdsButton { Size = CdsSize.Sm, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsEmptyStateTokens.ActionGap) };
            _action.Clicked += OnActionClicked;
            _content.Insert(_content.IndexOf(_subtitle) + 1, _action);
        }
        _action.Text = ActionText;
        _action.Kind = CdsEmptyStateTokens.ActionKind(ActionKind);
        _action.Icon = ActionIcon;
        _action.Command = ActionCommand;
        _action.CommandParameter = ActionCommandParameter;
        _action.IsVisible = true;
    }

    void UpdateLink()
    {
        var show = EmptyStateModel.ShowsLink(LinkText);
        if (!show)
        {
            if (_link is not null) _link.IsVisible = false;
            return;
        }
        if (_link is null)
        {
            _link = new CdsLink();
            _link.Clicked += OnLinkClicked;
            _content.Add(_link);
        }
        _link.Text = LinkText;
        _link.Href = LinkHref;
        _link.Icon = LinkIcon;
        _link.Command = LinkCommand;
        _link.CommandParameter = LinkCommandParameter;
        _link.IsVisible = true;
    }

    void PlaceParts()
    {
        var placement = EmptyStateModel.Place(IllustrationPosition);
        _root.RowDefinitions.Clear();
        _root.ColumnDefinitions.Clear();
        if (placement.IsRow)
        {
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var i = 0; i < 2; i++)
                _root.ColumnDefinitions.Add(new ColumnDefinition(i == placement.IllustrationIndex ? GridLength.Auto : GridLength.Star));
            Grid.SetRow(_illustrationHost, 0);
            Grid.SetRow(_section, 0);
            Grid.SetColumn(_illustrationHost, placement.IllustrationIndex);
            Grid.SetColumn(_section, placement.ContentIndex);
            // A reversed row packs the text against the illustration at the end edge, as flex-direction: row-reverse does.
            _section.HorizontalOptions = placement.ContentIndex == 0 ? LayoutOptions.End : LayoutOptions.Fill;
        }
        else
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetColumn(_illustrationHost, 0);
            Grid.SetColumn(_section, 0);
            Grid.SetRow(_illustrationHost, placement.IllustrationIndex);
            Grid.SetRow(_section, placement.ContentIndex);
            _section.HorizontalOptions = LayoutOptions.Fill;
        }
    }

    void OnActionClicked(object? sender, EventArgs e) => ActionClicked?.Invoke(this, EventArgs.Empty);

    void OnLinkClicked(object? sender, EventArgs e) => LinkClicked?.Invoke(this, EventArgs.Empty);

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsEmptyState)b).UpdateLayout();
}
