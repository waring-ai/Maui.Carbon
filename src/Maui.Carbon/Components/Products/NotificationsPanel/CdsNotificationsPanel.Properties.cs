namespace Maui.Carbon;

public partial class CdsNotificationsPanel
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsNotificationsPanel), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).OnIsOpenChanged());

    /// <summary>Shows the panel while true, in the top end corner under the UI shell header. Two-way: a tap outside or Escape closes it.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsNotificationsPanel), "Notifications",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The heading of the panel.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(System.Collections.IEnumerable), typeof(CdsNotificationsPanel), null,
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).OnItemsSourceChanged());

    /// <summary>The notifications (<see cref="CdsNotificationsPanelItem"/>s). An observable collection updates an open panel.</summary>
    public System.Collections.IEnumerable? ItemsSource
    {
        get => (System.Collections.IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Bindable property for <see cref="DismissAllLabel"/>.</summary>
    public static readonly BindableProperty DismissAllLabelProperty = BindableProperty.Create(
        nameof(DismissAllLabel), typeof(string), typeof(CdsNotificationsPanel), "Dismiss all",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The label of the button that dismisses every notification.</summary>
    public string DismissAllLabel
    {
        get => (string)GetValue(DismissAllLabelProperty);
        set => SetValue(DismissAllLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="DismissLabel"/>.</summary>
    public static readonly BindableProperty DismissLabelProperty = BindableProperty.Create(
        nameof(DismissLabel), typeof(string), typeof(CdsNotificationsPanel), "Dismiss",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The accessible name of each notification's dismiss button.</summary>
    public string DismissLabel
    {
        get => (string)GetValue(DismissLabelProperty);
        set => SetValue(DismissLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="HasDoNotDisturb"/>.</summary>
    public static readonly BindableProperty HasDoNotDisturbProperty = BindableProperty.Create(
        nameof(HasDoNotDisturb), typeof(bool), typeof(CdsNotificationsPanel), false,
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>Shows the "Do not disturb" toggle under the title (Carbon shows it when <c>onDoNotDisturbChange</c> is set).</summary>
    public bool HasDoNotDisturb
    {
        get => (bool)GetValue(HasDoNotDisturbProperty);
        set => SetValue(HasDoNotDisturbProperty, value);
    }

    /// <summary>Bindable property for <see cref="IsDoNotDisturb"/>.</summary>
    public static readonly BindableProperty IsDoNotDisturbProperty = BindableProperty.Create(
        nameof(IsDoNotDisturb), typeof(bool), typeof(CdsNotificationsPanel), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).SyncDoNotDisturb());

    /// <summary>The state of the "Do not disturb" toggle.</summary>
    public bool IsDoNotDisturb
    {
        get => (bool)GetValue(IsDoNotDisturbProperty);
        set => SetValue(IsDoNotDisturbProperty, value);
    }

    /// <summary>Bindable property for <see cref="DoNotDisturbLabel"/>.</summary>
    public static readonly BindableProperty DoNotDisturbLabelProperty = BindableProperty.Create(
        nameof(DoNotDisturbLabel), typeof(string), typeof(CdsNotificationsPanel), "Do not disturb",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The label of the "Do not disturb" toggle.</summary>
    public string DoNotDisturbLabel
    {
        get => (string)GetValue(DoNotDisturbLabelProperty);
        set => SetValue(DoNotDisturbLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="EmptyStateLabel"/>.</summary>
    public static readonly BindableProperty EmptyStateLabelProperty = BindableProperty.Create(
        nameof(EmptyStateLabel), typeof(string), typeof(CdsNotificationsPanel), "You do not have any notifications",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The text shown when there are no notifications.</summary>
    public string EmptyStateLabel
    {
        get => (string)GetValue(EmptyStateLabelProperty);
        set => SetValue(EmptyStateLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="TodayLabel"/>.</summary>
    public static readonly BindableProperty TodayLabelProperty = BindableProperty.Create(
        nameof(TodayLabel), typeof(string), typeof(CdsNotificationsPanel), "Today",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The heading of today's notifications.</summary>
    public string TodayLabel
    {
        get => (string)GetValue(TodayLabelProperty);
        set => SetValue(TodayLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="YesterdayLabel"/>.</summary>
    public static readonly BindableProperty YesterdayLabelProperty = BindableProperty.Create(
        nameof(YesterdayLabel), typeof(string), typeof(CdsNotificationsPanel), "Yesterday",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The heading of yesterday's notifications.</summary>
    public string YesterdayLabel
    {
        get => (string)GetValue(YesterdayLabelProperty);
        set => SetValue(YesterdayLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="PreviousLabel"/>.</summary>
    public static readonly BindableProperty PreviousLabelProperty = BindableProperty.Create(
        nameof(PreviousLabel), typeof(string), typeof(CdsNotificationsPanel), "Previous",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The heading of older notifications.</summary>
    public string PreviousLabel
    {
        get => (string)GetValue(PreviousLabelProperty);
        set => SetValue(PreviousLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="ReadMoreLabel"/>.</summary>
    public static readonly BindableProperty ReadMoreLabelProperty = BindableProperty.Create(
        nameof(ReadMoreLabel), typeof(string), typeof(CdsNotificationsPanel), "Read more",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The button that shows a long description in full.</summary>
    public string ReadMoreLabel
    {
        get => (string)GetValue(ReadMoreLabelProperty);
        set => SetValue(ReadMoreLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="ReadLessLabel"/>.</summary>
    public static readonly BindableProperty ReadLessLabelProperty = BindableProperty.Create(
        nameof(ReadLessLabel), typeof(string), typeof(CdsNotificationsPanel), "Read less",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The button that clamps a long description again.</summary>
    public string ReadLessLabel
    {
        get => (string)GetValue(ReadLessLabelProperty);
        set => SetValue(ReadLessLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="HasFooterActions"/>.</summary>
    public static readonly BindableProperty HasFooterActionsProperty = BindableProperty.Create(
        nameof(HasFooterActions), typeof(bool), typeof(CdsNotificationsPanel), false,
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>Shows the footer with "View all" and the settings button (Carbon shows it when both <c>onViewAllClick</c> and <c>onSettingsClick</c> are set).</summary>
    public bool HasFooterActions
    {
        get => (bool)GetValue(HasFooterActionsProperty);
        set => SetValue(HasFooterActionsProperty, value);
    }

    /// <summary>Bindable property for <see cref="ViewAllLabelFormat"/>.</summary>
    public static readonly BindableProperty ViewAllLabelFormatProperty = BindableProperty.Create(
        nameof(ViewAllLabelFormat), typeof(string), typeof(CdsNotificationsPanel), "View all ({0})",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The footer button label; <c>{0}</c> is the number of notifications.</summary>
    public string ViewAllLabelFormat
    {
        get => (string)GetValue(ViewAllLabelFormatProperty);
        set => SetValue(ViewAllLabelFormatProperty, value);
    }

    /// <summary>Bindable property for <see cref="SettingsLabel"/>.</summary>
    public static readonly BindableProperty SettingsLabelProperty = BindableProperty.Create(
        nameof(SettingsLabel), typeof(string), typeof(CdsNotificationsPanel), "Settings",
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>The accessible name and tooltip of the settings button.</summary>
    public string SettingsLabel
    {
        get => (string)GetValue(SettingsLabelProperty);
        set => SetValue(SettingsLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="TimeFormatter"/>.</summary>
    public static readonly BindableProperty TimeFormatterProperty = BindableProperty.Create(
        nameof(TimeFormatter), typeof(Func<DateTimeOffset, string>), typeof(CdsNotificationsPanel), null,
        propertyChanged: static (b, _, _) => ((CdsNotificationsPanel)b).Rebuild());

    /// <summary>Formats each timestamp. Default: English relative time ("5 minutes ago", "Yesterday at 9:41 AM").</summary>
    public Func<DateTimeOffset, string>? TimeFormatter
    {
        get => (Func<DateTimeOffset, string>?)GetValue(TimeFormatterProperty);
        set => SetValue(TimeFormatterProperty, value);
    }

    /// <summary>Bindable property for <see cref="TriggerElement"/>.</summary>
    public static readonly BindableProperty TriggerElementProperty = BindableProperty.Create(
        nameof(TriggerElement), typeof(VisualElement), typeof(CdsNotificationsPanel), null);

    /// <summary>The header action that opens the panel; focus returns to it when the panel closes (Carbon: <c>triggerButtonRef</c>).</summary>
    public VisualElement? TriggerElement
    {
        get => (VisualElement?)GetValue(TriggerElementProperty);
        set => SetValue(TriggerElementProperty, value);
    }

    /// <summary>Bindable property for <see cref="NotificationCommand"/>.</summary>
    public static readonly BindableProperty NotificationCommandProperty = BindableProperty.Create(
        nameof(NotificationCommand), typeof(System.Windows.Input.ICommand), typeof(CdsNotificationsPanel), null);

    /// <summary>Runs when a notification is activated, with the <see cref="CdsNotificationsPanelItem"/> as parameter.</summary>
    public System.Windows.Input.ICommand? NotificationCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(NotificationCommandProperty);
        set => SetValue(NotificationCommandProperty, value);
    }

    /// <summary>Bindable property for <see cref="DismissNotificationCommand"/>.</summary>
    public static readonly BindableProperty DismissNotificationCommandProperty = BindableProperty.Create(
        nameof(DismissNotificationCommand), typeof(System.Windows.Input.ICommand), typeof(CdsNotificationsPanel), null);

    /// <summary>Runs when a notification's dismiss button is activated, with the item as parameter.</summary>
    public System.Windows.Input.ICommand? DismissNotificationCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(DismissNotificationCommandProperty);
        set => SetValue(DismissNotificationCommandProperty, value);
    }

    /// <summary>Bindable property for <see cref="DismissAllCommand"/>.</summary>
    public static readonly BindableProperty DismissAllCommandProperty = BindableProperty.Create(
        nameof(DismissAllCommand), typeof(System.Windows.Input.ICommand), typeof(CdsNotificationsPanel), null);

    /// <summary>Runs when "Dismiss all" is activated.</summary>
    public System.Windows.Input.ICommand? DismissAllCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(DismissAllCommandProperty);
        set => SetValue(DismissAllCommandProperty, value);
    }

    /// <summary>Bindable property for <see cref="ViewAllCommand"/>.</summary>
    public static readonly BindableProperty ViewAllCommandProperty = BindableProperty.Create(
        nameof(ViewAllCommand), typeof(System.Windows.Input.ICommand), typeof(CdsNotificationsPanel), null);

    /// <summary>Runs when "View all" is activated.</summary>
    public System.Windows.Input.ICommand? ViewAllCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(ViewAllCommandProperty);
        set => SetValue(ViewAllCommandProperty, value);
    }

    /// <summary>Bindable property for <see cref="SettingsCommand"/>.</summary>
    public static readonly BindableProperty SettingsCommandProperty = BindableProperty.Create(
        nameof(SettingsCommand), typeof(System.Windows.Input.ICommand), typeof(CdsNotificationsPanel), null);

    /// <summary>Runs when the settings button is activated.</summary>
    public System.Windows.Input.ICommand? SettingsCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SettingsCommandProperty);
        set => SetValue(SettingsCommandProperty, value);
    }
}
