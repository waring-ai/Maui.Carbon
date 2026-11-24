using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsDialog
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsDialog), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsDialog)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="IsModal"/>.</summary>
    public static readonly BindableProperty IsModalProperty = BindableProperty.Create(
        nameof(IsModal), typeof(bool), typeof(CdsDialog), false,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).Reshow());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsModalSize), typeof(CdsDialog), CdsModalSize.Md,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).Reshow());

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsDialog), null,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsDialog), null,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsDialog), "Save",
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsDialog), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="TertiaryButtonText"/>.</summary>
    public static readonly BindableProperty TertiaryButtonTextProperty = BindableProperty.Create(
        nameof(TertiaryButtonText), typeof(string), typeof(CdsDialog), null,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsDanger"/>.</summary>
    public static readonly BindableProperty IsDangerProperty = BindableProperty.Create(
        nameof(IsDanger), typeof(bool), typeof(CdsDialog), false,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsPassive"/>.</summary>
    public static readonly BindableProperty IsPassiveProperty = BindableProperty.Create(
        nameof(IsPassive), typeof(bool), typeof(CdsDialog), false,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).Reshow());

    /// <summary>Bindable property for <see cref="PrimaryButtonDisabled"/>.</summary>
    public static readonly BindableProperty PrimaryButtonDisabledProperty = BindableProperty.Create(
        nameof(PrimaryButtonDisabled), typeof(bool), typeof(CdsDialog), false,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="HasScrollingContent"/>.</summary>
    public static readonly BindableProperty HasScrollingContentProperty = BindableProperty.Create(
        nameof(HasScrollingContent), typeof(bool), typeof(CdsDialog), false,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="CloseButtonLabel"/>.</summary>
    public static readonly BindableProperty CloseButtonLabelProperty = BindableProperty.Create(
        nameof(CloseButtonLabel), typeof(string), typeof(CdsDialog), "Close",
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="Footer"/>.</summary>
    public static readonly BindableProperty FooterProperty = BindableProperty.Create(
        nameof(Footer), typeof(View), typeof(CdsDialog), null,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).Reshow());

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsDialog), null);

    /// <summary>Bindable property for <see cref="InitialFocusElement"/>.</summary>
    public static readonly BindableProperty InitialFocusElementProperty = BindableProperty.Create(
        nameof(InitialFocusElement), typeof(VisualElement), typeof(CdsDialog), null);

    /// <summary>Bindable property for <see cref="LoadingStatus"/>.</summary>
    public static readonly BindableProperty LoadingStatusProperty = BindableProperty.Create(
        nameof(LoadingStatus), typeof(CdsInlineLoadingStatus), typeof(CdsDialog), CdsInlineLoadingStatus.Inactive,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="LoadingDescription"/>.</summary>
    public static readonly BindableProperty LoadingDescriptionProperty = BindableProperty.Create(
        nameof(LoadingDescription), typeof(string), typeof(CdsDialog), null,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="LoadingIconDescription"/>.</summary>
    public static readonly BindableProperty LoadingIconDescriptionProperty = BindableProperty.Create(
        nameof(LoadingIconDescription), typeof(string), typeof(CdsDialog), null,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="IsAlert"/>.</summary>
    public static readonly BindableProperty IsAlertProperty = BindableProperty.Create(
        nameof(IsAlert), typeof(bool), typeof(CdsDialog), false,
        propertyChanged: static (b, _, _) => ((CdsDialog)b).SyncParts());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsDialog), null);

    /// <summary>Bindable property for <see cref="RequestCloseCommand"/>.</summary>
    public static readonly BindableProperty RequestCloseCommandProperty = BindableProperty.Create(
        nameof(RequestCloseCommand), typeof(ICommand), typeof(CdsDialog), null);

    /// <summary>Open state (two-way). Setting it to <c>true</c> shows the dialog once the page is loaded.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Modal: scrim, page blocked, a tap outside requests close. Non-modal (default): a panel docked at the top end that leaves the page usable.</summary>
    public bool IsModal
    {
        get => (bool)GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    /// <summary>Width step: xs, sm, md (default) or lg. A non-modal dialog is 24, 36, 48 or 72rem wide at most.</summary>
    public CdsModalSize Size
    {
        get => (CdsModalSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The title: a short phrase that matches the button or menu item that opened it ("Delete bucket?").</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Small <c>label-01</c> text above the title (<c>DialogSubtitle</c>).</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Primary button label. Default <c>Save</c>; use a specific verb ("Delete").</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Secondary button label. Default <c>Cancel</c>; an empty string hides it (a single button takes the end half).</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Label of an optional third button at the far left (the second <c>secondaryButtons</c> entry). It raises <see cref="TertiarySubmit"/>.</summary>
    public string? TertiaryButtonText
    {
        get => (string?)GetValue(TertiaryButtonTextProperty);
        set => SetValue(TertiaryButtonTextProperty, value);
    }

    /// <summary>Destructive confirmation: a danger primary button, and focus starts on the secondary (Cancel) button.</summary>
    public bool IsDanger
    {
        get => (bool)GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>A passive dialog shows information only: no footer.</summary>
    public bool IsPassive
    {
        get => (bool)GetValue(IsPassiveProperty);
        set => SetValue(IsPassiveProperty, value);
    }

    /// <summary>Disables the primary button.</summary>
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

    /// <summary>Accessible name and tooltip of the close button. Default <c>Close</c>.</summary>
    public string CloseButtonLabel
    {
        get => (string)GetValue(CloseButtonLabelProperty);
        set => SetValue(CloseButtonLabelProperty, value);
    }

    /// <summary>Your own footer content (buttons) instead of the generated ones.</summary>
    public View? Footer
    {
        get => (View?)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// The element focus returns to on close (<c>focusAfterCloseRef</c>). When it is not set, focus returns to the Carbon
    /// control that was focused when the dialog opened, if it is still on the page.
    /// </summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>The element that takes focus when the dialog opens. Default: Cancel in a danger dialog, else the first field, else the primary button.</summary>
    public VisualElement? InitialFocusElement
    {
        get => (VisualElement?)GetValue(InitialFocusElementProperty);
        set => SetValue(InitialFocusElementProperty, value);
    }

    /// <summary>
    /// The submit wait (the footer's <c>loadingStatus</c>): Active, Finished or Error disables the generated footer buttons
    /// and shows a <see cref="CdsInlineLoading"/> with <see cref="LoadingDescription"/> in place of the primary label.
    /// Default <see cref="CdsInlineLoadingStatus.Inactive"/>. Close the dialog on <see cref="LoadingSuccess"/>. Not used
    /// with a custom <see cref="Footer"/>.
    /// </summary>
    public CdsInlineLoadingStatus LoadingStatus
    {
        get => (CdsInlineLoadingStatus)GetValue(LoadingStatusProperty);
        set => SetValue(LoadingStatusProperty, value);
    }

    /// <summary>The loading text in place of the primary label (<c>loadingDescription</c>): "Deleting...", then "Deleted".</summary>
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
    /// An alert dialog (<c>role="alertdialog"</c>) for interruptive confirmations ("Delete bucket?"): the dialog is
    /// described by its body text, and opening it announces the title and that text assertively.
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

    /// <summary>Runs when the dialog is asked to close (close button, Escape, secondary button, scrim tap).</summary>
    public ICommand? RequestCloseCommand
    {
        get => (ICommand?)GetValue(RequestCloseCommandProperty);
        set => SetValue(RequestCloseCommandProperty, value);
    }
}
