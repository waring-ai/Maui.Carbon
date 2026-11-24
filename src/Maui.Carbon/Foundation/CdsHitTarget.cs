namespace Maui.Carbon;

/// <summary>The accessibility role a <see cref="CdsHitTarget"/> reports to the platform.</summary>
public enum CdsAccessibilityRole
{
    /// <summary>A push button.</summary>
    Button,
    /// <summary>A button that toggles between pressed and not pressed (uses <see cref="CdsHitTarget.IsChecked"/>).</summary>
    ToggleButton,
    /// <summary>A link to another place.</summary>
    Link,
    /// <summary>A checkbox (uses <see cref="CdsHitTarget.IsChecked"/>; <c>null</c> is mixed).</summary>
    Checkbox,
    /// <summary>A radio button (uses <see cref="CdsHitTarget.IsChecked"/>).</summary>
    Radio,
    /// <summary>An on/off switch (uses <see cref="CdsHitTarget.IsChecked"/>).</summary>
    Switch,
    /// <summary>A tab (uses <see cref="CdsHitTarget.IsSelected"/>).</summary>
    Tab,
    /// <summary>A menu item.</summary>
    MenuItem,
    /// <summary>An option in a list box (uses <see cref="CdsHitTarget.IsSelected"/>).</summary>
    Option,
    /// <summary>A combo box / select trigger (uses <see cref="CdsHitTarget.IsExpanded"/>).</summary>
    ComboBox,
    /// <summary>A button that expands and collapses content (uses <see cref="CdsHitTarget.IsExpanded"/>).</summary>
    Expander,
    /// <summary>An adjustable value such as a slider thumb.</summary>
    Adjustable,
    /// <summary>
    /// A focusable text area that is not a native input (the web's <c>role="textbox"</c>), such as the scrollable code of a
    /// code snippet. Set <see cref="CdsHitTarget.IsReadOnly"/> when the text cannot be edited (<c>aria-readonly</c>).
    /// </summary>
    TextBox,
}

/// <summary>
/// What a <see cref="CdsHitTarget"/> is the current one of (the web's <c>aria-current</c>): the page a navigation link
/// points to, the step of a progress indicator, the location in a breadcrumb.
/// </summary>
public enum CdsCurrentKind
{
    /// <summary>Not current (default).</summary>
    None,
    /// <summary>The current page (<c>aria-current="page"</c>): side nav, header menu and pagination links.</summary>
    Page,
    /// <summary>The current step of a process (<c>aria-current="step"</c>).</summary>
    Step,
    /// <summary>The current location (<c>aria-current="location"</c>), for example in a breadcrumb.</summary>
    Location,
    /// <summary>The current date (<c>aria-current="date"</c>) in a calendar.</summary>
    Date,
    /// <summary>The current item of a set (<c>aria-current="true"</c>).</summary>
    Item,
}

/// <summary>
/// An invisible native button laid over a control's drawn visuals. It gives Carbon controls real keyboard focus,
/// Tab order, Enter/Space activation, press feedback and a native accessibility element on every platform.
/// It measures as zero so it never changes the layout; it fills whatever cell it is placed in.
/// </summary>
public sealed partial class CdsHitTarget : Button
{
    /// <summary>Bindable property for <see cref="Role"/>.</summary>
    public static readonly BindableProperty RoleProperty = BindableProperty.Create(
        nameof(Role), typeof(CdsAccessibilityRole), typeof(CdsHitTarget), CdsAccessibilityRole.Button);

    /// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool?), typeof(CdsHitTarget), false);

    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(CdsHitTarget), false);

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool?), typeof(CdsHitTarget), null);

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsHitTarget), false);

    /// <summary>Bindable property for <see cref="CurrentKind"/>.</summary>
    public static readonly BindableProperty CurrentKindProperty = BindableProperty.Create(
        nameof(CurrentKind), typeof(CdsCurrentKind), typeof(CdsHitTarget), CdsCurrentKind.None);

    /// <summary>Bindable property for <see cref="IsTabStop"/>.</summary>
    public static readonly BindableProperty IsTabStopProperty = BindableProperty.Create(
        nameof(IsTabStop), typeof(bool), typeof(CdsHitTarget), true);

    /// <summary>Creates a hit target.</summary>
    public CdsHitTarget()
    {
        Text = null;
        Padding = 0;
        BorderWidth = 0;
        CornerRadius = 0;
        BackgroundColor = Colors.Transparent;
        TextColor = Colors.Transparent;
        MinimumHeightRequest = 0;
        MinimumWidthRequest = 0;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
    }

    /// <summary>The accessibility role. Default <see cref="CdsAccessibilityRole.Button"/>.</summary>
    public CdsAccessibilityRole Role
    {
        get => (CdsAccessibilityRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    /// <summary>Checked state for checkbox, radio, switch and toggle-button roles; <c>null</c> means mixed.</summary>
    public bool? IsChecked
    {
        get => (bool?)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Selected state for tab and option roles (reported for any role that sets it).</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Expanded state (<c>aria-expanded</c>) of anything that opens or shows more content: combo boxes, expanders, and
    /// buttons or links that open a panel or menu. <c>null</c> (default) when the target controls nothing that expands;
    /// set <c>false</c>, not <c>null</c>, while such a panel is closed. Reported for every role.
    /// </summary>
    public bool? IsExpanded
    {
        get => (bool?)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Read-only state (<c>aria-readonly</c>): the control shows a value the user cannot change. It stays enabled and
    /// focusable (Carbon: read-only controls stay in the Tab order), and screen readers report "read only"; the owning
    /// control ignores activation. Do not use it for disabled controls (<c>IsEnabled = false</c>).
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Marks the target as the current one of its set (<c>aria-current</c>): the link of the page being shown, the step
    /// in progress. Default <see cref="CdsCurrentKind.None"/>. Screen readers report "current page", "current step"…
    /// </summary>
    public CdsCurrentKind CurrentKind
    {
        get => (CdsCurrentKind)GetValue(CurrentKindProperty);
        set => SetValue(CurrentKindProperty, value);
    }

    /// <summary>
    /// Whether keyboard focus can land on the target (Tab order / focus system). Default <c>true</c>. Rows of a
    /// virtual-focus menu or list box, and secondary parts such as number-input steppers, turn it off: they stay
    /// clickable and visible to screen readers but are skipped by Tab.
    /// </summary>
    public bool IsTabStop
    {
        get => (bool)GetValue(IsTabStopProperty);
        set => SetValue(IsTabStopProperty, value);
    }

    /// <summary>
    /// True when the current focus came from the keyboard (or the platform cannot tell). Carbon shows its focus ring
    /// only for keyboard focus.
    /// </summary>
    public bool IsFocusFromKeyboard { get; internal set; } = true;

    /// <summary>Raised when the user activates the target from the keyboard while it has focus.</summary>
    internal void RaiseKeyboardActivation() => SendClicked();

    /// <summary>Sets the platform focus state (used by platform views that track focus themselves).</summary>
    internal void ReportFocus(bool focused, bool fromKeyboard)
    {
        IsFocusFromKeyboard = fromKeyboard;
        if (IsFocused != focused) ((IView)this).IsFocused = focused;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint) => Size.Zero;
}
