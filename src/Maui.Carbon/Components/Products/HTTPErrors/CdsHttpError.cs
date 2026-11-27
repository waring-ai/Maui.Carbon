using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The full-screen HTTP error layout shared by <see cref="CdsHttpError403"/>, <see cref="CdsHttpError404"/> and
/// <see cref="CdsHttpErrorOther"/> (Carbon for IBM Products <c>HTTPErrors</c>): an error code label, the title as the
/// page's level 1 heading, a description and links, centered over an illustration that fills the area from the bottom
/// start. Carbon for IBM Products marks these deprecated in favor of <see cref="CdsFullPageError"/>; prefer that for new
/// pages. Give the control the whole page (it fills the area it is given and, without a height, takes the
/// illustration's).
/// </summary>
[ContentProperty(nameof(Links))]
public abstract partial class CdsHttpError : CdsControl
{
    /// <summary>Bindable property for <see cref="ErrorCodeLabel"/>.</summary>
    public static readonly BindableProperty ErrorCodeLabelProperty = BindableProperty.Create(
        nameof(ErrorCodeLabel), typeof(string), typeof(CdsHttpError), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsHttpError), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsHttpError), null, propertyChanged: OnLayoutChanged);

    readonly VerticalStackLayout _content = new() { Spacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.HeadingCompact02, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Heading05, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _description = new() { TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.WordWrap };
    readonly VerticalStackLayout _linkHost = new() { Spacing = 0 };
    readonly CdsItemCollection<CdsLink> _links = [];
    readonly CdsCoalescedAction _rebuildLinks;
    readonly CdsIllustrationView _art;

    private protected CdsHttpError(IllustrationDrawing drawing)
    {
        _label.Margin = new Thickness(0, 0, 0, HttpErrorModel.LabelGap);
        _title.Margin = new Thickness(0, 0, 0, HttpErrorModel.TitleGap);
        _description.Margin = new Thickness(0, 0, 0, HttpErrorModel.DescriptionGap);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);
        _content.Add(_label);
        _content.Add(_title);
        _content.Add(_description);
        _content.Add(_linkHost);
        _art = new CdsIllustrationView { Light = drawing, Alignment = IllustrationAlignment.BottomStart };
        _rebuildLinks = new CdsCoalescedAction(this, RebuildLinks);
        _links.CollectionChanged += OnLinksChanged;
        SetChrome(new HttpErrorLayout(_art, _content, drawing), fillsWidth: true);
        UpdateLayout();
    }

    /// <summary>The error code above the title ("Error 404"), in <c>heading-compact-02</c>. Required.</summary>
    public string? ErrorCodeLabel
    {
        get => (string?)GetValue(ErrorCodeLabelProperty);
        set => SetValue(ErrorCodeLabelProperty, value);
    }

    /// <summary>What happened, as the page heading in <c>heading-05</c> ("Page not found"). Required.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Why it happened and what to do next, in <c>body-compact-01</c>. Required.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Links for more context (<c>links</c>), one per line under the description: "Go to the dashboard", "Contact
    /// support". The content property: nest <see cref="CdsLink"/> elements in XAML.
    /// </summary>
    public IList<CdsLink> Links => _links;

    /// <summary>The illustration shown (for tests).</summary>
    internal IllustrationDrawing? CurrentArt => _art.Current;

    /// <summary>The title label (for tests).</summary>
    internal CdsText TitleLabel => _title;

    /// <summary>The links as placed (for tests).</summary>
    internal IReadOnlyList<IView> PlacedLinks => _linkHost.Children.ToList();

    void UpdateLayout()
    {
        _label.Text = ErrorCodeLabel;
        _label.IsVisible = !string.IsNullOrEmpty(ErrorCodeLabel);
        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.Text = Description;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        RefreshVisuals();
    }

    void OnLinksChanged(object? sender, NotifyCollectionChangedEventArgs e) => _rebuildLinks.Request();

    void RebuildLinks()
    {
        // Keep links already in place; take out the removed ones and add the rest in order.
        for (var i = _linkHost.Children.Count - 1; i >= 0; i--)
        {
            if (_linkHost.Children[i] is CdsLink link && !_links.Contains(link)) _linkHost.Children.RemoveAt(i);
        }
        for (var i = 0; i < _links.Count; i++)
        {
            var link = _links[i];
            link.Margin = new Thickness(0, 0, 0, HttpErrorModel.LinkGap);
            var at = _linkHost.Children.IndexOf(link);
            if (at == i) continue;
            if (at >= 0) _linkHost.Children.RemoveAt(at);
            _linkHost.Children.Insert(i, link);
        }
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsHttpError)b).UpdateLayout();
}

/// <summary>
/// The HTTP 403 page (Carbon for IBM Products <c>HTTPError403</c>): people lack permission to see the page. Say who can
/// grant access and link back to a page they can open. Deprecated in Carbon for IBM Products: prefer
/// <see cref="CdsFullPageError"/> with <see cref="CdsFullPageErrorKind.Forbidden"/>.
/// </summary>
public class CdsHttpError403 : CdsHttpError
{
    /// <summary>Creates the 403 page.</summary>
    public CdsHttpError403() : base(IllustrationArt.HttpError403)
    {
    }
}

/// <summary>
/// The HTTP 404 page (Carbon for IBM Products <c>HTTPError404</c>): the page does not exist. Link to the home page or a
/// search. Deprecated in Carbon for IBM Products: prefer <see cref="CdsFullPageError"/> with
/// <see cref="CdsFullPageErrorKind.NotFound"/>.
/// </summary>
public class CdsHttpError404 : CdsHttpError
{
    /// <summary>Creates the 404 page.</summary>
    public CdsHttpError404() : base(IllustrationArt.HttpError404)
    {
    }
}

/// <summary>
/// The page for any other HTTP error code (Carbon for IBM Products <c>HTTPErrorOther</c>): put the code in
/// <see cref="CdsHttpError.ErrorCodeLabel"/>. Deprecated in Carbon for IBM Products: prefer <see cref="CdsFullPageError"/>.
/// </summary>
public class CdsHttpErrorOther : CdsHttpError
{
    /// <summary>Creates the page.</summary>
    public CdsHttpErrorOther() : base(IllustrationArt.HttpErrorOther)
    {
    }
}
