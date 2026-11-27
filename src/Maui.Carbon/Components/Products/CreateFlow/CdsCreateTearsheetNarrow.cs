namespace Maui.Carbon;

/// <summary>
/// A one-step create form in a narrow tearsheet (Carbon for IBM Products' <c>CreateTearsheetNarrow</c>): an optional
/// form title and description over the fields, and a primary and a secondary button. Use it to create something simple
/// that needs more room than a modal; for many steps use <see cref="CdsCreateTearsheet"/>.
/// </summary>
public class CdsCreateTearsheetNarrow : CdsTearsheetShell
{
    /// <summary>Bindable property for <see cref="FormTitle"/>.</summary>
    public static readonly BindableProperty FormTitleProperty = BindableProperty.Create(
        nameof(FormTitle), typeof(string), typeof(CdsCreateTearsheetNarrow), null,
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheetNarrow)b).OnPartsChanged());

    /// <summary>A heading over the fields.</summary>
    public string? FormTitle
    {
        get => (string?)GetValue(FormTitleProperty);
        set => SetValue(FormTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="FormDescription"/>.</summary>
    public static readonly BindableProperty FormDescriptionProperty = BindableProperty.Create(
        nameof(FormDescription), typeof(string), typeof(CdsCreateTearsheetNarrow), null,
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheetNarrow)b).OnPartsChanged());

    /// <summary>One or two sentences over the fields.</summary>
    public string? FormDescription
    {
        get => (string?)GetValue(FormDescriptionProperty);
        set => SetValue(FormDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsCreateTearsheetNarrow), "Create",
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheetNarrow)b).SyncButtons());

    /// <summary>The primary (submit) button label: the action ("Create topic").</summary>
    public string PrimaryButtonText
    {
        get => (string)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsCreateTearsheetNarrow), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheetNarrow)b).SyncButtons());

    /// <summary>The secondary button label; it closes the tearsheet.</summary>
    public string SecondaryButtonText
    {
        get => (string)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsSubmitEnabled"/>.</summary>
    public static readonly BindableProperty IsSubmitEnabledProperty = BindableProperty.Create(
        nameof(IsSubmitEnabled), typeof(bool), typeof(CdsCreateTearsheetNarrow), true,
        propertyChanged: static (b, _, _) => ((CdsCreateTearsheetNarrow)b).SyncButtons());

    /// <summary>Enables the primary button; turn it off until the form is valid (Carbon: <c>disableSubmit</c>).</summary>
    public bool IsSubmitEnabled
    {
        get => (bool)GetValue(IsSubmitEnabledProperty);
        set => SetValue(IsSubmitEnabledProperty, value);
    }

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(System.Windows.Input.ICommand), typeof(CdsCreateTearsheetNarrow), null);

    /// <summary>Runs when the primary button is activated, after <see cref="RequestSubmit"/>.</summary>
    public System.Windows.Input.ICommand? RequestSubmitCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }


    readonly CdsButton _primary = new("Create", CdsButtonKind.Primary);
    readonly CdsButton _secondary = new("Cancel", CdsButtonKind.Secondary);

    /// <summary>Creates a closed narrow create tearsheet.</summary>
    public CdsCreateTearsheetNarrow()
    {
        Actions.Add(_primary);
        Actions.Add(_secondary);
        _primary.Clicked += (_, _) =>
        {
            RequestSubmit?.Invoke(this, EventArgs.Empty);
            if (RequestSubmitCommand?.CanExecute(null) == true) RequestSubmitCommand.Execute(null);
        };
        _secondary.Clicked += (_, _) => TryClose();
    }

    /// <summary>Raised when the primary button is activated. Validate, create, then set <see cref="CdsTearsheetShell.IsOpen"/> to false.</summary>
    public event EventHandler? RequestSubmit;

    private protected override bool IsWide => false;

    private protected override View? ComposeBody(View? body) => CreateFormLayout.Compose(FormTitle, FormDescription, body, CdsSpacing.Spacing05);

    void SyncButtons()
    {
        _primary.Text = PrimaryButtonText;
        _primary.IsEnabled = IsSubmitEnabled;
        _secondary.Text = SecondaryButtonText;
    }
}

/// <summary>The form title, description and fields of the one-step create components.</summary>
internal static class CreateFormLayout
{
    public static View Compose(string? title, string? description, View? fields, double padding)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(padding, padding, padding, padding) };
        if (!string.IsNullOrEmpty(title))
        {
            var heading = new CdsText { Text = title, TypeStyle = CdsTypeStyle.HeadingCompact02, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
            SemanticProperties.SetHeadingLevel(heading, SemanticHeadingLevel.Level3);
            stack.Add(heading);
        }
        if (!string.IsNullOrEmpty(description))
            stack.Add(new CdsText { Text = description, TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.WordWrap });
        if (fields is not null)
        {
            if (fields.Parent is Layout old) old.Remove(fields);
            fields.Margin = new Thickness(fields.Margin.Left, Math.Max(fields.Margin.Top, CdsSpacing.Spacing05), fields.Margin.Right, fields.Margin.Bottom);
            stack.Add(fields);
        }
        return stack;
    }
}
