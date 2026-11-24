namespace Maui.Carbon.Tests.Foundation;

/// <summary>The adjustable hook of <see cref="CdsHitTarget"/> (CA-24): its value text and increment / decrement requests.</summary>
public class HitTargetAdjustableTests
{
    [Fact]
    public void Accessible_value_defaults_to_null()
    {
        var hit = new CdsHitTarget { Role = CdsAccessibilityRole.Adjustable };
        Assert.Null(hit.AccessibleValue);
        hit.AccessibleValue = "120 GB";
        Assert.Equal("120 GB", hit.AccessibleValue);
    }

    [Fact]
    public void Only_an_enabled_editable_adjustable_target_takes_adjust_requests()
    {
        var hit = new CdsHitTarget { Role = CdsAccessibilityRole.Adjustable };
        var seen = new List<string>();
        hit.Incremented += (_, _) => seen.Add("up");
        hit.Decremented += (_, _) => seen.Add("down");
        Assert.True(hit.RequestAdjust(increase: true));
        Assert.True(hit.RequestAdjust(increase: false));
        Assert.Equal(["up", "down"], seen);

        hit.IsReadOnly = true;
        Assert.False(hit.RequestAdjust(increase: true));
        hit.IsReadOnly = false;
        hit.IsEnabled = false;
        Assert.False(hit.RequestAdjust(increase: true));
        Assert.False(new CdsHitTarget { Role = CdsAccessibilityRole.Button }.RequestAdjust(increase: true));
        Assert.Equal(2, seen.Count);
    }

    [Fact]
    public void The_value_comes_before_the_state_words()
    {
        Assert.Equal("120 GB, read only", CdsHitTargetStates.WithValue("120 GB", "read only"));
        Assert.Equal("120 GB", CdsHitTargetStates.WithValue("120 GB", null));
        Assert.Equal("checked", CdsHitTargetStates.WithValue(null, "checked"));
        Assert.Null(CdsHitTargetStates.WithValue(null, null));
        Assert.Null(CdsHitTargetStates.WithValue("", ""));
    }
}
