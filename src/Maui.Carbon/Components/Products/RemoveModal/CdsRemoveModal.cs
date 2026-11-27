namespace Maui.Carbon;

/// <summary>
/// Confirms removing or deleting a resource (Carbon for IBM Products <c>RemoveModal</c>): a small danger modal with the
/// resource in its label, a message saying what is lost, Cancel and a danger button. Medium-impact removals confirm with
/// the danger button alone (focus starts on Cancel); for high-impact ones set <see cref="RequiresTextConfirmation"/>: the
/// person types <see cref="ResourceName"/> before the danger button enables. Use "Remove" when the resource survives
/// elsewhere and "Delete" when it is destroyed. The modal stays open after <see cref="RequestSubmit"/>: remove the
/// resource, then set <see cref="CdsProductModal.IsOpen"/> to false.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class CdsRemoveModal : CdsProductModal
{
    readonly CdsProductModalHeader _header = new(CdsProductModalHeaderStyle.Standard);
    readonly CdsModalFooter _footer = new() { IsDanger = true };
    readonly CdsEndInset _bodyBox;
    readonly CdsText _bodyText;
    readonly ContentView _custom = new() { IsVisible = false };
    readonly CdsTextInput _input = new() { IsVisible = false };
    bool _leftInput;

    /// <summary>Creates a closed remove modal.</summary>
    public CdsRemoveModal() : base(CdsModalSize.Sm, preventCloseOnClickOutside: true)
    {
        // __body: margin-bottom 16, 20% at the end.
        (_bodyBox, _bodyText) = CdsProductModalParts.Paragraph(CdsTypeStyle.Body01, ProductModalModel.TwentyPercent,
            new Thickness(0, 0, 0, CdsSpacing.Spacing05));
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_bodyBox);
        stack.Add(_custom);
        stack.Add(_input);

        _input.TextChanged += (_, e) => ConfirmationText = e.NewTextValue;
        _input.FocusLost += (_, _) =>
        {
            _leftInput = true;
            Sync();
        };
        _footer.RequestSubmit += (_, _) =>
        {
            if (_footer.PrimaryButtonDisabled) return;
            RequestSubmit?.Invoke(this, EventArgs.Empty);
            if (RequestSubmitCommand?.CanExecute(null) == true) RequestSubmitCommand.Execute(null);
        };
        SetParts(_header, new CdsModalBody { HasForm = true, Body = stack }, _footer);
        Sync();
    }

    /// <summary>
    /// Raised when the danger button is activated (<c>onRequestSubmit</c>). The modal stays open: remove the resource,
    /// then close it, or keep it open and say what failed.
    /// </summary>
    public event EventHandler? RequestSubmit;

    /// <summary>The confirmation input (for tests).</summary>
    internal CdsTextInput ConfirmationInput => _input;

    private protected override VisualElement? DefaultInitialFocus => RequiresTextConfirmation ? _input : null;

    private protected override void OnOpening()
    {
        _leftInput = false;
        ConfirmationText = string.Empty;
    }

    void Sync()
    {
        _header.Title = Title;
        _header.Label = Label;
        _header.IconDescription = IconDescription;
        SetAccessibleName(Title);

        _bodyText.Text = BodyText;
        _bodyBox.IsVisible = Body is null && !string.IsNullOrEmpty(BodyText);
        _custom.Content = Body;
        _custom.IsVisible = Body is not null;
        // A custom body keeps the paragraph's 16 to the input.
        _custom.Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05);

        var confirm = RequiresTextConfirmation;
        _input.IsVisible = confirm;
        _input.Label = InputLabel;
        _input.Placeholder = InputPlaceholder;
        if (_input.Text != ConfirmationText) _input.Text = ConfirmationText;
        var invalid = RemoveModalModel.ShowsInvalid(confirm, _leftInput, ConfirmationText, ResourceName);
        _input.IsInvalid = invalid && !string.IsNullOrEmpty(InputInvalidText);
        _input.InvalidText = InputInvalidText;

        _footer.PrimaryButtonText = PrimaryButtonText;
        _footer.SecondaryButtonText = SecondaryButtonText;
        _footer.PrimaryButtonDisabled = RemoveModalModel.IsPrimaryDisabled(PrimaryButtonDisabled, confirm, ConfirmationText, ResourceName);
        if (_footer.Buttons.LastOrDefault() is CdsModalButton primary && primary.Text == PrimaryButtonText)
            SemanticProperties.SetHint(primary.HitTarget, PrimaryDangerDescription);
    }
}
