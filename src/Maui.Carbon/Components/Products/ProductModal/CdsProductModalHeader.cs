namespace Maui.Carbon;

/// <summary>How a product modal lays out its header (the ibm-products SCSS overrides of the Carbon modal header).</summary>
internal enum CdsProductModalHeaderStyle
{
    /// <summary>Carbon's header: label over a heading-03 title, 48 at the end for the close button (remove, export, API key).</summary>
    Standard,

    /// <summary>The import modal: no close button, the header keeps 20% - 16 at the end.</summary>
    Import,

    /// <summary>The create modal: no close button, a subtitle under the title and a 1px layer-accent-01 rule under the header.</summary>
    Create,

    /// <summary>The about modal: the logo above a heading-04 product name, 20% at the end, no gap below.</summary>
    About,
}

/// <summary>
/// The header of a product modal. It is a <see cref="CdsModalHeader"/> (so the composed modal wires its close button,
/// names the dialog by <see cref="CdsModalHeader.Title"/> and disables the close button while a submit loads) with its own
/// layout: an optional logo, the label, the title, an optional subtitle and rule, and the 48px close button at the top end.
/// </summary>
internal sealed class CdsProductModalHeader : CdsModalHeader
{
    readonly CdsProductModalHeaderStyle _style;
    readonly CdsModalCloseButton _close;
    readonly ContentView _logo = new() { IsVisible = false, HorizontalOptions = LayoutOptions.Start };
    readonly CdsText _labelText = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, IsVisible = false, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _titleText = new() { TypeStyle = CdsTypeStyle.Heading03, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _subtitleText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Secondary, IsVisible = false, LineBreakMode = LineBreakMode.WordWrap };
    readonly BoxView _rule = new() { HeightRequest = 1, IsVisible = false, Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0) };

    public CdsProductModalHeader(CdsProductModalHeaderStyle style)
    {
        _style = style;
        // Take the base header's close button (its events and name stay wired) into this layout.
        var baseRoot = (Grid)Content;
        _close = baseRoot.Children.OfType<CdsModalCloseButton>().Single();
        baseRoot.Remove(_close);

        SemanticProperties.SetHeadingLevel(_titleText, SemanticHeadingLevel.Level1);
        var texts = new VerticalStackLayout { Spacing = 0 };
        texts.Add(_labelText);
        texts.Add(_titleText);
        texts.Add(_subtitleText);
        var inset = new CdsEndInset(texts, w => ProductModalModel.HeaderEndInset(style, w));

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_logo);
        stack.Add(inset);
        stack.Add(_rule);

        var root = new Grid { MinimumHeightRequest = CdsModalSizing.CloseButtonSize };
        root.Add(stack);
        root.Add(_close);
        Content = root;

        switch (style)
        {
            case CdsProductModalHeaderStyle.About:
                // The logo sits 16 from the edges with 32 below; the product name has no gap to the body.
                Padding = 0;
                _logo.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing07);
                inset.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
                _labelText.TypeStyle = CdsTypeStyle.Heading04;
                _labelText.Tone = CdsTextTone.Primary;
                stack.Margin = new Thickness(0, 0, 0, 0);
                break;
            case CdsProductModalHeaderStyle.Create:
                // 8 below the subtitle, then the rule; the title keeps 4 to the subtitle.
                Padding = 0;
                inset.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0, 0);
                _titleText.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing02);
                _subtitleText.Margin = new Thickness(0, 0, 0, 0);
                _rule.IsVisible = true;
                CdsTokens.SetColor(_rule, BoxView.ColorProperty, CdsProductModalTokens.CreateHeaderRule);
                ShowsCloseButton = false;
                break;
            case CdsProductModalHeaderStyle.Import:
                inset.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0, 0);
                _labelText.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing02);
                ShowsCloseButton = false;
                break;
            default:
                inset.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0, 0);
                _labelText.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing02);
                break;
        }
        Sync();
    }

    /// <summary>The layout this header was built with.</summary>
    public CdsProductModalHeaderStyle HeaderStyle => _style;

    /// <summary>Whether the 48px close button is shown (create, import and API key modals hide it).</summary>
    public bool ShowsCloseButton
    {
        get => _close.IsVisible;
        set => _close.IsVisible = value;
    }

    /// <summary>The product logo above the title (about modal).</summary>
    public View? Logo
    {
        get => _logo.Content;
        set
        {
            _logo.Content = value;
            _logo.IsVisible = value is not null;
        }
    }

    /// <summary>The secondary line under the title (create modal).</summary>
    public string? Subtitle
    {
        get => _subtitleText.Text;
        set
        {
            _subtitleText.Text = value;
            var shown = !string.IsNullOrEmpty(value);
            _subtitleText.IsVisible = shown;
            // The title keeps its 4 below only when a subtitle follows; the header always ends 8 above the rule.
            _titleText.Margin = new Thickness(0, 0, 0, shown ? CdsSpacing.Spacing02 : 0);
        }
    }

    /// <summary>The shown title text (for tests).</summary>
    public CdsText TitleText => _titleText;

    /// <summary>The shown label text (for tests).</summary>
    public CdsText LabelText => _labelText;

    /// <summary>The close button's hit target (for tests).</summary>
    public CdsModalCloseButton CloseButton => _close;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == TitleProperty.PropertyName || propertyName == LabelProperty.PropertyName) Sync();
    }

    void Sync()
    {
        // The about modal shows the product name in the label slot (heading-04) and has no separate title.
        if (_style == CdsProductModalHeaderStyle.About)
        {
            _labelText.Text = Title;
            _labelText.IsVisible = !string.IsNullOrEmpty(Title);
            _titleText.IsVisible = false;
            return;
        }
        _labelText.Text = Label;
        _labelText.IsVisible = !string.IsNullOrEmpty(Label);
        _titleText.Text = Title;
        _titleText.IsVisible = !string.IsNullOrEmpty(Title);
    }
}
