namespace Maui.Carbon;

/// <summary>
/// A multi-line text field for longer free-form input (descriptions, comments), with an optional character or word
/// counter. <see cref="Rows"/> sets the height in <c>body-01</c> lines (default 4); padding is 11 / 16 like Carbon. Use a
/// <see cref="CdsTextInput"/> for a short value and a select for a known list.
/// </summary>
public partial class CdsTextArea : CdsFormField
{
    /// <summary>The native multi-line editor inside the field.</summary>
    protected readonly CdsNativeEditor Editor = new();

    /// <summary>Hosts the editor with Carbon's vertical padding (11).</summary>
    protected readonly Grid EditorHost = new() { Padding = new Thickness(0, 11) };

    /// <summary>Creates a four-row text area.</summary>
    public CdsTextArea()
    {
        EditorHost.Add(Editor);
        Chrome.Input = EditorHost;
        Editor.TextChanged += OnEditorTextChanged;
        TrackFocus(Editor);
        SetChrome(Chrome);
        UpdateChrome();
    }

    /// <summary>Raised when the text changes (typing or <see cref="Text"/>).</summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <inheritdoc/>
    protected override VisualElement InputElement => Editor;

    /// <inheritdoc/>
    protected override void ApplyInput()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        Editor.IsEnabled = IsEnabled;
        SetColor(Editor, Microsoft.Maui.Controls.Editor.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(Editor, Microsoft.Maui.Controls.Editor.PlaceholderColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
    }

    /// <inheritdoc/>
    protected override string? HintText()
    {
        var note = TextAreaModel.LimitNote(CounterMode, EnableCounter ? MaxCount : 0);
        var message = Chrome.CurrentMessage;
        return message is null ? note : note is null ? message : $"{message}. {note}";
    }

    /// <inheritdoc/>
    protected override void UpdateChrome()
    {
        base.UpdateChrome();
        var limit = EnableCounter ? MaxCount : 0;
        // Multi-line: the field grows with the editor; the status icon sits 12 from the top, not centered.
        Chrome.FieldHeight = -1;
        Chrome.Trailing.VerticalOptions = LayoutOptions.Start;
        Chrome.Trailing.Margin = new Thickness(0, CdsSpacing.Spacing04, 0, 0);
        Editor.HeightRequest = TextAreaModel.Height(Rows, CdsTypeSpec.Of(CdsTypeStyle.Body01).LineBoxHeight);
        Editor.Placeholder = Placeholder;
        Editor.IsReadOnly = IsReadOnly;
        Editor.MaxLength = CounterMode == CdsCounterMode.Character && limit > 0 ? limit : int.MaxValue;
        Chrome.Counter = TextAreaModel.CounterText(Editor.Text, CounterMode, limit);
        RefreshVisuals();
    }

    void OnTextPropertyChanged(string? text)
    {
        if (Editor.Text != text) Editor.Text = text;
    }

    void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        var limit = EnableCounter ? MaxCount : 0;
        var text = e.NewTextValue;
        if (CounterMode == CdsCounterMode.Word && TextAreaModel.Limit(text, CounterMode, limit) is { } cut && cut != text)
        {
            Editor.Text = cut; // raises TextChanged again with the cut text
            return;
        }
        Text = text;
        Chrome.Counter = TextAreaModel.CounterText(text, CounterMode, limit);
        TextChanged?.Invoke(this, e);
    }
}
