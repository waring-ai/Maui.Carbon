namespace Maui.Carbon;

/// <summary>
/// A disclosure tile that groups related settings and summarizes them while closed (Carbon for IBM Products'
/// <c>OptionsTile</c>). The 48px (lg) or 64px (xl) header shows a chevron, the <c>heading-compact-01</c>
/// <see cref="Title"/> and a <c>helper-text-01</c> <see cref="Summary"/>; activating it opens the
/// <see cref="CdsContainer.Body"/> one layer up, indented under the title. Without a body the tile is static: a title
/// and summary row. Set <see cref="IsToggled"/> to add a small toggle at the end that enables the whole group. Invalid,
/// warning and locked states replace the summary with an icon and their text; locked also disables the toggle. Keep the
/// title free of interactive content.
/// </summary>
public partial class CdsOptionsTile : CdsContainer
{
    readonly Grid _root = new() { RowSpacing = 0 };
    readonly Grid _header = new() { ColumnSpacing = 0 };
    readonly Grid _zone = new() { ColumnSpacing = 0 };
    readonly CdsIcon _chevron = new(CdsIconName.ChevronDown, 16) { AnchorX = 0.5, AnchorY = 0.5 };
    readonly VerticalStackLayout _heading = new() { Spacing = 0, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.HeadingCompact01, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
    readonly Grid _summaryRow = new() { ColumnSpacing = 0, HeightRequest = CdsSpacing.Spacing05, IsVisible = false };
    readonly CdsIcon _summaryIcon = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center };
    readonly CdsText _summaryText = new() { TypeStyle = CdsTypeStyle.HelperText01, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, Margin = new Thickness(0, 0, CdsSpacing.Spacing05, 0) };
    readonly Grid _contentClip = new() { IsClippedToBounds = true, IsVisible = false };
    readonly VerticalStackLayout _content = new() { Spacing = 0, Padding = OptionsTileModel.ContentPadding };
    readonly CdsLayer _layer = new();
    readonly BoxView _rule = new() { HeightRequest = 1, InputTransparent = true };
    readonly CdsMessageAlert _alert = new();
    CdsToggle? _toggle;
    CdsHitTarget? _hit;
    CdsFocusRing? _ring;
    HorizontalStackLayout? _lockedNote;
    CdsText? _lockedText;
    bool _headerHover;
    bool _syncingToggle;
    int _version;

    /// <summary>Creates a closed, static options tile (it becomes expandable when it gets a body).</summary>
    public CdsOptionsTile()
    {
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(new GridLength(1)));

        _summaryRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _summaryRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _summaryRow.Add(_summaryIcon, 0);
        _summaryRow.Add(_summaryText, 1);
        _heading.Add(_title);
        _heading.Add(_summaryRow);

        _zone.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(OptionsTileModel.ChevronColumnWidth)));
        _zone.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _zone.Padding = new Thickness(0, 0, CdsSpacing.Spacing05, 0);
        _chevron.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_chevron, true);
        _zone.Add(_chevron, 0);
        _zone.Add(_heading, 1);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            _headerHover = true;
            RefreshVisuals();
        };
        pointer.PointerExited += (_, _) =>
        {
            _headerHover = false;
            RefreshVisuals();
        };
        _zone.GestureRecognizers.Add(pointer);

        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _header.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0)));
        _header.Add(_zone, 0);

        _content.Add(_layer);
        _contentClip.Add(_content);
        _root.Add(_header, 0, 0);
        _root.Add(_contentClip, 0, 1);
        _root.Add(_rule, 0, 2);
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Raised when <see cref="IsOpen"/> changes (by the user or in code).</summary>
    public event EventHandler<CdsChoiceChangedEventArgs<bool>>? OpenChanged;

    /// <summary>Raised when the user flips the toggle (Carbon's <c>onToggle</c>), after <see cref="IsToggled"/> changed.</summary>
    public event EventHandler<ToggledEventArgs>? Toggled;

    /// <summary>Whether the tile has a body and therefore opens.</summary>
    public bool IsExpandable => Body is not null;

    /// <summary>The validation state in effect (invalid, then warning, then locked).</summary>
    public CdsOptionsTileStatus Status => OptionsTileModel.Status(IsInvalid, IsWarning, IsLocked);

    /// <summary>The summary line's text as shown (the summary or the state's text; for tests and screen readers).</summary>
    public string? DisplayedSummary =>
        OptionsTileModel.SummaryText(Status, IsExpandable, Summary, InvalidText, WarnText, LockedText) is { Length: > 0 } text
        && !OptionsTileModel.IsSummaryHidden(Status, IsOpen && IsExpandable) ? text : null;

    /// <summary>The toggle, once <see cref="IsToggled"/> has been set (for tests).</summary>
    internal CdsToggle? Toggle => _toggle;

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        _layer.Body = newBody;
        UpdateChrome();
        ApplyOpen();
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsExpandable) IsOpen = !IsOpen;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        state = _headerHover && IsExpandable ? state | CdsInteractionState.Hover : state & ~CdsInteractionState.Hover;
        SetColor(_root, BackgroundColorProperty, CdsOptionsTileTokens.Background);
        SetColor(_rule, BoxView.ColorProperty, CdsOptionsTileTokens.BottomRule);
        SetColor(_zone, BackgroundColorProperty, CdsOptionsTileTokens.HeaderBackground(state));
        SetColor(_title, Label.TextColorProperty, CdsOptionsTileTokens.Title(state));
        SetColor(_chevron, CdsIcon.ColorProperty, CdsOptionsTileTokens.Chevron(state));
        var summary = CdsOptionsTileTokens.Summary(Status, state);
        SetColor(_summaryText, Label.TextColorProperty, summary.Text);
        SetColor(_summaryIcon, CdsIcon.ColorProperty, summary.IconColor);
        if (_lockedText is not null) SetColor(_lockedText, Label.TextColorProperty, CdsColorKeys.TextSecondary);
        if (_lockedNote?.Children.FirstOrDefault() is CdsIcon lockIcon) SetColor(lockIcon, CdsIcon.ColorProperty, CdsColorKeys.IconSecondary);
        if (_ring is not null) _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void UpdateChrome()
    {
        var expandable = IsExpandable;
        var size = OptionsTileModel.Normalize(Size);
        _zone.HeightRequest = OptionsTileModel.HeaderHeight(size);
        _zone.ColumnDefinitions[0].Width = new GridLength(expandable ? OptionsTileModel.ChevronColumnWidth : CdsSpacing.Spacing05);
        _chevron.IsVisible = expandable;
        _title.Text = Title;

        // The summary line: the summary, or the validation state's icon and text.
        var status = Status;
        var text = OptionsTileModel.SummaryText(status, expandable, Summary, InvalidText, WarnText, LockedText);
        var glyph = CdsOptionsTileTokens.Glyph(status);
        _summaryIcon.Icon = glyph;
        _summaryIcon.IsVisible = glyph is not null;
        _summaryRow.ColumnSpacing = glyph is null ? 0 : CdsSpacing.Spacing02;
        _summaryText.Text = text;
        _summaryRow.IsVisible = !string.IsNullOrEmpty(text) && !OptionsTileModel.IsSummaryHidden(status, IsOpen && expandable);
        _summaryRow.Margin = new Thickness(0, OptionsTileModel.SummaryGap(size), 0, 0);
        _alert.Update(this, status is CdsOptionsTileStatus.Invalid or CdsOptionsTileStatus.Warning ? text : null);

        UpdateToggle();
        UpdateLockedNote(expandable);
        UpdateHitTarget(expandable);
        RefreshVisuals();
    }

    void UpdateToggle()
    {
        var show = IsToggled is not null;
        _header.ColumnDefinitions[1].Width = new GridLength(show ? OptionsTileModel.ToggleColumnWidth : 0);
        if (!show)
        {
            if (_toggle is not null) _toggle.IsVisible = false;
            return;
        }
        if (_toggle is null)
        {
            _toggle = new CdsToggle
            {
                Size = CdsToggleSize.Sm,
                HideLabel = true,
                LabelA = string.Empty,
                LabelB = string.Empty,
                // The 32px switch sits 16 in from the start of its 64px column (padding-inline 16).
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0),
            };
            _toggle.Toggled += OnToggleToggled;
            _header.Add(_toggle, 1);
        }
        _toggle.IsVisible = true;
        _toggle.HeightRequest = -1;
        _syncingToggle = true;
        try
        {
            _toggle.IsToggled = IsToggled == true;
        }
        finally
        {
            _syncingToggle = false;
        }
        // The toggle is named by the title (aria-labelledby) and disabled while the tile is locked.
        SemanticProperties.SetDescription(_toggle, Title);
        _toggle.IsEnabled = !OptionsTileModel.IsToggleLocked(IsInvalid, IsWarning, IsLocked);
    }

    void OnToggleToggled(object? sender, ToggledEventArgs e)
    {
        if (_syncingToggle) return;
        IsToggled = e.Value;
        Toggled?.Invoke(this, e);
    }

    void UpdateLockedNote(bool expandable)
    {
        var show = expandable && OptionsTileModel.IsToggleLocked(IsInvalid, IsWarning, IsLocked) && !string.IsNullOrEmpty(LockedText);
        if (!show)
        {
            if (_lockedNote is not null) _lockedNote.IsVisible = false;
            return;
        }
        if (_lockedNote is null)
        {
            _lockedText = new CdsText { TypeStyle = CdsTypeStyle.Body01, VerticalOptions = LayoutOptions.Center };
            _lockedNote = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing02, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing06) };
            _lockedNote.Add(new CdsIcon(CdsOptionsTileTokens.LockedIcon, 16) { VerticalOptions = LayoutOptions.Center });
            _lockedNote.Add(_lockedText);
            _content.Insert(0, _lockedNote);
        }
        _lockedText!.Text = LockedText;
        _lockedNote.IsVisible = true;
    }

    void UpdateHitTarget(bool expandable)
    {
        if (!expandable)
        {
            if (_hit is not null) _hit.IsVisible = false;
            if (_ring is not null) _ring.IsVisible = false;
            AutomationProperties.SetExcludedWithChildren(_heading, false);
            SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level3);
            return;
        }
        if (_hit is null)
        {
            _ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
            _hit = CreateHitTarget(CdsAccessibilityRole.Expander);
            _zone.Add(_ring, 0);
            Grid.SetColumnSpan(_ring, 2);
            _zone.Add(_hit, 0);
            Grid.SetColumnSpan(_hit, 2);
        }
        _hit.IsVisible = true;
        _ring!.IsVisible = true;
        // The header is one expander: the title names it and the summary line is its hint.
        AutomationProperties.SetExcludedWithChildren(_heading, true);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.None);
        SemanticProperties.SetDescription(_hit, Title);
        SemanticProperties.SetHint(_hit, DisplayedSummary);
        _hit.IsExpanded = IsOpen;
    }

    void OnOpenChanged(bool oldValue, bool newValue)
    {
        UpdateChrome();
        var version = ++_version;
        if (!IsExpandable || !IsLoaded || Handler is null || Width <= 0 || CdsMotion.IsReducedMotion) ApplyOpen();
        else _ = AnimateAsync(newValue, version);
        OpenChanged?.Invoke(this, new CdsChoiceChangedEventArgs<bool>(oldValue, newValue));
    }

    // The settled state: content shown at its natural height or hidden, chevron down (closed) or up (open).
    void ApplyOpen()
    {
        var open = IsOpen && IsExpandable;
        _contentClip.IsVisible = open;
        _contentClip.HeightRequest = -1;
        _content.Opacity = 1;
        _chevron.Rotation = open ? 180 : 0;
    }

    async Task AnimateAsync(bool opening, int version)
    {
        double from, to;
        if (opening)
        {
            _content.Opacity = 0;
            _contentClip.HeightRequest = 0;
            _contentClip.IsVisible = true;
            from = 0;
            to = _content.Measure(Width, double.PositiveInfinity).Height;
        }
        else
        {
            from = _contentClip.Height > 0 ? _contentClip.Height : _content.Measure(Width, double.PositiveInfinity).Height;
            to = 0;
        }
        var rotationFrom = _chevron.Rotation;
        // Height, padding and opacity move together over duration-moderate-01 (entrance productive); the chevron turns over
        // duration-fast-02.
        await Task.WhenAll(
            CdsMotion.AnimateAsync(this, "CdsOptionsTileContent", p =>
            {
                _contentClip.HeightRequest = OptionsTileModel.HeightAt(from, to, p);
                _content.Opacity = opening ? p : 1 - p;
            }, 0, 1, CdsDuration.Moderate01, CdsEasing.EntranceProductive),
            CdsMotion.AnimateAsync(this, "CdsOptionsTileChevron", p => _chevron.Rotation = p, rotationFrom, opening ? 180 : 0, CdsDuration.Fast02, CdsEasing.StandardProductive));
        if (version == _version) ApplyOpen();
    }
}
