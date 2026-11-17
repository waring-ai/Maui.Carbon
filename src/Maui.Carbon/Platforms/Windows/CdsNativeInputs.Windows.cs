using Microsoft.UI.Xaml;
using WBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Maui.Carbon;

internal static class CdsWindowsChrome
{
    static readonly string[] TextBoxKeys =
    [
        "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused", "TextControlBackgroundDisabled",
        "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused", "TextControlBorderBrushDisabled",
    ];

    static readonly string[] ComboBoxKeys =
    [
        "ComboBoxBackground", "ComboBoxBackgroundPointerOver", "ComboBoxBackgroundPressed", "ComboBoxBackgroundFocused",
        "ComboBoxBackgroundDisabled", "ComboBoxBorderBrush", "ComboBoxBorderBrushPointerOver", "ComboBoxBorderBrushPressed",
        "ComboBoxBorderBrushFocused", "ComboBoxBorderBrushDisabled", "ComboBoxBackgroundBorderBrushFocused",
    ];

    internal static void StripTextBox(Microsoft.UI.Xaml.Controls.Control control) => Strip(control, TextBoxKeys);

    internal static void StripComboBox(Microsoft.UI.Xaml.Controls.Control control) => Strip(control, ComboBoxKeys);

    static void Strip(Microsoft.UI.Xaml.Controls.Control control, string[] keys)
    {
        var clear = new WBrush(Microsoft.UI.Colors.Transparent);
        foreach (var key in keys) control.Resources[key] = clear;
        control.Resources["TextControlBorderThemeThickness"] = new Microsoft.UI.Xaml.Thickness(0);
        control.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
        control.Background = clear;
        control.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        control.Padding = new Microsoft.UI.Xaml.Thickness(0);
        control.MinHeight = 0;
        control.UseSystemFocusVisuals = false;
    }
}

public partial class CdsNativeEntryHandler
{
    partial void Strip() => CdsWindowsChrome.StripTextBox(PlatformView);
}

public partial class CdsNativeEditorHandler
{
    partial void Strip() => CdsWindowsChrome.StripTextBox(PlatformView);
}

public partial class CdsNativePickerHandler
{
    partial void Strip() => CdsWindowsChrome.StripComboBox(PlatformView);
}
