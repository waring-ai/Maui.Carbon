using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsModal
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsModal), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsModal)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="Heading"/>.</summary>
    public static readonly BindableProperty HeadingProperty = BindableProperty.Create(
        nameof(Heading), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsModalSize), typeof(CdsModal), CdsModalSize.Md,
        propertyChanged: static (b, _, _) => ((CdsModal)b).Reshow());

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="TertiaryButtonText"/>.</summary>
    public static readonly BindableProperty TertiaryButtonTextProperty = BindableProperty.Create(
        nameof(TertiaryButtonText), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsDanger"/>.</summary>
    public static readonly BindableProperty IsDangerProperty = BindableProperty.Create(
        nameof(IsDanger), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsPassive"/>.</summary>
    public static readonly BindableProperty IsPassiveProperty = BindableProperty.Create(
        nameof(IsPassive), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).Reshow());

    /// <summary>Bindable property for <see cref="PreventCloseOnClickOutside"/>.</summary>
    public static readonly BindableProperty PreventCloseOnClickOutsideProperty = BindableProperty.Create(
        nameof(PreventCloseOnClickOutside), typeof(bool), typeof(CdsModal), false);

    /// <summary>Bindable property for <see cref="PrimaryButtonDisabled"/>.</summary>
    public static readonly BindableProperty PrimaryButtonDisabledProperty = BindableProperty.Create(
        nameof(PrimaryButtonDisabled), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="HasScrollingContent"/>.</summary>
    public static readonly BindableProperty HasScrollingContentProperty = BindableProperty.Create(
        nameof(HasScrollingContent), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="HasForm"/>.</summary>
    public static readonly BindableProperty HasFormProperty = BindableProperty.Create(
        nameof(HasForm), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsFullWidth"/>.</summary>
    public static readonly BindableProperty IsFullWidthProperty = BindableProperty.Create(
        nameof(IsFullWidth), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="CloseButtonLabel"/>.</summary>
    public static readonly BindableProperty CloseButtonLabelProperty = BindableProperty.Create(
        nameof(CloseButtonLabel), typeof(string), typeof(CdsModal), "Close",
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsModal), null);

    /// <summary>Bindable property for <see cref="InitialFocusElement"/>.</summary>
    public static readonly BindableProperty InitialFocusElementProperty = BindableProperty.Create(
        nameof(InitialFocusElement), typeof(VisualElement), typeof(CdsModal), null);

    /// <summary>Bindable property for <see cref="LoadingStatus"/>.</summary>
    public static readonly BindableProperty LoadingStatusProperty = BindableProperty.Create(
        nameof(LoadingStatus), typeof(CdsInlineLoadingStatus), typeof(CdsModal), CdsInlineLoadingStatus.Inactive,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="LoadingDescription"/>.</summary>
    public static readonly BindableProperty LoadingDescriptionProperty = BindableProperty.Create(
        nameof(LoadingDescription), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="LoadingIconDescription"/>.</summary>
    public static readonly BindableProperty LoadingIconDescriptionProperty = BindableProperty.Create(
        nameof(LoadingIconDescription), typeof(string), typeof(CdsModal), null,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsAlert"/>.</summary>
    public static readonly BindableProperty IsAlertProperty = BindableProperty.Create(
        nameof(IsAlert), typeof(bool), typeof(CdsModal), false,
        propertyChanged: static (b, _, _) => ((CdsModal)b).SyncParts());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsModal), null);

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(ICommand), typeof(CdsModal), null);

    /// <summary>Open state (two-way). Setting it to <c>true</c> shows the modal once the page is loaded; closing it from the UI sets it to <c>false</c>.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The heading (<c>modalHeading</c>): a short verb phrase that matches the button that opened the modal.</summary>
    public string? Heading
    {
        get => (string?)GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    /// <summary>Small <c>label-01</c> text above the heading (<c>modalLabel</c>).</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Width step: xs, sm, md (default) or lg. Use xs and sm for short messages and lg for complex content.</summary>
    public CdsModalSize Size
    {
        get => (CdsModalSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Primary button label: a specific verb ("Create key"), never "OK". Leave empty (with no secondary) for no footer.</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Secondary button label ("Cancel"), left of the primary. It closes the modal unless <see cref="SecondarySubmit"/> has a handler.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Label of an optional third button at the far left (the web version's second <c>secondaryButtons</c> entry). It raises <see cref="TertiarySubmit"/>.</summary>
    public string? TertiaryButtonText
    {
        get => (string?)GetValue(TertiaryButtonTextProperty);
        set => SetValue(TertiaryButtonTextProperty, value);
    }

    /// <summary>Destructive confirmation: the primary button is a danger button and focus starts on the secondary (Cancel) button.</summary>
    public bool IsDanger
    {
        get => (bool)GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>A passive modal shows information only: no footer, and a tap outside closes it.</summary>
    public bool IsPassive
    {
        get => (bool)GetValue(IsPassiveProperty);
        set => SetValue(IsPassiveProperty, value);
    }

    /// <summary>Keep a passive modal open when the scrim is tapped. Transactional modals never close on a scrim tap.</summary>
    public bool PreventCloseOnClickOutside
    {
        get => (bool)GetValue(PreventCloseOnClickOutsideProperty);
        set => SetValue(PreventCloseOnClickOutsideProperty, value);
    }

    /// <summary>Disables the primary button (for example until the form is valid).</summary>
    public bool PrimaryButtonDisabled
    {
        get => (bool)GetValue(PrimaryButtonDisabledProperty);
        set => SetValue(PrimaryButtonDisabledProperty, value);
    }

    /// <summary>Always draw the scrolling treatment (bottom fade), even when the content fits.</summary>
    public bool HasScrollingContent
    {
        get => (bool)GetValue(HasScrollingContentProperty);
        set => SetValue(HasScrollingContentProperty, value);
    }

    /// <summary>The body holds form fields, so it runs the full width instead of wrapping text at 80%.</summary>
    public bool HasForm
    {
        get => (bool)GetValue(HasFormProperty);
        set => SetValue(HasFormProperty, value);
    }

    /// <summary>Remove the body padding so content runs edge to edge (<c>isFullWidth</c>).</summary>
    public bool IsFullWidth
    {
        get => (bool)GetValue(IsFullWidthProperty);
        set => SetValue(IsFullWidthProperty, value);
    }

    /// <summary>Accessible name and tooltip of the close button (<c>closeButtonLabel</c>). Default <c>Close</c>.</summary>
    public string CloseButtonLabel
    {
        get => (string)GetValue(CloseButtonLabelProperty);
        set => SetValue(CloseButtonLabelProperty, value);
    }

    /// <summary>
    /// The element that opened the modal; focus returns to it on close (<c>launcherButtonRef</c>). When it is not set,
    /// focus returns to the Carbon control that was focused when the modal opened, if it is still on the page.
    /// </summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>The element that takes focus when the modal opens (<c>selectorPrimaryFocus</c>). Default: Cancel in a danger modal, else the first field, else the primary button.</summary>
    public VisualElement? InitialFocusElement
    {
        get => (VisualElement?)GetValue(InitialFocusElementProperty);
        set => SetValue(InitialFocusElementProperty, value);
    }

    /// <summary>
    /// The submit wait (<c>loadingStatus</c>): Active, Finished or Error shows a <see cref="CdsInlineLoading"/> with
    /// <see cref="LoadingDescription"/> in place of the primary label and disables the footer buttons, so the modal can
    /// stay open while the request runs and the submit cannot run twice. Default <see cref="CdsInlineLoadingStatus.Inactive"/>.
    /// For short waits only; show a <see cref="CdsLoading"/> overlay for long ones. Close the modal on
    /// <see cref="LoadingSuccess"/>, or set the status back to Inactive after an error and show the error inline.
    /// </summary>
    public CdsInlineLoadingStatus LoadingStatus
    {
        get => (CdsInlineLoadingStatus)GetValue(LoadingStatusProperty);
        set => SetValue(LoadingStatusProperty, value);
    }

    /// <summary>The loading text in place of the primary label (<c>loadingDescription</c>): "Creating key...", then "Key created".</summary>
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

    /// <summary>
    /// An alert dialog (<c>alert</c>, the <c>alertdialog</c> role) for interruptive confirmations such as an expiring
    /// session or discarding changes: the dialog is described by its body text, and opening it announces the heading and
    /// that text assertively.
    /// </summary>
    public bool IsAlert
    {
        get => (bool)GetValue(IsAlertProperty);
        set => SetValue(IsAlertProperty, value);
    }

    /// <summary>Runs when the primary button is activated.</summary>
    public ICommand? RequestSubmitCommand
    {
        get => (ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }

    /// <summary>Runs when the modal is asked to close (close button, Escape, secondary button, scrim tap).</summary>
    public ICommand? RequestCloseCommand
    {
        get => (ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }
}
