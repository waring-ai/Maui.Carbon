namespace Maui.Carbon;

public partial class CdsTabs
{
    /// <summary>Bindable property for <see cref="SelectedIndex"/>.</summary>
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(CdsTabs), 0, BindingMode.TwoWay,
        coerceValue: static (_, v) => Math.Max(0, (int)v),
        propertyChanged: static (b, _, _) => ((CdsTabs)b).ApplySelection());

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsTabsType), typeof(CdsTabs), CdsTabsType.Line,
        propertyChanged: static (b, _, _) => ((CdsTabs)b).Rebuild());

    /// <summary>Bindable property for <see cref="Orientation"/>.</summary>
    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation), typeof(CdsTabsOrientation), typeof(CdsTabs), CdsTabsOrientation.Horizontal,
        propertyChanged: static (b, _, _) => ((CdsTabs)b).ApplyOrientation());

    /// <summary>Bindable property for <see cref="IsFullWidth"/>.</summary>
    public static readonly BindableProperty IsFullWidthProperty = BindableProperty.Create(
        nameof(IsFullWidth), typeof(bool), typeof(CdsTabs), false,
        propertyChanged: static (b, _, _) => ((CdsTabs)b).Rebuild());

    /// <summary>Bindable property for <see cref="Activation"/>.</summary>
    public static readonly BindableProperty ActivationProperty = BindableProperty.Create(
        nameof(Activation), typeof(CdsActivation), typeof(CdsTabs), CdsActivation.Automatic);

    /// <summary>Bindable property for <see cref="IsIconOnly"/>.</summary>
    public static readonly BindableProperty IsIconOnlyProperty = BindableProperty.Create(
        nameof(IsIconOnly), typeof(bool), typeof(CdsTabs), false,
        propertyChanged: static (b, _, _) => ((CdsTabs)b).Rebuild());

    /// <summary>Bindable property for <see cref="IconSize"/>.</summary>
    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(
        nameof(IconSize), typeof(CdsSize), typeof(CdsTabs), CdsSize.Md,
        propertyChanged: static (b, _, _) => ((CdsTabs)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsDismissible"/>.</summary>
    public static readonly BindableProperty IsDismissibleProperty = BindableProperty.Create(
        nameof(IsDismissible), typeof(bool), typeof(CdsTabs), false,
        propertyChanged: static (b, _, _) => ((CdsTabs)b).Rebuild());

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTabs), "Tabs",
        propertyChanged: static (b, _, _) => ((CdsTabs)b).Rebuild());

    /// <summary>Bindable property for <see cref="PanelPadding"/>.</summary>
    public static readonly BindableProperty PanelPaddingProperty = BindableProperty.Create(
        nameof(PanelPadding), typeof(Thickness), typeof(CdsTabs), new Thickness(CdsSpacing.Spacing05),
        propertyChanged: static (b, _, n) => ((CdsTabs)b)._panel.Padding = (Thickness)n);

    /// <summary>
    /// The index of the selected tab (two-way). The first tab is selected by default; a disabled or out-of-range
    /// index falls back to the nearest enabled tab.
    /// </summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Line (default) or contained tabs.</summary>
    public CdsTabsType Type
    {
        get => (CdsTabsType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// <see cref="CdsTabsOrientation.Horizontal"/> (default): a row of tabs above the panel. <see cref="CdsTabsOrientation.Vertical"/>:
    /// Carbon's vertical tabs, a column of 64 high tabs a quarter of the width with the panel beside it; Up and Down move between
    /// tabs, and the column scrolls when the control is shorter than its tabs. Vertical tabs show the label (up to two lines) and
    /// the icon; <see cref="Type"/>, <see cref="IsFullWidth"/>, <see cref="IsIconOnly"/>, <see cref="IsDismissible"/> and secondary
    /// labels apply to horizontal tabs only. Carbon shows horizontal tabs instead below the md breakpoint (672); switch the
    /// orientation from your layout for small windows.
    /// </summary>
    public CdsTabsOrientation Orientation
    {
        get => (CdsTabsOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Contained tabs share the width equally (from the md breakpoint, 672, up; auto width below). Works best with four or fewer tabs.</summary>
    public bool IsFullWidth
    {
        get => (bool)GetValue(IsFullWidthProperty);
        set => SetValue(IsFullWidthProperty, value);
    }

    /// <summary>
    /// <see cref="CdsActivation.Automatic"/> (default): arrow keys select. <see cref="CdsActivation.Manual"/>: arrows move
    /// focus, Enter or Space select; use when a panel is slow to load.
    /// </summary>
    public CdsActivation Activation
    {
        get => (CdsActivation)GetValue(ActivationProperty);
        set => SetValue(ActivationProperty, value);
    }

    /// <summary>Every tab shows only its icon (each needs <see cref="CdsTab.Icon"/>); <see cref="CdsTab.Text"/> is its name and tooltip.</summary>
    public bool IsIconOnly
    {
        get => (bool)GetValue(IsIconOnlyProperty);
        set => SetValue(IsIconOnlyProperty, value);
    }

    /// <summary>Icon-only tab size: <see cref="CdsSize.Md"/> (40, default) or <see cref="CdsSize.Lg"/> (48).</summary>
    public CdsSize IconSize
    {
        get => (CdsSize)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Shows a close icon on every tab; <see cref="TabCloseRequested"/> fires and you remove the tab. Only for user-created
    /// content (open files, saved queries), never for navigation.
    /// </summary>
    public bool IsDismissible
    {
        get => (bool)GetValue(IsDismissibleProperty);
        set => SetValue(IsDismissibleProperty, value);
    }

    /// <summary>Names the tab list for screen readers (not drawn). Default "Tabs".</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Padding around the selected tab's body. Default 16 (Carbon's tab content padding).</summary>
    public Thickness PanelPadding
    {
        get => (Thickness)GetValue(PanelPaddingProperty);
        set => SetValue(PanelPaddingProperty, value);
    }
}
