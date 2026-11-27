namespace Maui.Carbon;

/// <summary>
/// A whole-page error for a page people cannot open (Carbon for IBM Products <c>FullPageError</c>): an arrow and the
/// error label in <c>text-error</c>, the title as the page's level 1 heading, a description, your content in
/// <see cref="CdsContainer.Body"/> (links back to safety such as "Go to the dashboard") and a line drawing in the
/// theme's <c>background</c> and <c>border-inverse</c> colors. Pick the drawing with <see cref="Kind"/>. The text takes 6
/// of 16 columns at lg and up and the drawing the rest; below md the drawing goes under the text. Use it as the page
/// content (inside the UI shell), not in a dialog.
/// </summary>
public partial class CdsFullPageError : CdsContainer
{
    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsFullPageErrorKind), typeof(CdsFullPageError), CdsFullPageErrorKind.Custom, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsFullPageError), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsFullPageError), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsFullPageError), null, propertyChanged: OnLayoutChanged);

    readonly VerticalStackLayout _text = new() { Spacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.FluidHeading05, Tone = CdsTextTone.Error, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.FluidHeading05, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _description = new() { TypeStyle = CdsTypeStyle.Body02, LineBreakMode = LineBreakMode.WordWrap };
    readonly ContentView _body = new();
    readonly CdsIllustrationView _art = new() { SizesToWidth = true, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Start };

    /// <summary>Creates a full-page error with the generic drawing.</summary>
    public CdsFullPageError()
    {
        // The label and title form one h1; the arrow is hidden from screen readers, so the title reads both.
        AutomationProperties.SetIsInAccessibleTree(_label, false);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);
        _title.Margin = new Thickness(0, 0, 0, FullPageErrorModel.TitleGap);
        _description.Margin = new Thickness(0, 0, 0, FullPageErrorModel.DescriptionGap);
        _text.Add(_label);
        _text.Add(_title);
        _text.Add(_description);
        _text.Add(_body);
        _art.ThemeFillKey = CdsColorKeys.Background;
        _art.ThemeStrokeKey = CdsColorKeys.BorderInverse;
        var artHost = new ContentView { Padding = FullPageErrorModel.ArtPadding, Content = _art };
        SetChrome(new FullPageErrorLayout(_text, artHost), fillsWidth: true);
        UpdateLayout();
    }

    /// <summary>The drawing: <see cref="CdsFullPageErrorKind.Forbidden"/> (403), <see cref="CdsFullPageErrorKind.NotFound"/> (404) or <see cref="CdsFullPageErrorKind.Custom"/> (default) for any other error.</summary>
    public CdsFullPageErrorKind Kind
    {
        get => (CdsFullPageErrorKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The error in a few words above the title, in <c>text-error</c> after an arrow ("Error 403"). Required.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>What happened, as the page heading ("Access denied"). Required.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Why it happened and what people can do ("Contact your administrator to request access."). Required.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The drawing shown now (for tests).</summary>
    internal IllustrationDrawing? CurrentArt => _art.Current;

    /// <summary>The title label (for tests).</summary>
    internal CdsText TitleLabel => _title;

    /// <summary>The label above the title (for tests).</summary>
    internal CdsText LabelText => _label;

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _body.Content = newBody;

    void UpdateLayout()
    {
        _art.Light = FullPageErrorModel.Art(Kind);
        _label.Text = string.IsNullOrEmpty(Label) ? null : FullPageErrorModel.LabelArrow + Label;
        _label.IsVisible = !string.IsNullOrEmpty(Label);
        _title.Text = Title;
        SemanticProperties.SetDescription(_title, FullPageErrorModel.HeadingDescription(Label, Title));
        _description.Text = Description;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        RefreshVisuals();
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsFullPageError)b).UpdateLayout();
}
