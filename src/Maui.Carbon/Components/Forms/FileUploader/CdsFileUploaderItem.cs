using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// One file row of a <see cref="CdsFileUploader"/> (usable on its own): the name on the left (ellipsis when long) on
/// <c>layer-01</c>, at most 320 wide, and the status at the end: a spinner while uploading, a remove button while editable
/// and a filled check when complete. An invalid row gets a 2px <c>support-error</c> outline, the error icon beside the
/// remove button and, under a divider, the error title and its supporting text.
/// </summary>
public partial class CdsFileUploaderItem : CdsControl
{
    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly CdsText _name = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing03, 0) };
    readonly HorizontalStackLayout _state = new() { Spacing = 0, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, CdsSpacing.Spacing04, 0) };
    readonly CdsIcon _errorIcon = new(CdsIconName.WarningFilled, 16) { IsVisible = false, Margin = new Thickness(0, 0, CdsSpacing.Spacing02, 0) };
    readonly CdsFileSpinner _spinner = new() { IsVisible = false, Margin = new Thickness(0, 0, CdsSpacing.Spacing02, 0) };
    readonly CdsFieldIconButton _remove = new(CdsIconName.Close, CdsFieldButtonKind.Plain, 24, 24) { IsVisible = false };
    // Status is never color alone: the check is an image named "Complete", like the spinner's "Uploading".
    readonly CdsIcon _complete = new(CdsIconName.CheckmarkFilled, 16) { IsVisible = false, Description = "Complete" };
    readonly VerticalStackLayout _message = new() { Spacing = 0, IsVisible = false };
    readonly BoxView _divider = new() { HeightRequest = 1, InputTransparent = true };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Label01, Margin = new Thickness(CdsSpacing.Spacing05, 0) };
    readonly CdsText _supplement = new() { TypeStyle = CdsTypeStyle.Label01, Margin = new Thickness(CdsSpacing.Spacing05, 0) };
    readonly CdsFocusRing _outline = new(CdsFocusRingStyle.Inset);
    string? _errorText;
    bool _alertPending;
    bool _batching;
    CdsFileStatus? _lastStatus;

    /// <summary>Creates a 48 high row.</summary>
    public CdsFileUploaderItem()
    {
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _outline.SetColorKey(CdsColorKeys.SupportError);
        AutomationProperties.SetIsInAccessibleTree(_outline, false);

        _state.Add(_errorIcon);
        _state.Add(_spinner);
        _state.Add(_remove);
        _state.Add(_complete);
        _message.Add(_divider);
        _message.Add(_title);
        _message.Add(_supplement);
        _root.Add(_name, 0, 0);
        _root.Add(_state, 1, 0);
        _root.Add(_message, 0, 1);
        Grid.SetColumnSpan(_message, 2);
        _root.Add(_outline, 0, 0);
        Grid.SetColumnSpan(_outline, 2);
        Grid.SetRowSpan(_outline, 2);

        _remove.Clicked += (_, _) =>
        {
            Deleted?.Invoke(this, new CdsFileRemovedEventArgs(null));
            if (DeleteCommand?.CanExecute(CommandParameter) == true) DeleteCommand.Execute(CommandParameter);
        };
        HorizontalOptions = LayoutOptions.Fill;
        Loaded += (_, _) => OnRowLoaded();
        SetChrome(new CdsMaxWidthBox(_root, FileUploaderModel.MaxRowWidth));
        UpdateRow();
    }

    /// <summary>Raised when the remove button is activated (the owner removes the row).</summary>
    public event EventHandler<CdsFileRemovedEventArgs>? Deleted;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_root, BackgroundColorProperty, CdsColorKeys.Layer01);
        SetColor(_name, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        SetColor(_title, Microsoft.Maui.Controls.Label.TextColorProperty, CdsColorKeys.TextError);
        SetColor(_supplement, Microsoft.Maui.Controls.Label.TextColorProperty, CdsColorKeys.TextPrimary);
        SetColor(_errorIcon, CdsIcon.ColorProperty, CdsColorKeys.SupportError);
        SetColor(_complete, CdsIcon.ColorProperty, CdsColorKeys.Interactive);
        SetColor(_complete, CdsIcon.InnerColorProperty, CdsColorKeys.IconInverse);
        _outline.IsActive = IsInvalid && !disabled;
        _remove.IsEnabled = IsEnabled;
    }

    /// <summary>Applies several property changes as one update, so a row that turns invalid announces its whole message once.</summary>
    internal void Batch(Action changes)
    {
        _batching = true;
        try
        {
            changes();
        }
        finally
        {
            _batching = false;
        }
        UpdateRow();
    }

    /// <summary>Re-applies sizes, texts and status after a property change.</summary>
    protected virtual void UpdateRow()
    {
        if (_batching) return;
        var height = FileUploaderModel.RowHeight(Size);
        var invalid = IsInvalid && !string.IsNullOrEmpty(ErrorSubject);
        var padding = IsInvalid ? FileUploaderModel.InvalidRowPadding(Size) : 0;

        _name.Text = Name;
        _root.MinimumHeightRequest = height;
        _root.Padding = new Thickness(0, padding);
        _root.RowSpacing = invalid ? padding : 0;

        _spinner.IsVisible = Status == CdsFileStatus.Uploading;
        if (_spinner.IsVisible) _spinner.Start();
        _remove.IsVisible = Status == CdsFileStatus.Edit;
        _complete.IsVisible = Status == CdsFileStatus.Complete;
        _errorIcon.IsVisible = IsInvalid;

        _title.Text = ErrorSubject;
        _supplement.Text = ErrorBody;
        _supplement.IsVisible = !string.IsNullOrEmpty(ErrorBody);
        _message.IsVisible = invalid;
        _divider.Margin = new Thickness(0, 0, 0, FileUploaderModel.InvalidMessageGap(Size));

        var error = FileUploaderModel.ErrorText(IsInvalid, ErrorSubject, ErrorBody);
        _remove.Description = FileUploaderModel.RemoveLabel(IconDescription, Name);
        SemanticProperties.SetDescription(_name, Name);
        // aria-describedby: the error describes the remove button, or the name when there is no remove button.
        SemanticProperties.SetHint(_remove, error);
        SemanticProperties.SetHint(_name, _remove.IsVisible ? null : error);
        AnnounceError(error);
        AnnounceStatus();
        RefreshVisuals();
    }

    // The row's message is role="alert": speak it, assertively, when it appears or changes while the row is on screen. A
    // row that is invalid before it is on screen (an invalid file just added) is an inserted alert: it speaks once loaded.
    void AnnounceError(string? error)
    {
        if (error == _errorText) return;
        _errorText = error;
        _alertPending = error is not null && !IsLoaded;
        if (error is not null && IsLoaded) SpeakError();
    }

    void OnRowLoaded()
    {
        if (!_alertPending) return;
        _alertPending = false;
        SpeakError();
    }

    void SpeakError() =>
        CdsAnnouncer.Announce(FileUploaderModel.InvalidAnnouncement(Name, _errorText), CdsAnnouncePriority.Assertive);

    // Uploading to complete: the spinner's "Uploading" image is replaced by the check; say the upload finished.
    void AnnounceStatus()
    {
        var previous = _lastStatus;
        _lastStatus = Status;
        if (previous == CdsFileStatus.Uploading && Status == CdsFileStatus.Complete && IsLoaded)
            CdsAnnouncer.Announce(FileUploaderModel.CompletedAnnouncement(Name));
    }
}
