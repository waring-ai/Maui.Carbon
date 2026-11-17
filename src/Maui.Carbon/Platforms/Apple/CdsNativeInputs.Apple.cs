using UIKit;

namespace Maui.Carbon;

public partial class CdsNativeEntryHandler
{
    partial void Strip()
    {
        PlatformView.BorderStyle = UITextBorderStyle.None;
        PlatformView.BackgroundColor = UIColor.Clear;
        if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
            PlatformView.FocusEffect = null;
    }
}

public partial class CdsNativeEditorHandler
{
    partial void Strip()
    {
        PlatformView.BackgroundColor = UIColor.Clear;
        PlatformView.TextContainerInset = UIEdgeInsets.Zero;
        PlatformView.TextContainer.LineFragmentPadding = 0;
        if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
            PlatformView.FocusEffect = null;
    }
}

public partial class CdsNativePickerHandler
{
    partial void Strip()
    {
        PlatformView.BorderStyle = UITextBorderStyle.None;
        PlatformView.BackgroundColor = UIColor.Clear;
        if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
            PlatformView.FocusEffect = null;
    }
}
