using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The modal of the export pattern (Carbon for IBM Products <c>ExportModal</c>): the person names the file, or picks one of
/// a few formats as radio buttons, and exports. Set <see cref="ValidExtensions"/> to require an extension: the field turns
/// invalid once it is left without one. The modal stays open after <see cref="RequestSubmit"/> and walks through
/// <see cref="IsLoading"/>, then <see cref="IsSuccessful"/> or <see cref="IsError"/>: the fields and footer give way to a
/// status message, and the close button ends the flow. It never closes on a tap outside.
/// </summary>
public partial class CdsExportModal : CdsProductModal
{
    readonly CdsProductModalHeader _header = new(CdsProductModalHeaderStyle.Standard);
    readonly CdsModalFooter _footer = new();
    readonly CdsEndInset _bodyBox;
    readonly CdsText _bodyText;
    readonly CdsRadioButtonGroup _formats = new() { Orientation = CdsChoiceOrientation.Vertical, IsVisible = false };
    readonly CdsTextInput _textInput = new() { IsVisible = false };
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly CdsInlineLoading _loading = new() { IsVisible = false };
    readonly Grid _successRow;
    readonly CdsText _successText;
    readonly Grid _errorRow;
    readonly CdsText _errorText;
    readonly CdsItemCollection<string> _validExtensions = [];
    readonly CdsItemCollection<CdsExportExtension> _extensions = [];
    CdsPasswordInput? _passwordInput;
    string? _name;
    string? _extension;
    bool _dirty;
    bool _syncingFormats;
    string? _announced;
    bool _focusedClose;

    /// <summary>Creates a closed export modal.</summary>
    public CdsExportModal() : base(CdsModalSize.Sm, preventCloseOnClickOutside: true)
    {
        (_bodyBox, _bodyText) = CdsProductModalParts.Paragraph(CdsTypeStyle.Body01, ProductModalModel.TwentyPercentLessSpacing05,
            new Thickness(0, 0, 0, CdsSpacing.Spacing05));
        (_successRow, _, _successText) = CdsProductModalParts.MessageRow(CdsIconName.CheckmarkFilled, CdsProductModalTokens.SuccessIcon, CdsTypeStyle.Body01);
        (_errorRow, _, _errorText) = CdsProductModalParts.MessageRow(CdsIconName.ErrorFilled, CdsProductModalTokens.ErrorIcon, CdsTypeStyle.Body01);
        _stack.Add(_bodyBox);
        _stack.Add(_formats);
        _stack.Add(_textInput);
        _stack.Add(_loading);
        _stack.Add(_successRow);
        _stack.Add(_errorRow);

        WireInput(_textInput);
        _formats.SelectionChanged += (_, e) =>
        {
            if (_syncingFormats) return;
            _extension = e.NewValue as string;
        };
        _validExtensions.CollectionChanged += (_, _) => Sync();
        _extensions.CollectionChanged += OnExtensionsChanged;
        _footer.RequestSubmit += (_, _) => Submit();
        SetParts(_header, new CdsModalBody { HasForm = true, Body = _stack }, _footer);
        ResetInput();
        Sync();
    }

    /// <summary>
    /// Raised with the file name when the primary button is activated (<c>onRequestSubmit</c>). The modal stays open:
    /// set <see cref="IsLoading"/>, then <see cref="IsSuccessful"/> or <see cref="IsError"/>.
    /// </summary>
    public event EventHandler<CdsExportModalSubmitEventArgs>? RequestSubmit;

    /// <summary>
    /// The extensions a typed name may end with, without the dot (<c>validExtensions</c>): "pdf". Empty (the default)
    /// accepts any name.
    /// </summary>
    public IList<string> ValidExtensions => _validExtensions;

    /// <summary>
    /// Formats offered as radio buttons instead of the name input (<c>preformattedExtensions</c>). The first is chosen
    /// when the modal opens; the exported name is <see cref="Filename"/> plus the chosen extension in lowercase.
    /// </summary>
    public IList<CdsExportExtension> PreformattedExtensions => _extensions;

    /// <summary>The name in the input (for tests).</summary>
    internal string? CurrentName => _name;

    /// <summary>The chosen format (for tests).</summary>
    internal string? CurrentExtension => _extension;

    /// <summary>The name input in use (for tests).</summary>
    internal CdsTextInput NameInput => InputType == CdsExportModalInputType.Password ? EnsurePasswordInput() : _textInput;

    /// <summary>The format radio buttons (for tests).</summary>
    internal CdsRadioButtonGroup FormatGroup => _formats;

    private protected override VisualElement? DefaultInitialFocus => _extensions.Count > 0 ? _formats : NameInput;

    private protected override void OnOpening()
    {
        ResetInput();
        Sync();
    }

    /// <summary>Exports under the current name, as the primary button does (for tests).</summary>
    internal void Submit()
    {
        if (ExportModalModel.IsPrimaryDisabled(IsLoading, _name, HasInvalidExtension)) return;
        var name = ExportModalModel.SubmitName(Filename, _name, _extension);
        RequestSubmit?.Invoke(this, new CdsExportModalSubmitEventArgs(name));
        if (RequestSubmitCommand?.CanExecute(name) == true) RequestSubmitCommand.Execute(name);
    }

    /// <summary>Marks the name as edited and left, as leaving the field does (for tests).</summary>
    internal void MarkDirty()
    {
        _dirty = true;
        Sync();
    }

    bool HasInvalidExtension => _extensions.Count == 0 && ExportModalModel.HasInvalidExtension(_name, _validExtensions, _dirty);

    void WireInput(CdsTextInput input)
    {
        input.TextChanged += (_, e) =>
        {
            if (!ReferenceEquals(input, NameInput)) return;
            _name = e.NewTextValue;
            Sync();
        };
        input.FocusLost += (_, _) =>
        {
            _dirty = true;
            Sync();
        };
    }

    CdsPasswordInput EnsurePasswordInput()
    {
        if (_passwordInput is not null) return _passwordInput;
        _passwordInput = new CdsPasswordInput { IsVisible = false, Text = _name };
        WireInput(_passwordInput);
        _stack.Insert(_stack.IndexOf(_textInput) + 1, _passwordInput);
        return _passwordInput;
    }

    void OnFilenameChanged()
    {
        ResetInput();
        Sync();
    }

    void OnExtensionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _syncingFormats = true;
        try
        {
            _formats.Items.Clear();
            foreach (var extension in _extensions)
                _formats.Items.Add(new CdsRadioButton(ExportModalModel.ExtensionLabel(extension), extension.Extension));
        }
        finally
        {
            _syncingFormats = false;
        }
        ResetInput();
        Sync();
    }

    void ResetInput()
    {
        _name = Filename;
        _dirty = false;
        _extension = ExportModalModel.DefaultExtension(_extensions);
        _syncingFormats = true;
        try
        {
            _formats.SelectedValue = _extension;
        }
        finally
        {
            _syncingFormats = false;
        }
    }

    void Sync()
    {
        _header.Title = Title;
        SetAccessibleName(Title);

        var submitted = ExportModalModel.IsSubmitted(IsLoading, IsError, IsSuccessful);
        var hasFormats = _extensions.Count > 0;
        _bodyText.Text = BodyText;
        _bodyBox.IsVisible = !submitted && !string.IsNullOrEmpty(BodyText);

        _formats.Legend = PreformattedExtensionsLabel;
        _formats.IsVisible = !submitted && hasFormats;

        var password = InputType == CdsExportModalInputType.Password;
        var input = NameInput;
        var other = password ? _textInput : _passwordInput;
        if (other is not null) other.IsVisible = false;
        input.IsVisible = !submitted && !hasFormats;
        input.Label = InputLabel;
        if (input.Text != _name) input.Text = _name;
        var invalid = HasInvalidExtension;
        input.IsInvalid = invalid;
        input.InvalidText = InvalidInputText;
        if (_passwordInput is not null)
        {
            _passwordInput.ShowPasswordLabel = ShowPasswordLabel;
            _passwordInput.HidePasswordLabel = HidePasswordLabel;
        }

        _loading.IsVisible = IsLoading;
        _loading.Status = IsLoading ? CdsInlineLoadingStatus.Active : CdsInlineLoadingStatus.Inactive;
        _loading.Description = LoadingMessage;
        _successText.Text = SuccessMessage;
        _successRow.IsVisible = IsSuccessful && !IsLoading;
        _errorText.Text = ErrorMessage;
        _errorRow.IsVisible = IsError && !IsLoading && !IsSuccessful;

        // After a submit the footer goes away: the close button ends the flow.
        _footer.PrimaryButtonText = submitted ? null : PrimaryButtonText;
        _footer.SecondaryButtonText = submitted ? null : SecondaryButtonText;
        _footer.PrimaryButtonDisabled = ExportModalModel.IsPrimaryDisabled(IsLoading, _name, invalid);

        AnnounceStatus();
        // Success moves focus to the close button, once: it is the way out of the finished flow.
        if (IsSuccessful && !_focusedClose) FocusWhileOpen(_header.CloseTarget);
        _focusedClose = IsSuccessful;
    }

    // The messaging area is a polite live region: say each new status once while the modal is shown.
    void AnnounceStatus()
    {
        var status = IsLoading ? LoadingMessage : IsSuccessful ? SuccessMessage : IsError ? ErrorMessage : null;
        if (status == _announced) return;
        _announced = status;
        if (IsShowing && IsLoaded && !string.IsNullOrEmpty(status) && !IsLoading) CdsAnnouncer.Announce(status);
    }
}
