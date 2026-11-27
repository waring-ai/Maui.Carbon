namespace Maui.Carbon;

// MAUI's Button implements ICrossPlatformLayout on iOS and Mac Catalyst to measure and lay out its native title and
// image, and the WrapperView a hit target sits in (for its clip, shadow or semantics) calls it on every layout pass: each
// call reads the UIButton's CurrentImage, ImageView and TitleLabel through the Objective-C bridge. A hit target has no
// title or image, measures as zero (MeasureOverride) and its native view is sized to the wrapper's bounds by the wrapper
// itself, so it re-implements the interface to skip that work. Profiling Relay put it at ~12% of the UI thread.
public sealed partial class CdsHitTarget : ICrossPlatformLayout
{
    Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) => Size.Zero;

    Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds) => bounds.Size;
}
