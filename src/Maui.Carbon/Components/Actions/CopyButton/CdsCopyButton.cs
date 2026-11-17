namespace Maui.Carbon;

/// <summary>
/// An icon button with the copy glyph. On activation it writes <see cref="Text"/> to the clipboard, switches its tooltip to
/// the <see cref="Feedback"/> text ("Copied!") for <see cref="FeedbackTimeout"/> milliseconds, announces it to screen
/// readers and raises <see cref="Copied"/>. Use it next to one value people paste elsewhere (an ID, key or URL); for code
/// blocks use a code snippet, for any other action an <see cref="CdsIconButton"/>. It sits on <c>layer-01</c>.
/// </summary>
public class CdsCopyButton : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsCopyButton), null);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsCopyButton), "Copy to clipboard", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Feedback"/>.</summary>
    public static readonly BindableProperty FeedbackProperty = BindableProperty.Create(
        nameof(Feedback), typeof(string), typeof(CdsCopyButton), "Copied!", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="FeedbackTimeout"/>.</summary>
    public static readonly BindableProperty FeedbackTimeoutProperty = BindableProperty.Create(
        nameof(FeedbackTimeout), typeof(int), typeof(CdsCopyButton), 2000,
        coerceValue: static (_, v) => Math.Max(0, (int)v));

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsCopyButton), CdsSize.Md, propertyChanged: OnChanged,
        coerceValue: static (_, v) => CdsCopyButtonTokens.Coerce((CdsSize)v));

    /// <summary>Bindable property for <see cref="TooltipPlacement"/>.</summary>
    public static readonly BindableProperty TooltipPlacementProperty = BindableProperty.Create(
        nameof(TooltipPlacement), typeof(CdsPlacement), typeof(CdsCopyButton), CdsPlacement.Bottom, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsCopied"/>.</summary>
    public static readonly BindableProperty IsCopiedProperty = BindableProperty.Create(
        nameof(IsCopied), typeof(bool), typeof(CdsCopyButton), false, propertyChanged: OnIsCopiedChanged);

    readonly CdsActionFace _face = new();
    readonly CdsIconTooltip _tooltip;
    readonly CopyFeedbackModel _feedback = new();

    /// <summary>Creates a medium copy button.</summary>
    public CdsCopyButton()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _tooltip = new CdsIconTooltip(this, _face);
        _face.Icon = CdsIconName.Copy;
        _face.Mode = CdsActionFaceMode.IconOnly;
        _face.RingStyle = CdsFocusRingStyle.Inset;
        _face.Resolver = CdsCopyButtonTokens.Resolve;
        _face.Activated += (_, _) => _ = CopyAsync();
        _face.StateChanged += (_, _) => _tooltip.StateChanged(_face);
        SetChrome(_face);
        TrackFocus(_face.Hit, () => _face.Hit.IsFocusFromKeyboard);
        Unloaded += (_, _) => _feedback.Reset();
        Loaded += (_, _) =>
        {
            // IsCopied set in XAML renders the feedback tooltip once the button is on screen.
            if (IsCopied && !_tooltip.IsOpen) _tooltip.Pin(FeedbackTimeout);
        };
        Update();
    }

    /// <summary>Raised after the text was written to the clipboard.</summary>
    public event EventHandler? Copied;

    /// <summary>The text written to the clipboard on activation.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Accessible name and tooltip ("Copy to clipboard"). Make it specific when several copy buttons share a view ("Copy key ID").</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>The tooltip text after copying. Default "Copied!". Also announced politely to screen readers.</summary>
    public string? Feedback
    {
        get => (string?)GetValue(FeedbackProperty);
        set => SetValue(FeedbackProperty, value);
    }

    /// <summary>How long the feedback tooltip stays, in milliseconds. Default 2000.</summary>
    public int FeedbackTimeout
    {
        get => (int)GetValue(FeedbackTimeoutProperty);
        set => SetValue(FeedbackTimeoutProperty, value);
    }

    /// <summary>Square side: sm 32 (inside a 32px field or value row), md 40 (default), lg 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Where the tooltip sits. Default <see cref="CdsPlacement.Bottom"/>.</summary>
    public CdsPlacement TooltipPlacement
    {
        get => (CdsPlacement)GetValue(TooltipPlacementProperty);
        set => SetValue(TooltipPlacementProperty, value);
    }

    /// <summary>
    /// True while the feedback is showing. Set it to <c>true</c> to render the button in its feedback state (it reverts
    /// after <see cref="FeedbackTimeout"/> like a real copy).
    /// </summary>
    public bool IsCopied
    {
        get => (bool)GetValue(IsCopiedProperty);
        set => SetValue(IsCopiedProperty, value);
    }

    /// <summary>Writes <see cref="Text"/> to the clipboard and shows the feedback. Called on activation.</summary>
    public async Task CopyAsync()
    {
        if (!IsEnabled) return;
        try
        {
            await Clipboard.Default.SetTextAsync(Text ?? string.Empty);
        }
        catch (Exception ex) when (ex is FeatureNotSupportedException or PermissionException or InvalidOperationException)
        {
            return;
        }
        Copied?.Invoke(this, EventArgs.Empty);
        ShowFeedback();
    }

    void ShowFeedback()
    {
        var token = _feedback.Start();
        SetValue(IsCopiedProperty, true);
        _tooltip.Pin(FeedbackTimeout);
        if (!string.IsNullOrEmpty(Feedback)) SemanticScreenReader.Announce(Feedback);
        _ = Task.Delay(FeedbackTimeout).ContinueWith(_ => Dispatcher.Dispatch(() =>
        {
            if (_feedback.Expire(token)) SetValue(IsCopiedProperty, false);
        }), TaskScheduler.Default);
    }

    void Update()
    {
        _face.ButtonHeight = CdsCopyButtonTokens.Side(Size);
        _face.AccessibleName = IconDescription;
        _tooltip.Placement = TooltipPlacement;
        _tooltip.Text = CopyFeedbackModel.TooltipText(IsCopied, IconDescription, Feedback);
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _face is not null) _face.IsEnabled = IsEnabled;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsCopyButton)b).Update();

    static void OnIsCopiedChanged(BindableObject b, object? o, object? n)
    {
        var button = (CdsCopyButton)b;
        if (n is true && !button._feedback.IsCopied) button.ShowFeedback();
        button.Update();
    }
}
