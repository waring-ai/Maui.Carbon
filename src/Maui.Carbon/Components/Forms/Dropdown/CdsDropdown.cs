namespace Maui.Carbon;

/// <summary>
/// A select-like control: a field that opens a styled list and holds one chosen item. Use it to pick one option from a
/// predefined list in a form, modal or side panel, or (<see cref="CdsListBox.Type"/> inline) to filter or sort page content.
/// For two options use radio buttons; for long lists or values not in the list use a <see cref="CdsComboBox"/>; for several
/// choices a <see cref="CdsMultiSelect"/>; for plain forms on mobile a <see cref="CdsSelect"/>. Never nest dropdowns.
/// <para>
/// Keyboard: Down and Up open and move the highlight, Home and End jump, typing letters jumps to the next match, Enter or
/// Space opens or chooses, Escape or Tab close without changing the choice. Screen readers hear the highlighted option,
/// its state and its position as it moves. Read-only fields stay focusable, ignore input and are reported as read only.
/// </para>
/// </summary>
public partial class CdsDropdown : CdsListBox
{
    /// <summary>The field text: the chosen item, or <see cref="CdsListBox.Placeholder"/>.</summary>
    protected readonly CdsText ValueText = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };

    readonly CdsHitTarget _hit;

    /// <summary>Creates a medium dropdown.</summary>
    public CdsDropdown()
    {
        _hit = CreateTriggerHit();
        AutomationProperties.SetExcludedWithChildren(ValueText, true);
        ValueText.InputTransparent = true;
        var input = new Grid();
        input.Add(ValueText);
        input.Add(_hit);
        BuildField(input, _hit);
        UpdateChrome();
    }

    /// <summary>Raised when <see cref="SelectedItem"/> changes (by the user or in code).</summary>
    public event EventHandler<CdsListBoxSelectionChangedEventArgs>? SelectionChanged;

    /// <inheritdoc/>
    protected override string? AccessibleValue => SelectedItem is null ? null : TextOf(SelectedItem);

    /// <inheritdoc/>
    internal override bool IsRowChosen(ListBoxEntry row) => SelectedItem is not null && Equals(row.Value, SelectedItem);

    /// <inheritdoc/>
    internal override void ChooseRow(int index) => SelectedItem = VisibleRows[index].Value;

    /// <inheritdoc/>
    internal override object? HighlightOnOpen => SelectedItem;

    void OnSelectedItemChanged(object? previous, object? current)
    {
        UpdateValueText();
        UpdateAccessibleName();
        RefreshOpenList();
        SelectionChanged?.Invoke(this, new CdsListBoxSelectionChangedEventArgs(current, current is null ? [] : [current]));
    }

    void UpdateValueText()
    {
        ValueText.Text = SelectedItem is not null ? TextOf(SelectedItem) : Placeholder;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnChromeUpdated()
    {
        UpdateValueText();
        UpdateAccessibleName();
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged() => UpdateValueText();

    /// <inheritdoc/>
    protected override void ApplyTrigger(bool disabled, bool readOnly) =>
        SetColor(ValueText, Microsoft.Maui.Controls.Label.TextColorProperty, CdsListBoxTokens.FieldText(disabled));

    /// <summary>The wired hit target (for subclasses that restyle the trigger).</summary>
    protected CdsHitTarget TriggerTarget => _hit;
}

/// <summary>
/// The loading placeholder of <see cref="CdsDropdown"/>, <see cref="CdsComboBox"/> and <see cref="CdsMultiSelect"/>: a label
/// bar over a full-width field-height block, both shimmering.
/// </summary>
public class CdsDropdownSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsDropdownSkeleton), CdsSize.Md,
        propertyChanged: static (b, _, _) => ((CdsDropdownSkeleton)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsDropdownSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsDropdownSkeleton)b).UpdateLayout());

    readonly View _label = new CdsShimmerView { HeightRequest = 14, WidthRequest = 75, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly View _field = new CdsShimmerView();

    /// <summary>Creates a medium skeleton with a label bar.</summary>
    public CdsDropdownSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_label);
        stack.Add(_field);
        SetChrome(stack);
        UpdateLayout();
    }

    /// <summary>Field height: sm 32, md 40 (default), lg 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Hides the label bar.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    void UpdateLayout()
    {
        _field.HeightRequest = CdsListBoxTokens.RowHeight(Size);
        _label.IsVisible = !HideLabel;
    }
}
