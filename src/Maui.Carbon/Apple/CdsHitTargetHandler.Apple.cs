using Foundation;
using UIKit;

namespace Maui.Carbon;

public partial class CdsHitTargetHandler
{
    /// <inheritdoc/>
    protected override UIButton CreatePlatformView() => new CdsAppleHitTargetView();

    /// <inheritdoc/>
    protected override void ConnectHandler(UIButton platformView)
    {
        base.ConnectHandler(platformView);
        if (platformView is CdsAppleHitTargetView view && VirtualView is CdsHitTarget target)
            view.Owner = new WeakReference<CdsHitTarget>(target);
    }

    partial void StripChrome()
    {
        PlatformView.BackgroundColor = UIColor.Clear;
        PlatformView.TintColor = UIColor.Clear;
    }

    partial void UpdateTabStop(CdsHitTarget target)
    {
        if (PlatformView is CdsAppleHitTargetView view) view.IsTabStop = target.IsTabStop;
    }

    partial void UpdateAccessibility(CdsHitTarget target)
    {
        var traits = target.Role switch
        {
            CdsAccessibilityRole.Link => UIAccessibilityTrait.Link,
            CdsAccessibilityRole.Adjustable => UIAccessibilityTrait.Adjustable,
            // A text area, not a button: VoiceOver reads its label and value without "button".
            CdsAccessibilityRole.TextBox => UIAccessibilityTrait.StaticText,
            _ => UIAccessibilityTrait.Button,
        };
        var role = target.Role;
        if (role is CdsAccessibilityRole.Checkbox or CdsAccessibilityRole.Radio or CdsAccessibilityRole.ToggleButton && target.IsChecked == true)
            traits |= UIAccessibilityTrait.Selected;
        // Selected tabs and options, and the current page or step (aria-current), carry the selected trait.
        if (target.IsSelected || target.CurrentKind != CdsCurrentKind.None) traits |= UIAccessibilityTrait.Selected;
        if (!target.IsEnabled) traits |= UIAccessibilityTrait.NotEnabled;

        // iOS 18 and Mac Catalyst 18 report expansion natively ("expanded"/"collapsed" after the role); before that it
        // goes into the value. Every role reports it: a header action or menu button that opens a panel is a disclosure.
        var nativeExpanded = OperatingSystem.IsIOSVersionAtLeast(18) || OperatingSystem.IsMacCatalystVersionAtLeast(18);
        if (OperatingSystem.IsIOSVersionAtLeast(18) || OperatingSystem.IsMacCatalystVersionAtLeast(18))
        {
            PlatformView.AccessibilityExpandedStatus = target.IsExpanded switch
            {
                true => UIAccessibilityExpandedStatus.Expanded,
                false => UIAccessibilityExpandedStatus.Collapsed,
                null => UIAccessibilityExpandedStatus.Unsupported,
            };
        }
        PlatformView.AccessibilityTraits = traits;
        // Read-only stays enabled and focusable; VoiceOver hears "read only" in the value.
        // An adjustable target's value (aria-valuetext) comes first; VoiceOver reads it after the name and role.
        PlatformView.AccessibilityValue = CdsHitTargetStates.WithValue(target.AccessibleValue, CdsHitTargetStates.AppleValue(
            role, target.IsChecked, target.IsExpanded, target.IsReadOnly, target.CurrentKind, nativeExpanded));
    }
}

/// <summary>A chrome-less UIButton that reports keyboard (focus-system) focus and activation.</summary>
internal sealed class CdsAppleHitTargetView : UIButton
{
    static int s_next;

    // The default initializer is a custom-type button. UIKit rejects UIButton(UIButtonType) from a subclass, and
    // logs it with a stack trace for every instance.
    public CdsAppleHitTargetView() : base()
    {
        BackgroundColor = UIColor.Clear;
        if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15))
        {
            FocusEffect = null;
            // Own focus group: Tab moves between Carbon controls like the web.
            FocusGroupIdentifier = "cds.hit." + Interlocked.Increment(ref s_next);
        }
    }

    public WeakReference<CdsHitTarget>? Owner { get; set; }

    CdsHitTarget? Target => Owner is not null && Owner.TryGetTarget(out var t) ? t : null;

    public bool IsTabStop { get; set; } = true;

    // VoiceOver's swipe up and down (iOS) and VO+Up / VO+Down (Mac Catalyst) on an adjustable element.
    public override void AccessibilityIncrement() => Target?.RequestAdjust(increase: true);

    public override void AccessibilityDecrement() => Target?.RequestAdjust(increase: false);

    public override bool CanBecomeFocused => IsTabStop && Enabled && !Hidden && UserInteractionEnabled;

    public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
    {
        base.DidUpdateFocus(context, coordinator);
        if (Target is not { } t) return;
        if (ReferenceEquals(context.NextFocusedView, this)) t.ReportFocus(true, fromKeyboard: true);
        else if (ReferenceEquals(context.PreviouslyFocusedView, this)) t.ReportFocus(false, fromKeyboard: true);
    }

    public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        if (Focused && IsActivation(presses)) return;
        // Keys the focus system hands to this button (Escape, arrows) go to the page's Carbon key router.
        foreach (var press in presses)
        {
            if (press.Key is { } k && CdsAppleHostView.Map(k.KeyCode) is { } key && key is not (CdsKey.Enter or CdsKey.Space) &&
                CdsOverlayHostHandler.Route(CdsOverlayHost.Find(Target), key, k.ModifierFlags.HasFlag(UIKeyModifierFlags.Shift)))
                return;
        }
        // A letter typed on a focused list box trigger is type-ahead.
        if (CdsAppleHostView.TypedCharacter(presses) is { } c && CdsOverlayHost.Find(Target)?.RouteCharacter(c) == true) return;
        base.PressesBegan(presses, evt);
    }

    public override void PressesEnded(NSSet<UIPress> presses, UIPressesEvent evt)
    {
        if (Focused && IsActivation(presses) && Target is { IsEnabled: true } t)
        {
            t.RaiseKeyboardActivation();
            return;
        }
        base.PressesEnded(presses, evt);
    }

    static bool IsActivation(NSSet<UIPress> presses)
    {
        foreach (var press in presses)
        {
            if (press.Key?.KeyCode is UIKeyboardHidUsage.KeyboardSpacebar or UIKeyboardHidUsage.KeyboardReturnOrEnter)
                return true;
        }
        return false;
    }
}
