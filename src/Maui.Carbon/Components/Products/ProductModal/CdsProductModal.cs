using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The base of the Carbon for IBM Products modals (<see cref="CdsRemoveModal"/>, <see cref="CdsCreateModal"/>,
/// <see cref="CdsExportModal"/>, <see cref="CdsImportModal"/>, <see cref="CdsAboutModal"/>, <see cref="CdsAPIKeyModal"/>):
/// a <see cref="CdsComposedModal"/> that each one fills with its own header, body and footer. Declare the modal anywhere
/// in a page; it draws nothing inline and shows over a scrim in the page's overlay host while <see cref="IsOpen"/> is true.
/// The close button, Escape and the secondary button ask to close (<see cref="RequestClose"/>). Open it only from a user
/// action. Apps cannot derive their own modals from it: compose a <see cref="CdsComposedModal"/> instead.
/// </summary>
public abstract class CdsProductModal : CdsControl
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsProductModal), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsProductModal)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="IsInline"/>.</summary>
    public static readonly BindableProperty IsInlineProperty = BindableProperty.Create(
        nameof(IsInline), typeof(bool), typeof(CdsProductModal), false,
        propertyChanged: static (b, _, _) => ((CdsProductModal)b).UpdatePlacement());

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsProductModal), null);

    /// <summary>Bindable property for <see cref="InitialFocusElement"/>.</summary>
    public static readonly BindableProperty InitialFocusElementProperty = BindableProperty.Create(
        nameof(InitialFocusElement), typeof(VisualElement), typeof(CdsProductModal), null);

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(ICommand), typeof(CdsProductModal), null);

    readonly CdsComposedModal _modal = new();
    readonly Border _frame = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0, IsVisible = false };
    readonly VerticalStackLayout _framePanel = new() { Spacing = 0 };
    readonly Grid _root = new();
    CdsProductModalHeader? _header;
    CdsModalBody? _body;
    CdsModalFooter? _footer;
    bool _partsInline;

    private protected CdsProductModal(CdsModalSize size, bool preventCloseOnClickOutside)
    {
        _modal.Size = size;
        _modal.PreventCloseOnClickOutside = preventCloseOnClickOutside;
        _modal.PropertyChanged += (_, e) =>
        {
            // Closed by its chrome (close button, Escape, secondary button) or by the host.
            if (e.PropertyName == CdsComposedModal.IsOpenProperty.PropertyName && !_modal.IsOpen && IsOpen) IsOpen = false;
        };
        _modal.RequestClose += (_, e) => RaiseRequestClose(e);
        _frame.Content = _framePanel;
        _root.Add(_modal);
        _root.Add(_frame);
        SetChrome(_root);
        UpdatePlacement();
    }

    /// <summary>
    /// Raised when the modal is asked to close: the close button, Escape or the secondary button. Set
    /// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to keep it open; otherwise <see cref="RequestCloseCommand"/>
    /// runs and <see cref="IsOpen"/> becomes false. Inline (<see cref="IsInline"/>) the event is raised and nothing closes.
    /// </summary>
    public event EventHandler<CdsModalRequestCloseEventArgs>? RequestClose;

    /// <summary>Open state (two-way, <c>open</c>). Setting it to <c>true</c> shows the modal once the page is loaded; closing it from the UI sets it to <c>false</c>.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Preview aid (Carbon's inline modal preview): draws the modal container, with its 1px <c>border-subtle-01</c> border,
    /// in the page flow instead of over a scrim, so documentation and design reviews can show it next to other examples.
    /// <see cref="IsOpen"/> is ignored while it is set. Never use it in a product: a real modal blocks the whole window.
    /// </summary>
    public bool IsInline
    {
        get => (bool)GetValue(IsInlineProperty);
        set => SetValue(IsInlineProperty, value);
    }

    /// <summary>The element that opened the modal; focus returns to it on close (<c>launcherButtonRef</c> / <c>triggerButtonRef</c>).</summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>The element that takes focus when the modal opens (<c>selectorPrimaryFocus</c>). Each modal has its own Carbon default.</summary>
    public VisualElement? InitialFocusElement
    {
        get => (VisualElement?)GetValue(InitialFocusElementProperty);
        set => SetValue(InitialFocusElementProperty, value);
    }

    /// <summary>Runs when the modal is asked to close, after <see cref="RequestClose"/> unless a handler cancelled it.</summary>
    public ICommand? RequestCloseCommand
    {
        get => (ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }

    /// <summary>The inner composed modal (for tests).</summary>
    internal CdsComposedModal Modal => _modal;

    /// <summary>The header part.</summary>
    internal CdsProductModalHeader Header => _header ?? throw new InvalidOperationException("SetParts was not called.");

    /// <summary>The body part.</summary>
    internal CdsModalBody BodyPart => _body ?? throw new InvalidOperationException("SetParts was not called.");

    /// <summary>The footer part, if the modal has one.</summary>
    internal CdsModalFooter? Footer => _footer;

    /// <summary>The inline frame (for tests).</summary>
    internal Border InlineFrame => _frame;

    /// <summary>Whether the modal can be seen: open over the page or drawn inline.</summary>
    private protected bool IsShowing => IsInline || (IsOpen && _modal.IsOpen);

    /// <summary>Hands the parts to the modal; called once from the constructor of the derived modal.</summary>
    private protected void SetParts(CdsProductModalHeader header, CdsModalBody body, CdsModalFooter? footer)
    {
        _header = header;
        _body = body;
        _footer = footer;
        _modal.Parts.Add(header);
        _modal.Parts.Add(body);
        if (footer is not null)
        {
            _modal.Parts.Add(footer);
            // Inline there is no composed modal to route the secondary button's close request through.
            footer.RequestClose += (_, _) => { if (IsInline) RequestCloseFromChrome(); };
        }
        header.CloseButtonClicked += (_, _) => { if (IsInline) RequestCloseFromChrome(); };
        UpdatePlacement();
    }

    /// <summary>The element focused on open when the app set no <see cref="InitialFocusElement"/> (null: Carbon's modal order).</summary>
    private protected virtual VisualElement? DefaultInitialFocus => null;

    /// <summary>Runs before the modal shows: reset the state the user left behind.</summary>
    private protected virtual void OnOpening()
    {
    }

    /// <summary>Runs after the modal closed.</summary>
    private protected virtual void OnClosed()
    {
    }

    /// <summary>
    /// Asks to close from the modal's own chrome (a footer button that means close): raises <see cref="RequestClose"/>
    /// and closes unless a handler cancelled. Refused while a footer submit is loading.
    /// </summary>
    private protected bool RequestCloseFromChrome()
    {
        if (!IsInline && _modal.IsOpen) return _modal.TryClose();
        var args = new CdsModalRequestCloseEventArgs();
        RaiseRequestClose(args);
        if (args.Cancel) return false;
        if (!IsInline) IsOpen = false;
        return true;
    }

    /// <summary>Moves focus to <paramref name="target"/> while the modal is open over the page.</summary>
    private protected void FocusWhileOpen(VisualElement? target)
    {
        if (target is null || IsInline || !_modal.IsOpen || !target.IsLoaded) return;
        target.Focus();
    }

    /// <summary>Names the dialog (the header title by default; screen readers read it on open).</summary>
    private protected void SetAccessibleName(string? name) => CdsSemantics.SetGroupName(_frame, name);

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetBrush(_frame, Border.StrokeProperty, CdsModalTokens.ContainerBorder);
        SetColor(_frame, BackgroundColorProperty, CdsModalTokens.ContainerFill);
    }

    void RaiseRequestClose(CdsModalRequestCloseEventArgs args)
    {
        RequestClose?.Invoke(this, args);
        if (!args.Cancel && RequestCloseCommand?.CanExecute(null) == true) RequestCloseCommand.Execute(null);
    }

    void OnIsOpenChanged(bool open)
    {
        if (open)
        {
            if (IsInline) return;
            OnOpening();
            _modal.InitialFocusElement = InitialFocusElement ?? DefaultInitialFocus;
            _modal.LauncherElement = LauncherElement;
            _modal.IsOpen = true;
        }
        else
        {
            var wasOpen = _modal.IsOpen;
            _modal.IsOpen = false;
            if (wasOpen) OnClosed();
        }
    }

    void UpdatePlacement()
    {
        var inline = IsInline;
        if (inline && _modal.IsOpen) _modal.IsOpen = false;
        _frame.IsVisible = inline;
        InputTransparent = !inline;
        if (inline) ClearValue(AutomationProperties.IsInAccessibleTreeProperty);
        else AutomationProperties.SetIsInAccessibleTree(this, false);
        if (inline == _partsInline || _header is null) return;
        _partsInline = inline;
        if (inline)
        {
            _framePanel.Add(_header);
            if (_body is not null) _framePanel.Add(_body);
            if (_footer is not null) _framePanel.Add(_footer);
        }
        else
        {
            _framePanel.Clear();
        }
    }
}
