using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// One choice from a list in a form, shown in the platform's own list (<see cref="CdsNativePicker"/>) inside Carbon field
/// chrome with a chevron, so mobile and assistive technology get the native picker. Fill it with
/// <see cref="CdsSelectItem"/> children or an <see cref="ItemsSource"/> (text from <see cref="ItemToString"/>). Use a
/// radio group for two options, and a dropdown or combo box when rows need styling or filtering.
/// <see cref="CdsFormField.IsInline"/> puts the label beside a transparent control for toolbars and table headers.
/// <see cref="CdsFormField.Size"/> is sm, md (default) or lg; other sizes clamp to that range. Read-only keeps the field
/// focusable and puts back any change made in the native list.
/// </summary>
[ContentProperty(nameof(Items))]
public partial class CdsSelect : CdsFormField
{
    /// <summary>The native picker inside the field.</summary>
    protected readonly CdsNativePicker Picker = new();

    /// <summary>The chevron at the end of the field.</summary>
    protected readonly CdsIcon Chevron = new(CdsIconName.ChevronDown, 16);

    readonly Grid _chevronHost = new() { BackgroundColor = Colors.Transparent, Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0) };
    List<SelectEntry> _entries = [];
    bool _syncing;
    INotifyCollectionChanged? _observed;

    /// <summary>Creates a medium select.</summary>
    public CdsSelect()
    {
        _chevronHost.Add(Chevron);
        // The chevron is outside the native picker: tapping it opens the list like tapping the text does.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (!IsReadOnly && IsEnabled) Picker.Focus();
        };
        _chevronHost.GestureRecognizers.Add(tap);
        AutomationProperties.SetIsInAccessibleTree(_chevronHost, false);
        Chrome.Trailing.Add(_chevronHost);
        Chrome.Input = Picker;

        Items.CollectionChanged += OnItemsChanged;
        Picker.SelectedIndexChanged += OnPickerIndexChanged;
        TrackFocus(Picker);
        SetChrome(Chrome);
        Rebuild();
        UpdateChrome();
    }

    /// <summary>Raised when the selection changes.</summary>
    public event EventHandler<SelectedItemChangedEventArgs>? SelectedItemChanged;

    /// <summary>The declarative options and groups (the content property).</summary>
    public ObservableCollection<CdsSelectOption> Items { get; } = new CdsItemCollection<CdsSelectOption>();

    /// <inheritdoc/>
    protected override VisualElement InputElement => Picker;

    /// <inheritdoc/>
    protected override void ApplyInput()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        Picker.IsEnabled = IsEnabled;
        // Read-only stays focusable (no InputTransparent: on Apple it takes the picker out of keyboard focus); a change
        // made in the native list is put back in OnPickerIndexChanged.
        SetColor(Picker, Microsoft.Maui.Controls.Picker.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(Picker, Microsoft.Maui.Controls.Picker.TitleColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        SetColor(Chevron, CdsIcon.ColorProperty, disabled || IsReadOnly ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary);

        // Inline: no fill (except on hover) and no underline.
        var hover = Is(CdsInteractionState.Hover) && !disabled && !IsReadOnly;
        var inline = IsInline && !IsFluidMode;
        var fill = inline ? (hover ? CdsColorKeys.FieldHover01 : null) : CdsFieldChrome.FieldFillKey(IsFluidMode, IsReadOnly, hover, flat: false);
        CdsTokens.SetColor(Chrome.Field, BackgroundColorProperty, fill);
        if (FindRule() is { } rule) rule.IsVisible = !inline;
    }

    /// <inheritdoc/>
    protected override void UpdateChrome()
    {
        base.UpdateChrome();
        // Carbon's select comes in sm, md and lg only (32 / 40 / 48); xs and the larger sizes clamp to that range.
        Chrome.Size = CdsListBoxTokens.Coerce(Size);
        // Inline, the control hugs its value (.cds--select--inline is a row of auto-sized items).
        Chrome.InlineFieldHugsContent = IsInline && !IsFluidMode;
        Picker.Title = Placeholder;
        Chrome.Field.Padding = new Thickness(IsInline && !IsFluidMode ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, 0);
        RefreshVisuals();
    }

    /// <summary>Forces the fluid layout on (used by the time picker's parts); the context turns it on by itself.</summary>
    internal void SetFluid(bool fluid) => Chrome.IsFluid = fluid;

    /// <summary>
    /// Hides the bottom rule while a group around this select draws its own outline and message (the fluid time picker's
    /// invalid and warning states).
    /// </summary>
    internal void SetRuleSuppressed(bool suppressed)
    {
        if (Chrome.IsRuleSuppressed == suppressed) return;
        Chrome.IsRuleSuppressed = suppressed;
        RefreshVisuals();
    }

    BoxView? FindRule()
    {
        foreach (var child in Chrome.Field.Children)
        {
            if (child is Grid overlay && child is not CdsFocusRing)
            {
                foreach (var inner in overlay.Children)
                {
                    if (inner is BoxView box) return box;
                }
            }
        }
        return null;
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CdsSelectOption option in e.OldItems) option.Changed -= OnOptionChanged;
        }
        if (e.NewItems is not null)
        {
            foreach (CdsSelectOption option in e.NewItems) option.Changed += OnOptionChanged;
        }
        Rebuild();
    }

    void OnOptionChanged(object? sender, EventArgs e) => Rebuild();

    void OnItemsSourceChanged(IEnumerable? source)
    {
        if (_observed is not null) _observed.CollectionChanged -= OnSourceCollectionChanged;
        _observed = source as INotifyCollectionChanged;
        if (_observed is not null) _observed.CollectionChanged += OnSourceCollectionChanged;
        Rebuild();
    }

    void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    /// <summary>Rebuilds the native list from <see cref="ItemsSource"/> (or <see cref="Items"/>) and re-selects.</summary>
    protected void Rebuild()
    {
        _entries = ItemsSource is not null
            ? SelectModel.FromSource(ItemsSource, ItemToString)
            : SelectModel.Flatten(Items);
        var texts = new List<string>(_entries.Count);
        foreach (var entry in _entries) texts.Add(entry.Text);

        _syncing = true;
        try
        {
            Picker.ItemsSource = texts;
            var index = SelectedItem is not null ? SelectModel.IndexOf(_entries, SelectedItem) : SelectedIndex;
            if (index >= _entries.Count) index = -1;
            Picker.SelectedIndex = index;
            if (SelectedIndex != index) SelectedIndex = index;
            var item = SelectModel.ValueAt(_entries, index);
            if (item is not null && !Equals(SelectedItem, item)) SelectedItem = item;
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnSelectedItemPropertyChanged(object? item)
    {
        if (_syncing) return;
        var index = SelectModel.IndexOf(_entries, item);
        Select(index, item);
    }

    void OnSelectedIndexPropertyChanged(int index)
    {
        if (_syncing) return;
        Select(index, SelectModel.ValueAt(_entries, index));
    }

    void OnPickerIndexChanged(object? sender, EventArgs e)
    {
        if (_syncing) return;
        if (IsReadOnly)
        {
            // Read-only blocks changes but stays focusable: put the previous choice back.
            _syncing = true;
            try
            {
                Picker.SelectedIndex = SelectedIndex;
            }
            finally
            {
                _syncing = false;
            }
            return;
        }
        var index = Picker.SelectedIndex;
        Select(index, SelectModel.ValueAt(_entries, index));
    }

    void Select(int index, object? item)
    {
        _syncing = true;
        try
        {
            if (Picker.SelectedIndex != index) Picker.SelectedIndex = index;
            if (SelectedIndex != index) SelectedIndex = index;
            if (!Equals(SelectedItem, item)) SelectedItem = item;
        }
        finally
        {
            _syncing = false;
        }
        SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs(item!, index));
    }
}
