using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A text field for searching: a magnifier at the start, a clear button at the end once there is text, sizes sm / md / lg
/// (match the controls beside it; lg in a table toolbar). Escape or the clear button empties the field and returns focus to
/// it. <see cref="IsExpandable"/> collapses it to the magnifier alone; it expands on activation and collapses again on blur
/// or Escape while empty. <see cref="Label"/> is required: it is hidden and names the input. Keep one search per region and
/// filter as people type or on Enter.
/// </summary>
public partial class CdsSearch : CdsControl
{
    /// <summary>The field scaffold (the label row is always hidden).</summary>
    protected readonly CdsFieldChrome Chrome = new() { HideLabel = true };

    /// <summary>The native entry inside the field.</summary>
    protected readonly CdsNativeEntry Entry = new();

    /// <summary>The magnifier at the start of the field.</summary>
    protected readonly CdsIcon Magnifier = new(CdsIconName.Search, 16);

    readonly CdsIcon _fluidMagnifier = new(CdsIconName.Search, 16) { IsVisible = false, VerticalOptions = LayoutOptions.End, Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 13), InputTransparent = true };
    readonly Grid _root = new();
    readonly Grid _input = new() { ColumnSpacing = 0 };
    readonly CdsFieldIconButton _clear = new(CdsIconName.Close, CdsFieldButtonKind.Search) { IsVisible = false };
    readonly CdsFieldIconButton _collapsed = new(CdsIconName.Search, CdsFieldButtonKind.Plain) { IsVisible = false };

    /// <summary>Creates a medium search field.</summary>
    public CdsSearch()
    {
        _input.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _input.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _input.Add(Magnifier, 0);
        _input.Add(Entry, 1);
        Magnifier.InputTransparent = true;
        Chrome.Input = _input;
        Chrome.Trailing.Add(_clear);
        Chrome.Trailing.Add(_fluidMagnifier);
        Chrome.FluidLayoutChanged += (_, _) => UpdateLayout();

        Entry.TextChanged += OnEntryTextChanged;
        Entry.Completed += (_, _) => RaiseSearch();
        Entry.Unfocused += (_, _) =>
        {
            if (IsExpandable && string.IsNullOrEmpty(Text)) IsExpanded = false;
        };
        _clear.Clicked += (_, _) => Clear();
        _collapsed.Clicked += (_, _) => IsExpanded = true;
        TrackFocus(Entry);

        _root.Add(Chrome);
        _root.Add(_collapsed);
        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Raised when the text changes (typing, clearing or <see cref="Text"/>).</summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <summary>Raised when the user presses Return.</summary>
    public event EventHandler? Completed;

    /// <summary>Raised after the clear button or Escape emptied the field.</summary>
    public event EventHandler? Cleared;

    /// <summary>Raised when the expandable search expands or collapses.</summary>
    public event EventHandler? ExpandedChanged;

    /// <summary>The native entry's accessible target, for tests and subclasses.</summary>
    protected VisualElement InputElement => Entry;

    /// <summary>
    /// Whether the search renders its fluid layout (inside a <see cref="CdsFluidForm"/>, or a fluid variant): the label sits
    /// inside the 64px field and the magnifier moves to the bottom end, next to the clear button. Fluid search is never
    /// expandable.
    /// </summary>
    protected bool IsFluidMode => Chrome.IsFluidLayout;

    /// <summary>Empties the field, raises <see cref="Cleared"/> and returns focus to the input.</summary>
    public void Clear()
    {
        if (string.IsNullOrEmpty(Text)) return;
        Text = string.Empty;
        Cleared?.Invoke(this, EventArgs.Empty);
        Entry.Focus();
    }

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || !Is(CdsInteractionState.Focused)) return false;
        if (!string.IsNullOrEmpty(Text))
        {
            Clear();
            return true;
        }
        if (IsExpandable && IsExpanded)
        {
            IsExpanded = false;
            Dispatcher.Dispatch(() => _collapsed.Focus());
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        if (IsExpandable && !IsExpanded) state |= CdsInteractionState.ReadOnly;
        Chrome.Apply(state);
        var disabled = Is(CdsInteractionState.Disabled);
        Entry.IsEnabled = IsEnabled;
        SetColor(Entry, Microsoft.Maui.Controls.Entry.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        // Disabled hides the placeholder: it takes the field color.
        SetColor(Entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty, disabled ? CdsColorKeys.Field01 : CdsColorKeys.TextPlaceholder);
        SetColor(Magnifier, CdsIcon.ColorProperty, disabled ? CdsColorKeys.IconOnColorDisabled : CdsColorKeys.IconSecondary);
        SetColor(_fluidMagnifier, CdsIcon.ColorProperty, disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.IconSecondary);
        _clear.IsEnabled = IsEnabled;
        _collapsed.IsEnabled = IsEnabled;
    }

    /// <summary>Re-applies sizes, texts and accessibility after a property change.</summary>
    protected virtual void UpdateLayout()
    {
        var fluid = IsFluidMode;
        var height = fluid ? CdsSize.Md.Height() : Size.Height();
        var expanded = fluid || !IsExpandable || IsExpanded;
        Chrome.Size = Size;
        Chrome.IsVisible = expanded;
        _collapsed.IsVisible = !expanded;
        _collapsed.SetSize(height, height);
        _collapsed.Icon = Icon;
        _collapsed.Description = Label;
        _clear.SetSize(height, height);
        _clear.Description = CloseButtonLabel;
        _clear.IsVisible = expanded && !string.IsNullOrEmpty(Text);

        // The field pads its content by 16; the magnifier column and the clear button take their own square.
        // Fluid: no leading magnifier; the label floats in the field and the magnifier ends the row.
        _input.Margin = new Thickness(fluid ? 0 : -CdsSpacing.Spacing05, 0, 0, 0);
        _input.ColumnDefinitions[0].Width = new GridLength(fluid ? 0 : height);
        Magnifier.IsVisible = !fluid;
        _fluidMagnifier.IsVisible = fluid;
        _fluidMagnifier.Icon = Icon;
        Chrome.Label = fluid ? Label : null;
        Chrome.Trailing.Margin = fluid ? new Thickness(0) : new Thickness(0, 0, -CdsSpacing.Spacing05, 0);
        Chrome.Trailing.VerticalOptions = fluid ? LayoutOptions.End : LayoutOptions.Fill;
        Magnifier.Icon = Icon;
        Entry.Placeholder = Placeholder;

        if (IsExpandable && !fluid)
        {
            HorizontalOptions = expanded ? LayoutOptions.Fill : LayoutOptions.Start;
            _root.WidthRequest = expanded ? -1 : height;
        }
        else
        {
            HorizontalOptions = LayoutOptions.Fill;
            _root.WidthRequest = -1;
        }

        SemanticProperties.SetDescription(Entry, Label);
        RefreshVisuals();
    }

    void OnTextPropertyChanged(string? text)
    {
        if (Entry.Text != text) Entry.Text = text;
        _clear.IsVisible = (!IsExpandable || IsExpanded) && !string.IsNullOrEmpty(text);
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        Text = e.NewTextValue;
        _clear.IsVisible = (!IsExpandable || IsExpanded) && !string.IsNullOrEmpty(e.NewTextValue);
        TextChanged?.Invoke(this, e);
    }

    void RaiseSearch()
    {
        Completed?.Invoke(this, EventArgs.Empty);
        if (SearchCommand?.CanExecute(Text) == true) SearchCommand.Execute(Text);
    }

    void OnExpandedChanged(bool expanded)
    {
        UpdateLayout();
        if (expanded)
        {
            Chrome.Opacity = 0;
            _ = CdsMotion.AnimateAsync(this, "SearchExpand", v => Chrome.Opacity = v, 0, 1, CdsDuration.Fast01, CdsEasing.StandardProductive);
            if (IsLoaded) Dispatcher.Dispatch(() => Entry.Focus());
        }
        ExpandedChanged?.Invoke(this, EventArgs.Empty);
    }
}
