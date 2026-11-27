namespace Maui.Carbon;

/// <summary>
/// A small modal for creating a simple resource quickly (Carbon for IBM Products <c>CreateModal</c>): a title with an
/// optional subtitle over a rule, a description and a short form, Cancel and a primary button. It has no close button
/// and never closes on a tap outside, so a half-filled form is not lost; Escape and Cancel ask to close. Keep
/// <see cref="PrimaryButtonDisabled"/> set until the required fields are valid. The modal stays open after
/// <see cref="RequestSubmit"/>: create the resource, then set <see cref="CdsProductModal.IsOpen"/> to false. Use a
/// tearsheet for a long or multi-step creation flow.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class CdsCreateModal : CdsProductModal
{
    readonly CdsProductModalHeader _header = new(CdsProductModalHeaderStyle.Create);
    readonly CdsModalFooter _footer = new();
    readonly CdsEndInset _descriptionBox;
    readonly CdsText _description;
    readonly ContentView _form = new();

    /// <summary>Creates a closed create modal.</summary>
    public CdsCreateModal() : base(CdsModalSize.Sm, preventCloseOnClickOutside: true)
    {
        // __description: body-01, margin 8 0 16, calc(20% - 1rem) at the end.
        (_descriptionBox, _description) = CdsProductModalParts.Paragraph(CdsTypeStyle.Body01, ProductModalModel.TwentyPercentLessSpacing05,
            new Thickness(0, CdsSpacing.Spacing03, 0, CdsSpacing.Spacing05));
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_descriptionBox);
        stack.Add(_form);
        _footer.RequestSubmit += (_, _) =>
        {
            if (PrimaryButtonDisabled) return;
            RequestSubmit?.Invoke(this, EventArgs.Empty);
            if (RequestSubmitCommand?.CanExecute(null) == true) RequestSubmitCommand.Execute(null);
        };
        SetParts(_header, new CdsModalBody { HasForm = true, Body = stack }, _footer);
        Sync();
    }

    /// <summary>
    /// Raised when the primary button is activated (<c>onRequestSubmit</c>). The modal stays open: validate and create,
    /// then close it, or show the errors on the fields.
    /// </summary>
    public event EventHandler? RequestSubmit;

    void Sync()
    {
        _header.Title = Title;
        _header.Subtitle = Subtitle;
        SetAccessibleName(Title);
        _description.Text = Description;
        _descriptionBox.IsVisible = !string.IsNullOrEmpty(Description);
        _form.Content = Body;
        _footer.PrimaryButtonText = PrimaryButtonText;
        _footer.SecondaryButtonText = SecondaryButtonText;
        _footer.PrimaryButtonDisabled = PrimaryButtonDisabled;
    }
}
