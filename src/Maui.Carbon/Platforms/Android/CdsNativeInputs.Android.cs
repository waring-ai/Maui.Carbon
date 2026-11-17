using Android.Content.Res;
using AColor = Android.Graphics.Color;

namespace Maui.Carbon;

public partial class CdsNativeEntryHandler
{
    partial void Strip()
    {
        PlatformView.Background = null;
        PlatformView.BackgroundTintList = ColorStateList.ValueOf(AColor.Transparent);
        PlatformView.SetPadding(0, 0, 0, 0);
    }
}

public partial class CdsNativeEditorHandler
{
    partial void Strip()
    {
        PlatformView.Background = null;
        PlatformView.BackgroundTintList = ColorStateList.ValueOf(AColor.Transparent);
        PlatformView.SetPadding(0, 0, 0, 0);
    }
}

public partial class CdsNativePickerHandler
{
    partial void Strip()
    {
        PlatformView.Background = null;
        PlatformView.BackgroundTintList = ColorStateList.ValueOf(AColor.Transparent);
        PlatformView.SetPadding(0, 0, 0, 0);
    }
}
