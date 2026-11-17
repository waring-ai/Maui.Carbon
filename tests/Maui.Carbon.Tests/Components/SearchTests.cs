using System.Reflection;

namespace Maui.Carbon.Tests.Components;

public class SearchTests
{
    static void SetState(CdsControl control, CdsInteractionState flags) =>
        typeof(CdsControl).GetMethod("SetState", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(control, [flags, true]);

    [Fact]
    public void ClearButtonUsesFieldHoverAndSelectedBackground()
    {
        var idle = CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Search, CdsInteractionState.None);
        Assert.Null(idle.Background);
        Assert.Equal(CdsColorKeys.IconPrimary, idle.Icon);
        Assert.Equal(CdsColorKeys.FieldHover01, CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Search, CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.BackgroundSelected, CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Search, CdsInteractionState.Pressed).Background);
    }

    [Fact]
    public void CollapsedSearchUsesBackgroundHover() =>
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Plain, CdsInteractionState.Hover).Background);

    [Fact]
    public void DisabledButtonsDimTheirIcons()
    {
        Assert.Equal(CdsColorKeys.IconOnColorDisabled, CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Search, CdsInteractionState.Disabled | CdsInteractionState.Hover).Icon);
        var stepper = CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Field, CdsInteractionState.Disabled);
        Assert.Null(stepper.Background);
        Assert.Equal(CdsColorKeys.IconDisabled, stepper.Icon);
    }

    [Fact]
    public void ReadOnlyStepperIsMutedEvenOnHover()
    {
        var v = CdsFieldButtonTokens.Resolve(CdsFieldButtonKind.Field, CdsInteractionState.Hover, muted: true);
        Assert.Null(v.Background);
        Assert.Equal(CdsColorKeys.IconDisabled, v.Icon);
    }

    [Fact]
    public void ControlSetsEveryPropertyWithoutThrowing()
    {
        var search = new CdsSearch
        {
            Label = "Search clusters", Placeholder = "Search by name", Size = CdsSize.Lg, CloseButtonLabel = "Clear", Icon = CdsIconName.Search,
            IsExpandable = true, IsExpanded = true, Text = "prod",
        };
        search.IsEnabled = false;
        Assert.Equal("prod", search.Text);
    }

    [Fact]
    public void ClearEmptiesTheTextAndRaisesCleared()
    {
        var search = new CdsSearch { Label = "Search", Text = "abc" };
        var cleared = 0;
        search.Cleared += (_, _) => cleared++;
        search.Clear();
        Assert.Equal(string.Empty, search.Text);
        Assert.Equal(1, cleared);
        search.Clear(); // already empty: nothing happens
        Assert.Equal(1, cleared);
    }

    [Fact]
    public void EscapeClearsTheText()
    {
        var search = new CdsSearch { Label = "Search", IsExpandable = true, IsExpanded = true, Text = "abc" };
        SetState(search, CdsInteractionState.Focused);
        Assert.True(search.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.Equal(string.Empty, search.Text);
        Assert.True(search.IsExpanded); // the first Escape only clears
    }

    [Fact]
    public void OtherKeysAreNotConsumed()
    {
        var search = new CdsSearch { Label = "Search", Text = "abc" };
        Assert.False(search.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.False(search.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape))); // not focused
        Assert.Equal("abc", search.Text);
    }
}
