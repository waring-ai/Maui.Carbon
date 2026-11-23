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
            _ => UIAccessibilityTrait.Button,
        };
        string? value = null;
        switch (target.Role)
        {
            case CdsAccessibilityRole.Checkbox:
            case CdsAccessibilityRole.Radio:
            case CdsAccessibilityRole.ToggleButton:
                value = target.IsChecked switch { true => "checked", false => "not checked", null => "mixed" };
                if (target.IsChecked == true) traits |= UIAccessibilityTrait.Selected;
                break;
            case CdsAccessibilityRole.Switch:
                value = target.IsChecked == true ? "on" : "off";
                break;
            case CdsAccessibilityRole.Tab:
            case CdsAccessibilityRole.Option:
                if (target.IsSelected) traits |= UIAccessibilityTrait.Selected;
                break;
            case CdsAccessibilityRole.ComboBox:
            case CdsAccessibilityRole.Expander:
                value = target.IsExpanded switch { true => "expanded", false => "collapsed", null => null };
                break;
        }
        if (!target.IsEnabled) traits |= UIAccessibilityTrait.NotEnabled;
        PlatformView.AccessibilityTraits = traits;
        PlatformView.AccessibilityValue = value;
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
