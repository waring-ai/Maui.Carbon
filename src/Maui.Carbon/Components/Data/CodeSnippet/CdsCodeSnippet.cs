using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// Displays code or a command: inline in text (<see cref="CdsCodeSnippetType.Inline"/>), as one copyable line
/// (<see cref="CdsCodeSnippetType.Single"/>, default) or as a multi-line block that can expand
/// (<see cref="CdsCodeSnippetType.Multi"/>). The fill is <c>layer-01</c> (wrap it in a <see cref="CdsLayer"/> on a layer
/// that needs it), the type is <c>code-01</c> mono. The copy button writes the code (or <see cref="CopyText"/>) to the
/// clipboard and shows <see cref="Feedback"/> for <see cref="FeedbackTimeout"/> ms. Snippets are read-only: use a text input
/// when people must edit the value. <c>IsEnabled="False"</c> blocks copying.
/// <para>
/// The code area of single and multi-line snippets is a focusable, read-only text box named by <see cref="Label"/>: keyboard
/// users Tab to it and scroll it with the arrow keys, Page Up / Page Down and Home / End. Changing only
/// <see cref="Code"/> updates the text in place (the scroll position, focus and tooltip stay), so a snippet can show a live log.
/// </para>
/// </summary>
[ContentProperty(nameof(Code))]
public partial class CdsCodeSnippet : CdsControl
{
    readonly CdsSnippetTip _tip = new();
    readonly ScrollView _scroller = new() { HorizontalScrollBarVisibility = ScrollBarVisibility.Never };
    readonly CdsText _code = new() { TypeStyle = CdsTypeStyle.Code01 };
    readonly CdsEdgeFade _fade = new(CdsColorKeys.Layer01, false, 32, 0) { IsVisible = false };
    readonly Border _inlineBox = new() { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 4 }, Padding = 0, InputTransparent = true };
    readonly CdsFocusRing _codeRing = new(CdsFocusRingStyle.Inset);
    CdsSnippetAction? _copy;
    CdsSnippetAction? _expand;
    CdsHitTarget? _inlineHit;
    CdsHitTarget? _codeHit;
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
        // The code is read through the code area's text box; the label inside the scroller would only repeat it.
        AutomationProperties.SetExcludedWithChildren(_scroller, true);
        Loaded += (_, _) => ApplySemantics();
        Unloaded += (_, _) => _tip.Hide();
        Rebuild();
    }

    /// <summary>Raised after the code was copied (the standard click of the web version), before <see cref="CdsCodeSnippet.Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>
    /// Raised when writing to the clipboard failed (no clipboard, a sandbox or permission refused it). The snippet shows and
    /// announces <see cref="CopyFailedFeedback"/> instead of <see cref="Feedback"/>, and neither <see cref="Clicked"/> nor
    /// <see cref="Command"/> runs.
    /// </summary>
    public event EventHandler? CopyFailed;

    /// <summary>Writes the text to the clipboard. A seam for tests; the default is <c>Clipboard.Default.SetTextAsync</c>.</summary>
    internal Func<string, Task> WriteClipboard { get; set; } = static text => Clipboard.Default.SetTextAsync(text);

    /// <summary>The focusable code area of a single or multi-line snippet (<c>null</c> for inline snippets).</summary>
    internal CdsHitTarget? CodeArea => _codeHit;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        // Only the inline snippet copies when activated; Enter or Space on the code area does nothing.
        if (_inlineHit is not null) _ = CopyAsync(this);
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => _codeHit is not null;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (_codeHit is not { IsFocused: true }) return false;
        var to = CdsCodeSnippetModel.ScrollOffset(e.Key, _scroller.ScrollX, _scroller.ScrollY,
            _scroller.Width, _scroller.Height, _scroller.ContentSize.Width, _scroller.ContentSize.Height);
        if (to is not { } offset) return false;
        e.Handled = true;
        _ = _scroller.ScrollToAsync(offset.X, offset.Y, false);
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled) || !IsEnabled;
        SetColor(_code, CdsText.TextColorProperty, CdsCodeSnippetTokens.Text(disabled));
        if (Type == CdsCodeSnippetType.Inline)
        {
            var plain = HideCopyButton || disabled;
            SetColor(_inlineBox, BackgroundColorProperty, plain ? CdsCodeSnippetTokens.PlainFill : CdsCodeSnippetTokens.Fill(State));
            SetBrush(_inlineBox, Border.StrokeProperty, CdsCodeSnippetTokens.InlineBorder(State));
        }
        if (_copy is not null) _copy.IsEnabled = !disabled;
        if (_expand is not null) _expand.IsEnabled = !disabled;
        // Disabled removes the code area from the tab order.
        if (_codeHit is not null) _codeHit.IsEnabled = !disabled;
        _codeRing.IsActive = _codeHit is not null && Is(CdsInteractionState.FocusVisible);
        UpdateFade();
    }

    // Updates only the text: the layout, the native scroll view (and its offset), the buttons, their focus and the tooltip stay.
    void OnCodeChanged()
    {
        _code.Text = Code;
        if (Type == CdsCodeSnippetType.Multi && !IsWrapText) _code.MaxLines = Math.Max(1, CdsCodeSnippetModel.LineCount(Code));
        ApplySemantics();
        UpdateMulti();
        UpdateFade();
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
        _codeHit = null;
        // Free the shared parts from the previous layout before they are placed in the next one.
        _scroller.Content = null;
        _inlineBox.Content = null;
        Detach(_scroller);
        Detach(_fade);
        Detach(_inlineBox);
        Detach(_codeRing);

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
        // The code area: [text box][scroller][fade][focus ring]. The text box sits under the scroller so the pointer and
        // trackpad still scroll it; Tab and the screen reader reach it.
        var codeArea = new Grid();
        codeArea.Add(NewCodeHit());
        codeArea.Add(_scroller);
        codeArea.Add(_fade);
        _fade.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
        _codeRing.Margin = 0;
        codeArea.Add(_codeRing);
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
        _scroller.Margin = 0;
        _scroller.Content = _code;
        _fade.Margin = new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05);

        var root = new Grid();
        CdsTokens.SetColor(root, BackgroundColorProperty, CdsColorKeys.Layer01);
        // The code area inside the 16 padding: [text box][scroller][focus ring drawn just outside it (outline-offset 0)].
        var codeArea = new Grid { Margin = new Thickness(CdsSpacing.Spacing05), VerticalOptions = LayoutOptions.Start };
        codeArea.Add(NewCodeHit());
        codeArea.Add(_scroller);
        _codeRing.Margin = new Thickness(-2);
        codeArea.Add(_codeRing);
        root.Add(codeArea);
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
        _expand.StateChanged += (_, _) => UpdateFade();
        root.Add(_expand);
        return new CdsMaxWidthBox(root, 768);
    }

    // The read-only text box of the code area (role="textbox", aria-readonly): a tab stop that the arrow keys scroll.
    CdsHitTarget NewCodeHit()
    {
        var hit = _codeHit = CreateHitTarget(CdsAccessibilityRole.TextBox);
        hit.IsReadOnly = true;
        return hit;
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
            UpdateFade();
            var active = button.Is(CdsInteractionState.Hover) || button.Is(CdsInteractionState.FocusVisible);
            if (DateTime.UtcNow < _feedbackUntil) return;
            if (active && IsEnabled) _tip.Show(button, CopyButtonDescription, FeedbackPlacement, 0);
            else _tip.Hide();
        };
        return button;
    }

    void ApplySemantics()
    {
        if (_copy is not null) _copy.Description = CopyButtonDescription;
        // The inline button, or the code area's text box: named by Label (aria-label) and described by the code; without a
        // Label the code itself is the name.
        if ((_inlineHit ?? _codeHit) is { } target)
        {
            var label = Label;
            var named = !string.IsNullOrEmpty(label);
            SemanticProperties.SetDescription(target, named ? label : Code);
            if (named) SemanticProperties.SetHint(target, Code);
            else target.ClearValue(SemanticProperties.HintProperty);
        }
        if (_inlineHit is not null) ToolTipProperties.SetText(_inlineHit, CopyButtonDescription);
    }

    // Focus anywhere in the snippet (the code area or one of its buttons) removes the fade (Carbon's :focus-within).
    bool HasFocusWithin =>
        Is(CdsInteractionState.Focused) || _copy?.Is(CdsInteractionState.Focused) == true || _expand?.Is(CdsInteractionState.Focused) == true;

    void UpdateFade()
    {
        _fade.IsVisible = Type != CdsCodeSnippetType.Inline && !(Type == CdsCodeSnippetType.Multi && IsWrapText) && !HasFocusWithin
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
        var placement = Type == CdsCodeSnippetType.Inline ? CdsPlacement.Bottom : FeedbackPlacement;
        try
        {
            await WriteClipboard(CdsCodeSnippetModel.TextToCopy(Code, CopyText));
        }
        catch (Exception)
        {
            // No clipboard, or the platform refused the write: never claim the code was copied.
            ShowFeedback(anchor, CopyFailedFeedback, placement);
            CdsAnnouncer.Announce(CopyFailedFeedback, CdsAnnouncePriority.Assertive);
            CopyFailed?.Invoke(this, EventArgs.Empty);
            return;
        }
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        ShowFeedback(anchor, Feedback, placement);
        CdsAnnouncer.Announce(Feedback);
    }

    void ShowFeedback(VisualElement anchor, string? text, CdsPlacement placement)
    {
        if (FeedbackTimeout > 0) _feedbackUntil = DateTime.UtcNow.AddMilliseconds(FeedbackTimeout);
        _tip.Show(anchor, text ?? string.Empty, placement, FeedbackTimeout);
    }
}
