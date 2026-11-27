namespace Maui.Carbon;

/// <summary>
/// A one-step create form in a side panel that slides in beside the page (Carbon for IBM Products'
/// <c>CreateSidePanel</c>): the panel title and subtitle, a form title and description over the fields, and a primary
/// and a secondary button. It pushes <see cref="CdsSidePanel.PageContent"/> aside and has no close button; the secondary
/// button closes it. Use it to create something simple while the list it goes into stays in view.
/// </summary>
public class CdsCreateSidePanel : CdsSidePanel
{
    /// <summary>Bindable property for <see cref="FormTitle"/>.</summary>
    public static readonly BindableProperty FormTitleProperty = BindableProperty.Create(
        nameof(FormTitle), typeof(string), typeof(CdsCreateSidePanel), null,
        propertyChanged: static (b, _, _) => ((CdsCreateSidePanel)b).RecomposeBody());

    /// <summary>A heading over the fields.</summary>
    public string? FormTitle
    {
        get => (string?)GetValue(FormTitleProperty);
        set => SetValue(FormTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="FormDescription"/>.</summary>
    public static readonly BindableProperty FormDescriptionProperty = BindableProperty.Create(
        nameof(FormDescription), typeof(string), typeof(CdsCreateSidePanel), null,
        propertyChanged: static (b, _, _) => ((CdsCreateSidePanel)b).RecomposeBody());

    /// <summary>One or two sentences over the fields.</summary>
    public string? FormDescription
    {
        get => (string?)GetValue(FormDescriptionProperty);
        set => SetValue(FormDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsCreateSidePanel), "Create",
        propertyChanged: static (b, _, _) => ((CdsCreateSidePanel)b).SyncButtons());

    /// <summary>The primary (submit) button label: the action ("Create rule").</summary>
    public string PrimaryButtonText
    {
        get => (string)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsCreateSidePanel), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsCreateSidePanel)b).SyncButtons());

    /// <summary>The secondary button label; it closes the panel.</summary>
    public string SecondaryButtonText
    {
        get => (string)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsSubmitEnabled"/>.</summary>
    public static readonly BindableProperty IsSubmitEnabledProperty = BindableProperty.Create(
        nameof(IsSubmitEnabled), typeof(bool), typeof(CdsCreateSidePanel), true,
        propertyChanged: static (b, _, _) => ((CdsCreateSidePanel)b).SyncButtons());

    /// <summary>Enables the primary button; turn it off until the form is valid (Carbon: <c>disableSubmit</c>).</summary>
    public bool IsSubmitEnabled
    {
        get => (bool)GetValue(IsSubmitEnabledProperty);
        set => SetValue(IsSubmitEnabledProperty, value);
    }

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(System.Windows.Input.ICommand), typeof(CdsCreateSidePanel), null);

    /// <summary>Runs when the primary button is activated, after <see cref="RequestSubmit"/>.</summary>
    public System.Windows.Input.ICommand? RequestSubmitCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }


    readonly CdsButton _primary = new("Create", CdsButtonKind.Primary);
    readonly CdsButton _secondary = new("Cancel", CdsButtonKind.Secondary);

    /// <summary>Creates a closed create side panel: medium, slide-in, at the end edge, without a close button.</summary>
    public CdsCreateSidePanel()
    {
        Size = CdsSidePanelSize.Md;
        Placement = CdsSidePanelPlacement.End;
        SlideIn = true;
        AnimateTitle = false;
        HideCloseButton = true;
        Actions.Add(_primary);
        Actions.Add(_secondary);
        _primary.Clicked += (_, _) =>
        {
            RequestSubmit?.Invoke(this, EventArgs.Empty);
            if (RequestSubmitCommand?.CanExecute(null) == true) RequestSubmitCommand.Execute(null);
        };
        _secondary.Clicked += (_, _) => TryClose();
    }

    /// <summary>Raised when the primary button is activated. Validate, create, then set <see cref="CdsSidePanel.IsOpen"/> to false.</summary>
    public event EventHandler? RequestSubmit;

    private protected override View? ComposeBody(View? body) => CreateFormLayout.Compose(FormTitle, FormDescription, body, 0);

    private protected override CdsTextTone SubtitleTone => CdsTextTone.Secondary;

    void SyncButtons()
    {
        _primary.Text = PrimaryButtonText;
        _primary.IsEnabled = IsSubmitEnabled;
        _secondary.Text = SecondaryButtonText;
    }
}
