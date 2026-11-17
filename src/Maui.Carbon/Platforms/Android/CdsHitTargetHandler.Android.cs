using Android.Content;
using Android.Content.Res;
using Android.Views.Accessibility;
using Google.Android.Material.Button;
using AColor = Android.Graphics.Color;

namespace Maui.Carbon;

public partial class CdsHitTargetHandler
{
    /// <inheritdoc/>
    protected override MaterialButton CreatePlatformView() => new CdsAndroidHitTargetView(Context);

    partial void StripChrome()
    {
        var b = PlatformView;
        var clear = ColorStateList.ValueOf(AColor.Transparent);
        b.BackgroundTintList = clear;
        b.RippleColor = clear;
        b.StrokeWidth = 0;
        b.CornerRadius = 0;
        b.InsetTop = 0;
        b.InsetBottom = 0;
        b.StateListAnimator = null;
        b.Elevation = 0;
        b.SetMinHeight(0);
        b.SetMinimumHeight(0);
        b.SetMinWidth(0);
        b.SetMinimumWidth(0);
        b.SetPadding(0, 0, 0, 0);
    }

    partial void UpdateTabStop(CdsHitTarget target) => PlatformView.Focusable = target.IsTabStop;

    partial void UpdateAccessibility(CdsHitTarget target)
    {
        if (PlatformView is not CdsAndroidHitTargetView view) return;
        view.Role = target.Role;
        view.CheckedState = target.IsChecked;
        view.SelectedState = target.IsSelected;
        view.ExpandedState = target.IsExpanded;
    }
}

/// <summary>A chrome-less MaterialButton that reports the Carbon role to TalkBack.</summary>
internal sealed class CdsAndroidHitTargetView : MaterialButton
{
    public CdsAndroidHitTargetView(Context context) : base(context)
    {
    }

    internal CdsAccessibilityRole Role { get; set; }
    internal bool? CheckedState { get; set; }
    internal bool SelectedState { get; set; }
    internal bool? ExpandedState { get; set; }

    public override void OnInitializeAccessibilityNodeInfo(AccessibilityNodeInfo? info)
    {
        base.OnInitializeAccessibilityNodeInfo(info);
        if (info is null) return;
        info.ClassName = Role switch
        {
            CdsAccessibilityRole.Checkbox => "android.widget.CheckBox",
            CdsAccessibilityRole.Radio => "android.widget.RadioButton",
            CdsAccessibilityRole.Switch => "android.widget.Switch",
            CdsAccessibilityRole.ToggleButton => "android.widget.ToggleButton",
            CdsAccessibilityRole.Adjustable => "android.widget.SeekBar",
            CdsAccessibilityRole.ComboBox => "android.widget.Spinner",
            _ => "android.widget.Button",
        };
        if (Role is CdsAccessibilityRole.Checkbox or CdsAccessibilityRole.Radio or CdsAccessibilityRole.Switch or CdsAccessibilityRole.ToggleButton)
        {
            info.Checkable = true;
#pragma warning disable CA1422 // Obsolete on API 36 in favour of a tri-state setter, but still honoured there.
            info.Checked = CheckedState == true;
#pragma warning restore CA1422
        }
        if (Role is CdsAccessibilityRole.Tab or CdsAccessibilityRole.Option)
            info.Selected = SelectedState;
    }
}
