using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// Displays code or a command: inline in text (<see cref="CdsCodeSnippetType.Inline"/>), as one copyable line
/// (<see cref="CdsCodeSnippetType.Single"/>, default) or as a multi-line block that can expand
/// (<see cref="CdsCodeSnippetType.Multi"/>). The fill is <c>layer-01</c> (wrap it in a <see cref="CdsLayer"/> on a layer
/// that needs it), the type is <c>code-01</c> mono. The copy button writes the code (or <see cref="CopyText"/>) to the
/// clipboard and shows <see cref="Feedback"/> for <see cref="FeedbackTimeout"/> ms. Snippets are read-only: use a text input
/// when people must edit the value. <c>IsEnabled="False"</c> blocks copying.
/// </summary>
[ContentProperty(nameof(Code))]
public partial class CdsCodeSnippet : CdsControl
{
    readonly CdsSnippetTip _tip = new();
    readonly ScrollView _scroller = new() { HorizontalScrollBarVisibility = ScrollBarVisibility.Never };
    readonly CdsText _code = new() { TypeStyle = CdsTypeStyle.Code01 };
    readonly CdsEdgeFade _fade = new(CdsColorKeys.Layer01, false, 32, 0) { IsVisible = false };
    readonly Border _inlineBox = new() { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 4 }, Padding = 0, InputTransparent = true };
    CdsSnippetAction? _copy;
    CdsSnippetAction? _expand;
    CdsHitTarget? _inlineHit;
    DateTime _feedbackUntil = DateTime.MinValue;

    /// <summary>Creates an empty single-line snippet.</summary>
    public CdsCodeSnippet()
    {
        HorizontalOptions = LayoutOptions.Fill;
        _scroller.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName) { UpdateFade(); UpdateMulti(); }
        };
        _scroller.SizeChanged += (_, _) => UpdateFade();
        Loaded += (_, _) => ApplySemantics();
        Unloaded += (_, _) => _tip.Hide();
        Rebuild();
    }

    /// <summary>Raised after the code was copied (the standard click of the web version), before <see cref="CdsCodeSnippet.Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <inheritdoc/>
    protected override void OnActivated() => _ = CopyAsync(this);

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled) || !IsEnabled;
        SetColor(_code, Label.TextColorProperty, CdsCodeSnippetTokens.Text(disabled));
        if (Type == CdsCodeSnippetType.Inline)
        {
            var plain = HideCopyButton || disabled;
            SetColor(_inlineBox, BackgroundColorProperty, plain ? CdsCodeSnippetTokens.PlainFill : CdsCodeSnippetTokens.Fill(State));
            SetBrush(_inlineBox, Border.StrokeProperty, CdsCodeSnippetTokens.InlineBorder(State));
        }
        if (_copy is not null) _copy.IsEnabled = !disabled;
        if (_expand is not null) _expand.IsEnabled = !disabled;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) RefreshVisuals();
    }

    void Rebuild()
    {
        _tip.Hide();
        _code.Text = Code;
        _code.LineBreakMode = LineBreakMode.NoWrap;
        _code.MaxLines = 1;
        _copy = null;
        _expand = null;
        _inlineHit = null;
        // Free the shared parts from the previous layout before they are placed in the next one.
        _scroller.Content = null;
        _inlineBox.Content = null;
        Detach(_scroller);
        Detach(_fade);
        Detach(_inlineBox);

        View chrome = Type switch
        {
            CdsCodeSnippetType.Inline => BuildInline(),
            CdsCodeSnippetType.Multi => BuildMulti(),
            _ => BuildSingle(),
        };
        SetChrome(chrome);
        ApplySemantics();
        UpdateMulti();
        RefreshVisuals();
    }

    View BuildInline()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        _code.Margin = new Thickness(CdsSpacing.Spacing03, 0);
        _code.Padding = 0;
        _inlineBox.Content = _code;
        var root = new Grid();
        root.Add(_inlineBox);
        if (!HideCopyButton)
        {
            _inlineHit = CreateHitTarget();
            AutomationProperties.SetExcludedWithChildren(_inlineBox, true);
            root.Add(_inlineHit);
            _inlineBox.InputTransparent = true;
        }
        return root;
    }

    View BuildSingle()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        _inlineBox.Content = null;
        _code.Margin = 0;
        _code.Padding = new Thickness(0, 0, CdsSpacing.Spacing07, 0);
        _scroller.Orientation = ScrollOrientation.Horizontal;
        _scroller.VerticalOptions = LayoutOptions.Center;
        _scroller.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
        _scroller.Content = _code;

        var root = new Grid { HeightRequest = CdsSizing.Medium };
        root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        CdsTokens.SetColor(root, BackgroundColorProperty, CdsColorKeys.Layer01);
        var codeArea = new Grid();
        codeArea.Add(_scroller);
        codeArea.Add(_fade);
        _fade.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
        root.Add(codeArea);
        if (!HideCopyButton)
        {
            root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _copy = NewCopyButton(CdsSizing.Medium);
            root.Add(_copy, 1);
        }
        else
        {
            _code.Padding = 0;
        }
        return new CdsMaxWidthBox(root, 768);
    }

    View BuildMulti()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        _inlineBox.Content = null;
        _code.Margin = 0;
        _code.LineBreakMode = IsWrapText ? LineBreakMode.WordWrap : LineBreakMode.NoWrap;
        // Native labels draw a single line for non-wrapping modes unless the line count is explicit.
        _code.MaxLines = IsWrapText ? -1 : Math.Max(1, CdsCodeSnippetModel.LineCount(Code));
        _code.Padding = new Thickness(0, 0, HideCopyButton ? 0 : CdsSpacing.Spacing06, CdsCodeSnippetModel.CodeBottomPadding);
        _scroller.Orientation = IsWrapText ? ScrollOrientation.Vertical : ScrollOrientation.Both;
        _scroller.VerticalOptions = LayoutOptions.Start;
        _scroller.Margin = new Thickness(CdsSpacing.Spacing05);
        _scroller.Content = _code;
        _fade.Margin = new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05);

        var root = new Grid();
        CdsTokens.SetColor(root, BackgroundColorProperty, CdsColorKeys.Layer01);
        root.Add(_scroller);
        root.Add(_fade);
        if (!HideCopyButton)
        {
            _copy = NewCopyButton(CdsSizing.Small);
            _copy.HorizontalOptions = LayoutOptions.End;
            _copy.VerticalOptions = LayoutOptions.Start;
            _copy.Margin = new Thickness(0, CdsSpacing.Spacing03, CdsSpacing.Spacing03, 0);
            root.Add(_copy);
        }
        _expand = new CdsSnippetAction(0, CdsSizing.Small, CdsIconName.ChevronDown, ShowMoreText)
        {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            IsVisible = false,
        };
        _expand.IsExpanded = IsExpanded;
        _expand.Clicked += (_, _) => IsExpanded = !IsExpanded;
        root.Add(_expand);
        return new CdsMaxWidthBox(root, 768);
    }

    static void Detach(View view)
    {
        switch (view.Parent)
        {
            case Layout layout: layout.Remove(view); break;
            case ContentView content: content.Content = null; break;
        }
    }

    CdsSnippetAction NewCopyButton(double size)
    {
        var button = new CdsSnippetAction(size, size, CdsIconName.Copy) { Description = CopyButtonDescription };
        button.Clicked += (_, _) => _ = CopyAsync(button);
        button.StateChanged += (_, _) =>
        {
            var active = button.Is(CdsInteractionState.Hover) || button.Is(CdsInteractionState.FocusVisible);
            if (DateTime.UtcNow < _feedbackUntil) return;
            if (active && IsEnabled) _tip.Show(button, CopyButtonDescription, FeedbackPlacement, 0);
            else _tip.Hide();
        };
        return button;
    }

    void ApplySemantics()
    {
        var name = SemanticProperties.GetDescription(this);
        if (_copy is not null) _copy.Description = CopyButtonDescription;
        if (_inlineHit is not null)
        {
            SemanticProperties.SetDescription(_inlineHit, string.IsNullOrEmpty(name) ? Code : name);
            SemanticProperties.SetHint(_inlineHit, Code);
            ToolTipProperties.SetText(_inlineHit, CopyButtonDescription);
        }
        else
        {
            SemanticProperties.SetDescription(_scroller, string.IsNullOrEmpty(name) ? Code : name);
        }
    }

    void UpdateFade()
    {
        _fade.IsVisible = Type != CdsCodeSnippetType.Inline && !(Type == CdsCodeSnippetType.Multi && IsWrapText)
            && _scroller.ContentSize.Width > _scroller.Width + 0.5 && _scroller.Width > 0;
    }

    void UpdateMulti()
    {
        if (Type != CdsCodeSnippetType.Multi || _expand is null) return;
        var content = _scroller.ContentSize.Height;
        if (content <= 0) content = CdsCodeSnippetModel.LineCount(Code) * CdsCodeSnippetModel.RowHeight + CdsCodeSnippetModel.CodeBottomPadding;
        var showExpand = CdsCodeSnippetModel.ShowsExpandButton(content, MaxCollapsedNumberOfRows);
        var viewport = CdsCodeSnippetModel.ViewportHeight(content, IsExpanded && showExpand, MinCollapsedNumberOfRows, MaxCollapsedNumberOfRows, MinExpandedNumberOfRows, MaxExpandedNumberOfRows);
        if (Math.Abs(_scroller.HeightRequest - viewport) > 0.5) _scroller.HeightRequest = viewport;
        _expand.IsVisible = showExpand;
        _expand.Text = CdsCodeSnippetModel.ExpandText(IsExpanded, ShowMoreText, ShowLessText);
        _expand.IconRotation = CdsCodeSnippetModel.ChevronRotation(IsExpanded);
        _expand.IsExpanded = IsExpanded;
    }

    async Task CopyAsync(VisualElement anchor)
    {
        if (!IsEnabled || Is(CdsInteractionState.Disabled)) return;
        if (Type == CdsCodeSnippetType.Inline && HideCopyButton) return;
        try
        {
            await Clipboard.Default.SetTextAsync(CdsCodeSnippetModel.TextToCopy(Code, CopyText));
        }
        catch (Exception)
        {
            // No clipboard (unit tests, a sandboxed platform): still show the feedback so the interaction is complete.
        }
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        if (FeedbackTimeout > 0) _feedbackUntil = DateTime.UtcNow.AddMilliseconds(FeedbackTimeout);
        _tip.Show(anchor, Feedback, Type == CdsCodeSnippetType.Inline ? CdsPlacement.Bottom : FeedbackPlacement, FeedbackTimeout);
        CdsAnnouncer.Announce(Feedback);
    }
}
