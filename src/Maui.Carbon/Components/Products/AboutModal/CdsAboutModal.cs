namespace Maui.Carbon;

/// <summary>
/// Tells people about the product (Carbon for IBM Products <c>AboutModal</c>): its logo and name, the version, a few
/// links, legal text and the copyright, with optional <see cref="AdditionalInfo"/> (technology logos, notices) in a
/// footer. Open it from the product's help or user menu. It has only the close button; without additional info a tap
/// outside closes it too.
/// </summary>
public partial class CdsAboutModal : CdsProductModal
{
    readonly CdsProductModalHeader _header = new(CdsProductModalHeaderStyle.About);
    readonly CdsModalFooter _footer = new();
    readonly CdsText _version = new() { TypeStyle = CdsTypeStyle.BodyCompact02, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.WordWrap };
    readonly HorizontalStackLayout _links = new() { Spacing = 0, IsVisible = false, HorizontalOptions = LayoutOptions.Start };
    readonly CdsText _legal = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.WordWrap, IsVisible = false };
    readonly CdsText _copyright = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.WordWrap, IsVisible = false };
    readonly ContentView _infoPad = new() { Padding = new Thickness(CdsSpacing.Spacing05) };
    readonly CdsLayer _infoLayer = new();
    readonly CdsItemCollection<CdsLink> _linkItems = [];
    readonly CdsCoalescedAction _rebuildLinks;

    /// <summary>Creates a closed about modal.</summary>
    public CdsAboutModal() : base(CdsModalSize.Md, preventCloseOnClickOutside: false)
    {
        _rebuildLinks = new CdsCoalescedAction(this, RebuildLinks);
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_version);
        stack.Add(_links);
        stack.Add(_legal);
        stack.Add(_copyright);
        // __body: body-compact-02, padding 0 16 (20% at the end), 24 below, at least 80 high.
        var inset = new CdsEndInset(stack, ProductModalModel.TwentyPercent)
        {
            Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, CdsSpacing.Spacing06),
        };
        var body = new CdsModalBody { HasForm = true, IsFullWidth = true, Body = inset, MinimumHeightRequest = CdsSpacing.Spacing10 };

        // __footer: padding 16 on layer-02, height follows the content.
        CdsTokens.SetColor(_infoPad, BackgroundColorProperty, CdsProductModalTokens.AboutFooterFill);
        _infoLayer.Body = _infoPad;
        _linkItems.CollectionChanged += (_, _) => _rebuildLinks.Request();
        SetParts(_header, body, _footer);
        Sync();
    }

    /// <summary>
    /// Links to more about the product (<c>links</c>): a few <see cref="CdsLink"/>s in a row under the version, split by
    /// 1px <c>border-strong-01</c> dividers. Keep it to two or three.
    /// </summary>
    public IList<CdsLink> Links => _linkItems;

    private protected override VisualElement? DefaultInitialFocus => _header.CloseTarget;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SemanticProperties.DescriptionProperty.PropertyName) Sync();
    }

    void RebuildLinks()
    {
        _links.Clear();
        var first = true;
        foreach (var link in _linkItems)
        {
            if (!first)
            {
                // a + a: 12, a 1px border-strong-01 rule, 12.
                var rule = new BoxView { WidthRequest = 1, Margin = new Thickness(CdsSpacing.Spacing04, 0) };
                CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsProductModalTokens.AboutLinkDivider);
                _links.Add(rule);
            }
            link.Size = CdsLinkSize.Md;
            link.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03);
            _links.Add(link);
            first = false;
        }
        Sync();
    }

    void Sync()
    {
        _header.Title = Title;
        _header.Logo = Logo;
        _header.IconDescription = CloseIconDescription;
        var name = SemanticProperties.GetDescription(this);
        SetAccessibleName(string.IsNullOrEmpty(name) ? Title : name);

        _version.Text = Version;
        _version.IsVisible = !string.IsNullOrEmpty(Version);
        var hasLinks = _linkItems.Count > 0;
        var hasLegal = !string.IsNullOrEmpty(LegalText);
        var (linksTop, legalTop, copyrightTop) = AboutModalModel.Spacing(hasLinks, hasLegal);
        _links.IsVisible = hasLinks;
        _links.Margin = new Thickness(0, linksTop, 0, 0);
        _legal.Text = LegalText;
        _legal.IsVisible = hasLegal;
        _legal.Margin = new Thickness(0, legalTop, 0, 0);
        _copyright.Text = CopyrightText;
        _copyright.IsVisible = !string.IsNullOrEmpty(CopyrightText);
        _copyright.Margin = new Thickness(0, copyrightTop, 0, 0);

        _infoPad.Content = AdditionalInfo;
        _footer.Body = AdditionalInfo is null ? null : _infoLayer;
    }
}
