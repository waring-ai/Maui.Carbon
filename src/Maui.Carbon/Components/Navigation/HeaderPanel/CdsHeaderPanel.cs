namespace Maui.Carbon;

/// <summary>
/// The 256 wide panel that slides in from the end edge under the 48 high header, opened by a <see cref="CdsHeaderGlobalAction"/>: it
/// holds a <see cref="CdsSwitcher"/> of product links or your own content (notifications, help, a profile). <c>layer-01</c> with
/// <c>text-secondary</c>; 0 wide while closed, 256 with 1px <c>border-subtle-01</c> side borders while <see cref="IsExpanded"/>, the width animating
/// over <c>duration-fast-02</c> with the productive exit curve (it jumps under reduced motion). It floats over the page and never
/// pushes content; only one panel is open at a time, and its action shows <see cref="CdsShellItem.IsActive"/> while it is open.
/// <para>
/// In a <see cref="CdsUIShell"/> assign it to <see cref="CdsUIShell.HeaderPanel"/>: the shell places it under the header and
/// closes it on a tap outside (when it holds a switcher). Standalone, put it in a layout cell that covers the area below your
/// header, aligned to the end, with a 48 top margin (for example a <c>Grid</c> row 1 with <c>HorizontalOptions="End"</c>); it
/// then closes on Escape only, so also give it <see cref="ReturnFocusTo"/> and toggle <see cref="IsExpanded"/> from your action.
/// </para>
/// Escape closes it (when focus is inside) and returns focus to <see cref="ReturnFocusTo"/>; for custom content give it a
/// <see cref="Label"/> and make links non-focusable while it is closed (this panel hides itself, so they already are).
/// </summary>
public class CdsHeaderPanel : CdsContainer
{
    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsHeaderPanel), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsHeaderPanel)b).OnExpandedChanged((bool)n));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsHeaderPanel), null,
        propertyChanged: static (b, _, n) => SemanticProperties.SetDescription((CdsHeaderPanel)b, (string?)n));

    /// <summary>Bindable property for <see cref="ClosesOnOutsideTap"/>.</summary>
    public static readonly BindableProperty ClosesOnOutsideTapProperty = BindableProperty.Create(
        nameof(ClosesOnOutsideTap), typeof(bool?), typeof(CdsHeaderPanel), null);

    /// <summary>Bindable property for <see cref="ClosesOnEscape"/>.</summary>
    public static readonly BindableProperty ClosesOnEscapeProperty = BindableProperty.Create(
        nameof(ClosesOnEscape), typeof(bool), typeof(CdsHeaderPanel), true);

    /// <summary>Bindable property for <see cref="ReturnFocusTo"/>.</summary>
    public static readonly BindableProperty ReturnFocusToProperty = BindableProperty.Create(
        nameof(ReturnFocusTo), typeof(VisualElement), typeof(CdsHeaderPanel), null);

    readonly Grid _clip = new() { IsClippedToBounds = true };
    readonly Grid _panel = new() { WidthRequest = UIShellModel.HeaderPanelWidth, HorizontalOptions = LayoutOptions.Start };
    readonly ScrollView _scroll = new();
    readonly BoxView _left = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
    readonly BoxView _right = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true };
    double _width;

    /// <summary>Creates a closed panel.</summary>
    public CdsHeaderPanel()
    {
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Fill;
        var fill = new BoxView { InputTransparent = true };
        CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetColor(_left, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(_right, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _panel.Add(fill);
        _panel.Add(_scroll);
        _panel.Add(_left);
        _panel.Add(_right);
        _clip.Add(_panel);
        SetChrome(_clip);
        SetWidth(0);
    }

    /// <summary>Raised when the (animating) width changes.</summary>
    internal event EventHandler? WidthChanged;

    /// <summary>Open state (two-way). Set it from the opening action and clear it on Escape or an outside tap.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>The accessible name for custom content (a switcher is named by its own label).</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Whether a tap outside the panel closes it. <c>null</c> (default) closes it only when the panel holds a
    /// <see cref="CdsSwitcher"/>, as Carbon does; custom panels usually stay open until their action or Escape closes them.
    /// </summary>
    public bool? ClosesOnOutsideTap
    {
        get => (bool?)GetValue(ClosesOnOutsideTapProperty);
        set => SetValue(ClosesOnOutsideTapProperty, value);
    }

    /// <summary>Whether Escape closes the panel while focus is inside (Carbon's <c>addFocusListeners</c>). Default <c>true</c>.</summary>
    public bool ClosesOnEscape
    {
        get => (bool)GetValue(ClosesOnEscapeProperty);
        set => SetValue(ClosesOnEscapeProperty, value);
    }

    /// <summary>The element that gets focus back when Escape closes the panel (the action that opened it).</summary>
    public VisualElement? ReturnFocusTo
    {
        get => (VisualElement?)GetValue(ReturnFocusToProperty);
        set => SetValue(ReturnFocusToProperty, value);
    }

    /// <summary>The current (animating) width, 0 to 256.</summary>
    public double CurrentWidth => _width;

    /// <summary>Whether a tap outside currently closes the panel (see <see cref="ClosesOnOutsideTap"/>).</summary>
    public bool ClosesOnOutsideTapEffective => UIShellModel.ClosesOnOutsideTap(ClosesOnOutsideTap, Body is CdsSwitcher);

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => IsExpanded && ClosesOnEscape;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || !IsExpanded || !ClosesOnEscape) return false;
        IsExpanded = false;
        ReturnFocusTo?.Focus();
        return true;
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        _scroll.Content = newBody;
        if (newBody is CdsSwitcher switcher) switcher.IsExpanded = IsExpanded;
    }

    void OnExpandedChanged(bool open)
    {
        if (Body is CdsSwitcher switcher) switcher.IsExpanded = open;
        _left.IsVisible = _right.IsVisible = open;
        var target = open ? UIShellModel.HeaderPanelWidth : 0;
        if (open) _clip.IsVisible = true;
        _ = CdsMotion.AnimateAsync(this, "CdsHeaderPanelWidth", SetWidth, _width, target, CdsDuration.Fast02, CdsEasing.ExitProductive);
    }

    void SetWidth(double width)
    {
        _width = width;
        WidthRequest = width;
        _clip.IsVisible = width > 0.5;
        _left.IsVisible = _right.IsVisible = IsExpanded || width > 0.5;
        WidthChanged?.Invoke(this, EventArgs.Empty);
    }
}
