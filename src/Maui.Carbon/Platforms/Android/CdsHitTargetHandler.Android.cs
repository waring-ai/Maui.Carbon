using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views.Accessibility;
using AndroidX.Core.View.Accessibility;
using Google.Android.Material.Button;
using AColor = Android.Graphics.Color;

namespace Maui.Carbon;

public partial class CdsHitTargetHandler
{
    /// <inheritdoc/>
    protected override MaterialButton CreatePlatformView() => new CdsAndroidHitTargetView(Context);

    /// <inheritdoc/>
    protected override void ConnectHandler(MaterialButton platformView)
    {
        base.ConnectHandler(platformView);
        if (platformView is CdsAndroidHitTargetView view && VirtualView is CdsHitTarget target)
            view.Owner = new WeakReference<CdsHitTarget>(target);
    }

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
        view.ReadOnlyState = target.IsReadOnly;
        view.CurrentState = target.CurrentKind;
        var oldValue = view.ValueState;
        view.ValueState = target.AccessibleValue;
        view.CanAdjust = target.CanAdjust;
        // An adjustable target's value changed (aria-valuetext): like a SeekBar, tell TalkBack so it reads the new value.
        if (oldValue != view.ValueState && view.IsAccessibilityFocused)
            view.SendAccessibilityEvent(EventTypes.ViewSelected);
    }
}

/// <summary>A chrome-less MaterialButton that reports the Carbon role and states to TalkBack.</summary>
internal sealed class CdsAndroidHitTargetView : MaterialButton
{
    public CdsAndroidHitTargetView(Context context) : base(context)
    {
    }

    internal WeakReference<CdsHitTarget>? Owner { get; set; }
    internal CdsAccessibilityRole Role { get; set; }
    internal bool? CheckedState { get; set; }
    internal bool SelectedState { get; set; }
    internal bool? ExpandedState { get; set; }
    internal bool ReadOnlyState { get; set; }
    internal CdsCurrentKind CurrentState { get; set; }
    internal string? ValueState { get; set; }
    internal bool CanAdjust { get; set; }

    CdsHitTarget? Target => Owner is not null && Owner.TryGetTarget(out var t) ? t : null;

    public override void OnInitializeAccessibilityNodeInfo(AccessibilityNodeInfo? info)
    {
        base.OnInitializeAccessibilityNodeInfo(info);
        if (info is null) return;
        info.ClassName = CdsHitTargetStates.AndroidClassName(Role);
        if (CdsHitTargetStates.IsCheckable(Role))
        {
            info.Checkable = true;
#pragma warning disable CA1422 // Obsolete on API 36 in favour of a tri-state setter, but still honoured there.
            info.Checked = CheckedState == true;
#pragma warning restore CA1422
        }
        if (SelectedState || Role is CdsAccessibilityRole.Tab or CdsAccessibilityRole.Option)
            info.Selected = SelectedState;
        // aria-expanded: TalkBack offers the expand or collapse action and speaks the state.
        if (ExpandedState is { } expanded)
            info.AddAction(expanded ? AccessibilityNodeInfo.AccessibilityAction.ActionCollapse : AccessibilityNodeInfo.AccessibilityAction.ActionExpand);
        // Adjustable (role="slider"): TalkBack's adjust gestures and volume keys send scroll forward and backward.
        if (Role == CdsAccessibilityRole.Adjustable && CanAdjust)
        {
            info.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionScrollForward);
            info.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionScrollBackward);
        }
        // Mixed, expanded, current and read-only have no node flag: they go into the state description (compat stores it
        // in the node extras below API 30).
        if (AccessibilityNodeInfoCompat.Wrap(info) is not { } compat) return;
        if (CdsHitTargetStates.AndroidRoleDescription(Role) is { } role) compat.RoleDescription = role;
        compat.StateDescription = CdsHitTargetStates.WithValue(ValueState, CdsHitTargetStates.AndroidStateDescription(Role, CheckedState, ExpandedState, ReadOnlyState, CurrentState));
    }

    public override bool PerformAccessibilityAction(global::Android.Views.Accessibility.Action action, Bundle? arguments)
    {
        if ((action == global::Android.Views.Accessibility.Action.Expand && ExpandedState == false) ||
            (action == global::Android.Views.Accessibility.Action.Collapse && ExpandedState == true))
        {
            if (Target is not { IsEnabled: true } target) return false;
            target.RaiseKeyboardActivation();
            return true;
        }
        if (action is global::Android.Views.Accessibility.Action.ScrollForward or global::Android.Views.Accessibility.Action.ScrollBackward &&
            Role == CdsAccessibilityRole.Adjustable)
            return Target?.RequestAdjust(increase: action == global::Android.Views.Accessibility.Action.ScrollForward) == true;
        return base.PerformAccessibilityAction(action, arguments);
    }
}
