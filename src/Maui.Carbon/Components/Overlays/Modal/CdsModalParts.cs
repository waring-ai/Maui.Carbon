using System.ComponentModel;
using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// The header of a <see cref="CdsComposedModal"/> (or of the all-in-one <see cref="CdsModal"/>): a small
/// <c>label-01</c> label above a <c>heading-03</c> title, with the 48px close button in the top end corner. The title
/// wraps at about 80% of the width. Clicking the close button raises <see cref="CloseButtonClicked"/> and asks the
/// modal to close.
/// </summary>
public class CdsModalHeader : ContentView
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsModalHeader), null,
        propertyChanged: static (b, _, _) => ((CdsModalHeader)b).Sync());

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsModalHeader), null,
        propertyChanged: static (b, _, _) => ((CdsModalHeader)b).Sync());

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsModalHeader), "Close",
        propertyChanged: static (b, _, n) => ((CdsModalHeader)b)._close.Description = (string?)n ?? "Close");

    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsModalHeader), null,
        propertyChanged: static (b, o, n) => ((CdsModalHeader)b).OnDecoratorChanged(o as View, n as View));

    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, IsVisible = false, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Heading03, LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsModalCloseButton _close = new();
    readonly VerticalStackLayout _texts = new() { Spacing = 0 };
    readonly Grid _root = new();

    /// <summary>Creates a header.</summary>
    public CdsModalHeader()
    {
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);
        _texts.Add(_label);
        _texts.Add(_title);
        _root.MinimumHeightRequest = CdsModalSizing.CloseButtonSize;
        _root.Add(_texts);
        _root.Add(_close);
        // 16 start and top, 48 end for the close button, 8 below (the gap to the body).
        Padding = new Thickness(0, 0, 0, CdsSpacing.Spacing03);
        _texts.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsModalSizing.CloseButtonSize, 0);
        Content = _root;
        _close.Clicked += (_, _) => CloseButtonClicked?.Invoke(this, EventArgs.Empty);
        SizeChanged += (_, _) => UpdateTitleMeasure();
        Sync();
    }

    /// <summary>Raised when the close button is activated.</summary>
    public event EventHandler? CloseButtonClicked;

    /// <summary>Raised when <see cref="IsAiPresence"/> changes: the decorator became or stopped being an AI label.</summary>
    internal event EventHandler? AiPresenceChanged;

    /// <summary>The small label above the title (a category or the name of the thing the modal is about).</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The heading: a short verb phrase that matches the button that opened the modal ("Create API key").</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The accessible name and tooltip of the close button. Default <c>Close</c>.</summary>
    public string IconDescription
    {
        get => (string)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>
    /// A view at the end of the header, before the close button (Carbon's <c>decorator</c> slot). A <see cref="CdsAILabel"/> is shown at
    /// size <see cref="CdsAILabelSize.Sm"/> (32), 10px from the top and just before the close button, and gives the modal Carbon's AI
    /// presence: the AI aura, the gradient border and the AI scrim. Like Carbon, the modal keeps that presence while the label shows its
    /// revert button (<see cref="CdsAILabel.IsRevertActive"/>); the label is then centered on the close button's line. Any other view sits
    /// 1rem from the top without AI styling. A <see cref="CdsModal"/> or <see cref="CdsComposedModal"/> forwards its own
    /// <c>Decorator</c> here.
    /// </summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>Whether the <see cref="Decorator"/> is a <see cref="CdsAILabel"/>: the modal then takes Carbon's AI presence.</summary>
    internal bool IsAiPresence => Decorator is CdsAILabel;

    /// <summary>Space between label and title (4 in a modal, 2 in a dialog).</summary>
    internal double LabelSpacing
    {
        get => _texts.Spacing;
        set => _texts.Spacing = value;
    }

    internal VisualElement CloseTarget => _close.HitTarget;

    /// <summary>
    /// Whether the close button can be used. The modal turns it off while its submit runs (<c>loadingStatus</c>): it
    /// then draws disabled and its hit target is disabled, so it is not focusable or clickable.
    /// </summary>
    internal bool IsCloseEnabled
    {
        get => _close.IsEnabled;
        set => _close.IsEnabled = value;
    }

    void Sync()
    {
        _label.Text = Label;
        _label.IsVisible = !string.IsNullOrEmpty(Label);
        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
    }

    void OnDecoratorChanged(View? old, View? decorator)
    {
        var wasAi = old is CdsAILabel;
        if (old is not null)
        {
            old.PropertyChanged -= OnDecoratorPropertyChanged;
            old.SizeChanged -= OnDecoratorSizeChanged;
            _root.Remove(old);
        }
        if (decorator is not null)
        {
            // Carbon: "AILabel always size sm".
            if (decorator is CdsAILabel label) label.Size = CdsAILabelSize.Sm;
            decorator.HorizontalOptions = LayoutOptions.End;
            decorator.VerticalOptions = LayoutOptions.Start;
            decorator.PropertyChanged += OnDecoratorPropertyChanged;
            decorator.SizeChanged += OnDecoratorSizeChanged;
            // Before the close button: it reads and tabs before it, as in Carbon's markup.
            _root.Insert(_root.IndexOf(_close), decorator);
        }
        PlaceDecorator();
        UpdateTitleMeasure();
        if (wasAi != decorator is CdsAILabel) AiPresenceChanged?.Invoke(this, EventArgs.Empty);
    }

    void OnDecoratorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == CdsAILabel.IsRevertActiveProperty.PropertyName) PlaceDecorator();
    }

    void OnDecoratorSizeChanged(object? sender, EventArgs e) => UpdateTitleMeasure();

    void PlaceDecorator()
    {
        if (Decorator is not { } decorator) return;
        var ai = decorator as CdsAILabel;
        var height = ai is not null ? CdsAILabelTokens.Side(CdsAILabelSize.Sm) : 0;
        decorator.Margin = new Thickness(0, CdsModalSizing.DecoratorTopMargin(ai is not null, ai?.IsRevertActive == true, height), CdsModalSizing.CloseButtonSize, 0);
    }

    // Carbon reserves nothing for the decorator; here a long heading would run under it on a narrow modal, so the heading and the label
    // above it stop short of the decorator and the gap before it.
    double DecoratorReserve() => Decorator switch
    {
        null => 0,
        CdsAILabel => CdsAILabelTokens.Side(CdsAILabelSize.Sm) + CdsSpacing.Spacing03,
        { Width: > 0 } d => d.Width + CdsSpacing.Spacing03,
        { WidthRequest: > 0 } d => d.WidthRequest + CdsSpacing.Spacing03,
        _ => 0,
    };

    void UpdateTitleMeasure()
    {
        var reserve = DecoratorReserve();
        _title.Margin = new Thickness(0, 0, Math.Max(CdsModalSizing.HeadingEndPadding(Width), reserve), 0);
        _label.Margin = new Thickness(0, 0, reserve, 0);
    }
}

/// <summary>
/// The scrolling body of a <see cref="CdsComposedModal"/>: <c>body-01</c> content with 16 side padding and 48 below, in
/// one Carbon layer up (fields inside use the next layer's tokens). Only the body scrolls; it fades out at the bottom
/// while content overflows. A plain label body wraps at about 80% of the width.
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsModalBody : ContentView
{
    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsModalBody), null,
        propertyChanged: static (b, _, n) => ((CdsModalBody)b)._layer.Body = (View?)n);

    /// <summary>Bindable property for <see cref="HasForm"/>.</summary>
    public static readonly BindableProperty HasFormProperty = BindableProperty.Create(
        nameof(HasForm), typeof(bool), typeof(CdsModalBody), false,
        propertyChanged: static (b, _, _) => ((CdsModalBody)b).UpdatePadding());

    /// <summary>Bindable property for <see cref="HasScrollingContent"/>.</summary>
    public static readonly BindableProperty HasScrollingContentProperty = BindableProperty.Create(
        nameof(HasScrollingContent), typeof(bool), typeof(CdsModalBody), false,
        propertyChanged: static (b, _, _) => ((CdsModalBody)b).UpdateFade());

    /// <summary>Bindable property for <see cref="IsFullWidth"/>.</summary>
    public static readonly BindableProperty IsFullWidthProperty = BindableProperty.Create(
        nameof(IsFullWidth), typeof(bool), typeof(CdsModalBody), false,
        propertyChanged: static (b, _, _) => ((CdsModalBody)b).UpdatePadding());

    readonly CdsLayer _layer = new();
    readonly ContentView _pad = new();
    readonly ScrollView _scroller;
    readonly CdsEdgeFade _fade = new(CdsColorKeys.Layer01, true, 32, 48) { IsVisible = false };

    /// <summary>Creates a body.</summary>
    public CdsModalBody()
    {
        _pad.Content = _layer;
        _scroller = new ScrollView { Orientation = ScrollOrientation.Vertical, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, Content = _pad };
        Content = new CdsScrollFadeLayout(_scroller, _fade);
        _scroller.SizeChanged += (_, _) => { UpdatePadding(); UpdateFade(); };
        _scroller.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == ScrollView.ContentSizeProperty.PropertyName) UpdateFade();
        };
        UpdatePadding();
    }

    /// <summary>The body content.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>The body holds form fields: the content then runs the full width instead of wrapping at 80%.</summary>
    public bool HasForm
    {
        get => (bool)GetValue(HasFormProperty);
        set => SetValue(HasFormProperty, value);
    }

    /// <summary>Always show the scrolling treatment (the bottom fade), even when the content fits.</summary>
    public bool HasScrollingContent
    {
        get => (bool)GetValue(HasScrollingContentProperty);
        set => SetValue(HasScrollingContentProperty, value);
    }

    /// <summary>Remove the body padding so content (a table, an image) runs edge to edge.</summary>
    public bool IsFullWidth
    {
        get => (bool)GetValue(IsFullWidthProperty);
        set => SetValue(IsFullWidthProperty, value);
    }

    /// <summary>The first enabled focusable control in the body, if any (initial focus goes there).</summary>
    internal VisualElement? FirstFocusable() => FindFocusable(Body);

    void UpdatePadding()
    {
        if (IsFullWidth)
        {
            _pad.Padding = 0;
            return;
        }
        var end = CdsSpacing.Spacing05;
        if (!HasForm && Body is Label) end += CdsModalSizing.ParagraphEndPadding(Width);
        _pad.Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03, end, CdsSpacing.Spacing09);
    }

    void UpdateFade() =>
        _fade.IsVisible = CdsModalSizing.ShowsFade(_scroller.ContentSize.Height, _scroller.Height, HasScrollingContent);

    /// <summary>
    /// In an AI modal the bottom fade paints the container's AI fill under it instead of flat <c>layer-01</c> (Carbon masks the content
    /// there so the aura shows through); <c>null</c> restores the flat fade. For tests: the fade's current fill.
    /// </summary>
    internal void SetAiFade(Brush? fill) => _fade.Fill = fill;

    /// <summary>The fill the bottom fade shows now (for tests).</summary>
    internal Brush? FadeBackground => _fade.Background;

    static VisualElement? FindFocusable(Element? element)
    {
        switch (element)
        {
            case null:
                return null;
            case CdsControl c when c.IsEnabled && c.IsVisible && c.AccessibilityTarget is not null:
                return c;
            case InputView { IsEnabled: true, IsVisible: true } input:
                return input;
            case IVisualTreeElement tree:
                foreach (var child in tree.GetVisualChildren())
                    if (FindFocusable(child as Element) is { } found) return found;
                break;
        }
        return null;
    }
}

/// <summary>
/// The footer of a <see cref="CdsComposedModal"/>: 64px high with full-width buttons that touch, label at the top start
/// corner. One button takes the end half, two fill the footer, three take 25% each (only the rightmost is primary).
/// The secondary button is on the left; <see cref="PrimaryButtonText"/> is on the right. Set <see cref="Body"/> to use
/// your own buttons instead.
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsModalFooter : ContentView
{
    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsModalFooter), null,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).Rebuild());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsModalFooter), null,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).Rebuild());

    /// <summary>Bindable property for <see cref="TertiaryButtonText"/>.</summary>
    public static readonly BindableProperty TertiaryButtonTextProperty = BindableProperty.Create(
        nameof(TertiaryButtonText), typeof(string), typeof(CdsModalFooter), null,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsDanger"/>.</summary>
    public static readonly BindableProperty IsDangerProperty = BindableProperty.Create(
        nameof(IsDanger), typeof(bool), typeof(CdsModalFooter), false,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).Rebuild());

    /// <summary>Bindable property for <see cref="PrimaryButtonDisabled"/>.</summary>
    public static readonly BindableProperty PrimaryButtonDisabledProperty = BindableProperty.Create(
        nameof(PrimaryButtonDisabled), typeof(bool), typeof(CdsModalFooter), false,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).ApplyLoading());

    /// <summary>Bindable property for <see cref="LoadingStatus"/>.</summary>
    public static readonly BindableProperty LoadingStatusProperty = BindableProperty.Create(
        nameof(LoadingStatus), typeof(CdsInlineLoadingStatus), typeof(CdsModalFooter), CdsInlineLoadingStatus.Inactive,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).ApplyLoading());

    /// <summary>Bindable property for <see cref="LoadingDescription"/>.</summary>
    public static readonly BindableProperty LoadingDescriptionProperty = BindableProperty.Create(
        nameof(LoadingDescription), typeof(string), typeof(CdsModalFooter), null,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).ApplyLoading());

    /// <summary>Bindable property for <see cref="LoadingIconDescription"/>.</summary>
    public static readonly BindableProperty LoadingIconDescriptionProperty = BindableProperty.Create(
        nameof(LoadingIconDescription), typeof(string), typeof(CdsModalFooter), null,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).ApplyLoading());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsModalFooter), null,
        propertyChanged: static (b, _, _) => ((CdsModalFooter)b).Rebuild());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsModalFooter), null);

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(ICommand), typeof(CdsModalFooter), null);

    CdsModalButton? _primary;
    CdsModalButton? _secondary;
    CdsModalButton? _tertiary;

    /// <summary>Creates an empty footer.</summary>
    public CdsModalFooter()
    {
        IsVisible = false;
    }

    /// <summary>Raised when the primary button is activated. The modal stays open: close it after validating.</summary>
    public event EventHandler? RequestSubmit;

    /// <summary>
    /// Raised when the secondary button is activated and nothing handles <see cref="SecondarySubmit"/>. In a
    /// <see cref="CdsComposedModal"/> it is raised only when the modal agreed to close (no
    /// <see cref="CdsComposedModal.RequestClose"/> handler cancelled).
    /// </summary>
    public event EventHandler? RequestClose;

    /// <summary>Raised when the secondary button is activated; when it has handlers they replace <see cref="RequestClose"/>.</summary>
    public event EventHandler? SecondarySubmit;

    /// <summary>Raised when the tertiary (second secondary) button is activated.</summary>
    public event EventHandler? TertiarySubmit;

    /// <summary>
    /// Raised when the inline loading reports success: <see cref="CdsInlineLoading.SuccessDelay"/> after
    /// <see cref="LoadingStatus"/> became <see cref="CdsInlineLoadingStatus.Finished"/> (<c>onLoadingSuccess</c>). Close the
    /// modal or reset the form here.
    /// </summary>
    public event EventHandler? LoadingSuccess;

    /// <summary>Primary button label: a specific verb ("Create key"), never "OK".</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Secondary button label ("Cancel"), on the left of the primary.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>
    /// Label of the optional third button, the leftmost one (the second <c>secondaryButtons</c> entry of the web version;
    /// drawn as a secondary button). Setting it switches the footer to the three-button layout.
    /// </summary>
    public string? TertiaryButtonText
    {
        get => (string?)GetValue(TertiaryButtonTextProperty);
        set => SetValue(TertiaryButtonTextProperty, value);
    }

    /// <summary>The primary button is a danger button (destructive confirmation).</summary>
    public bool IsDanger
    {
        get => (bool)GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>Disables the primary button.</summary>
    public bool PrimaryButtonDisabled
    {
        get => (bool)GetValue(PrimaryButtonDisabledProperty);
        set => SetValue(PrimaryButtonDisabledProperty, value);
    }

    /// <summary>
    /// The submit wait (<c>loadingStatus</c>): anything but <see cref="CdsInlineLoadingStatus.Inactive"/> (the default)
    /// shows a <see cref="CdsInlineLoading"/> with <see cref="LoadingDescription"/> in place of the primary label and
    /// disables every footer button, so the submit cannot run twice. Use it for short waits after submit; set it back to
    /// Inactive, or close the modal on <see cref="LoadingSuccess"/>.
    /// </summary>
    public CdsInlineLoadingStatus LoadingStatus
    {
        get => (CdsInlineLoadingStatus)GetValue(LoadingStatusProperty);
        set => SetValue(LoadingStatusProperty, value);
    }

    /// <summary>The loading text in place of the primary label ("Creating key...", then "Key created" or "Failed").</summary>
    public string? LoadingDescription
    {
        get => (string?)GetValue(LoadingDescriptionProperty);
        set => SetValue(LoadingDescriptionProperty, value);
    }

    /// <summary>Accessible name of the loading icon (<c>loadingIconDescription</c>); defaults to the inline loading's own.</summary>
    public string? LoadingIconDescription
    {
        get => (string?)GetValue(LoadingIconDescriptionProperty);
        set => SetValue(LoadingIconDescriptionProperty, value);
    }

    /// <summary>True while <see cref="LoadingStatus"/> is not Inactive: the footer buttons are disabled.</summary>
    internal bool IsLoading => LoadingStatus != CdsInlineLoadingStatus.Inactive;

    /// <summary>The inline loading shown in the primary button, while there is one.</summary>
    internal CdsInlineLoading? PrimaryLoading => _primary?.Loading;

    /// <summary>The generated footer buttons, left to right (tertiary, secondary, primary), for inspection.</summary>
    internal IEnumerable<VisualElement> Buttons =>
        new[] { _tertiary, _secondary, _primary }.Where(b => b is not null).Cast<VisualElement>();

    /// <summary>Your own footer content instead of the buttons.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Runs when the primary button is activated.</summary>
    public ICommand? RequestSubmitCommand
    {
        get => (ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }

    /// <summary>Runs after <see cref="RequestClose"/> (so not when a <see cref="CdsComposedModal"/> cancelled the close).</summary>
    public ICommand? RequestCloseCommand
    {
        get => (ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }

    /// <summary>The element initial focus goes to: the secondary button of a danger modal, else the primary button.</summary>
    internal VisualElement? InitialFocus(bool danger) =>
        danger && _secondary is not null ? _secondary.HitTarget
        : _primary is { IsEnabled: true } ? _primary.HitTarget
        : null;

    /// <summary>Whether the footer shows anything.</summary>
    internal bool HasContent => Body is not null || _primary is not null || _secondary is not null;

    /// <summary>True when the secondary button should close the modal (nobody replaced it).</summary>
    internal bool SecondaryClosesModal => SecondarySubmit is null;

    internal void RaiseSecondary()
    {
        if (SecondarySubmit is not null) SecondarySubmit.Invoke(this, EventArgs.Empty);
        else RaiseClose();
    }

    /// <summary>
    /// Set by the modal that shows this footer: decides whether a close request goes ahead and, if so, calls the action
    /// it is given (the footer's own notification) before closing.
    /// </summary>
    internal Func<Action, bool>? CloseHandler { get; set; }

    internal void RaiseClose()
    {
        if (IsLoading) return;
        if (CloseHandler is { } handler) handler(NotifyClose);
        else NotifyClose();
    }

    void NotifyClose()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
        if (RequestCloseCommand?.CanExecute(null) == true) RequestCloseCommand.Execute(null);
    }

    void ApplyLoading()
    {
        var loading = IsLoading;
        if (_tertiary is not null) _tertiary.IsEnabled = !loading;
        if (_secondary is not null) _secondary.IsEnabled = !loading;
        if (_primary is not null)
        {
            _primary.IsEnabled = !loading && !PrimaryButtonDisabled;
            _primary.SetLoading(LoadingStatus, LoadingDescription, LoadingIconDescription);
        }
    }

    void Rebuild()
    {
        if (_primary is not null) _primary.LoadingSuccess -= OnLoadingSuccess;
        _primary = _secondary = _tertiary = null;
        if (Body is not null)
        {
            Content = Body;
            IsVisible = true;
            return;
        }

        var buttons = new List<CdsModalButton>();
        if (!string.IsNullOrEmpty(TertiaryButtonText))
        {
            _tertiary = new CdsModalButton(TertiaryButtonText, CdsButtonKind.Secondary);
            _tertiary.Clicked += (_, _) => TertiarySubmit?.Invoke(this, EventArgs.Empty);
            buttons.Add(_tertiary);
        }
        if (!string.IsNullOrEmpty(SecondaryButtonText))
        {
            _secondary = new CdsModalButton(SecondaryButtonText, CdsButtonKind.Secondary);
            _secondary.Clicked += (_, _) => RaiseSecondary();
            buttons.Add(_secondary);
        }
        if (!string.IsNullOrEmpty(PrimaryButtonText))
        {
            _primary = new CdsModalButton(PrimaryButtonText, CdsModalTokens.PrimaryKind(IsDanger));
            _primary.Clicked += (_, _) =>
            {
                if (IsLoading) return;
                RequestSubmit?.Invoke(this, EventArgs.Empty);
                if (RequestSubmitCommand?.CanExecute(null) == true) RequestSubmitCommand.Execute(null);
            };
            _primary.LoadingSuccess += OnLoadingSuccess;
            buttons.Add(_primary);
        }
        ApplyLoading();

        if (buttons.Count == 0)
        {
            Content = null;
            IsVisible = false;
            return;
        }

        var (spacer, button) = CdsModalSizing.FooterColumns(buttons.Count);
        var grid = new Grid { HeightRequest = CdsModalSizing.FooterHeight, ColumnSpacing = 0 };
        if (spacer > 0) grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(spacer, GridUnitType.Star)));
        foreach (var b in buttons)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(button, GridUnitType.Star)));
            grid.Add(b, grid.ColumnDefinitions.Count - 1);
        }
        Content = grid;
        IsVisible = true;
    }

    void OnLoadingSuccess(object? sender, EventArgs e) => LoadingSuccess?.Invoke(this, EventArgs.Empty);
}

/// <summary>Arguments of a modal's <c>RequestClose</c>: set <see cref="CancelEventArgs.Cancel"/> to keep it open.</summary>
public sealed class CdsModalRequestCloseEventArgs : CancelEventArgs
{
}
