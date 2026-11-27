namespace Maui.Carbon.Tests.Components;

/// <summary>The skeletons of the text, number and text area inputs, the toggle, the tag and the structured list.</summary>
public class FormSkeletonTests
{
    static View FieldBar(object skeleton) => Private.Field<View>(skeleton, "_field");

    static View LabelBar(object skeleton) => Private.Field<View>(skeleton, "_label");

    [Theory]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    public void Text_input_skeleton_is_as_tall_as_its_size(CdsSize size, double height)
    {
        var skeleton = new CdsTextInputSkeleton { Size = size };
        Assert.Equal(height, FieldBar(skeleton).HeightRequest);
        Assert.False(AutomationProperties.GetIsInAccessibleTree(skeleton) ?? true);
        Assert.True(skeleton.InputTransparent);
    }

    [Theory]
    [InlineData(CdsSize.Xs, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Xl, 48)]
    public void Number_input_skeleton_clamps_to_the_number_input_sizes(CdsSize size, double height)
    {
        var skeleton = new CdsNumberInputSkeleton { Size = size };
        Assert.Equal(height, FieldBar(skeleton).HeightRequest);
    }

    [Fact]
    public void Text_area_skeleton_is_100_high()
    {
        var skeleton = new CdsTextAreaSkeleton();
        Assert.Equal(100, FieldBar(skeleton).HeightRequest);
    }

    [Fact]
    public void Field_skeletons_hide_their_label_bar()
    {
        CdsControl[] skeletons = [new CdsTextInputSkeleton(), new CdsNumberInputSkeleton(), new CdsTextAreaSkeleton(), new CdsToggleSkeleton()];
        foreach (var skeleton in skeletons)
        {
            Assert.True(LabelBar(skeleton).IsVisible);
            skeleton.GetType().GetProperty("HideLabel")!.SetValue(skeleton, true);
            Assert.False(LabelBar(skeleton).IsVisible);
        }
    }

    [Theory]
    [InlineData(CdsToggleSize.Md, 48, 24, 18)]
    [InlineData(CdsToggleSize.Sm, 32, 16, 10)]
    public void Toggle_skeleton_matches_the_toggle_track(CdsToggleSize size, double width, double height, double knob)
    {
        Assert.Equal((width, height, knob), CdsToggleSkeleton.Metrics(size));
        var skeleton = new CdsToggleSkeleton { Size = size };
        var track = Private.Field<Border>(skeleton, "_track");
        Assert.Equal(width, track.WidthRequest);
        Assert.Equal(height, track.HeightRequest);
        Assert.Equal(knob, Private.Field<BoxView>(skeleton, "_knob").WidthRequest);
    }

    [Theory]
    [InlineData(CdsTagSize.Sm, 18)]
    [InlineData(CdsTagSize.Md, 24)]
    [InlineData(CdsTagSize.Lg, 32)]
    public void Tag_skeleton_is_a_60_wide_pill_of_the_tag_height(CdsTagSize size, double height)
    {
        var skeleton = new CdsTagSkeleton { Size = size };
        var pill = Private.Field<Border>(skeleton, "_pill");
        Assert.Equal(CdsTagSkeleton.PillWidth, pill.WidthRequest);
        Assert.Equal(height, pill.HeightRequest);
        Assert.Equal(new CornerRadius(height / 2), ((Microsoft.Maui.Controls.Shapes.RoundRectangle)pill.StrokeShape!).CornerRadius);
        Assert.False(AutomationProperties.GetIsInAccessibleTree(skeleton) ?? true);
    }

    [Fact]
    public void Structured_list_skeleton_has_a_header_and_the_requested_rows()
    {
        var skeleton = new CdsStructuredListSkeleton();
        Assert.Equal(5, skeleton.RowCount);
        Assert.Equal(6, skeleton.BuiltRows);
        skeleton.RowCount = 2;
        Assert.Equal(3, skeleton.BuiltRows);
        skeleton.RowCount = -3;
        Assert.Equal(0, skeleton.RowCount);
        Assert.Equal(1, skeleton.BuiltRows);
        Assert.Equal([8d, 30d, 15d], CdsStructuredListSkeleton.ColumnWeights);
        Assert.False(AutomationProperties.GetIsInAccessibleTree(skeleton) ?? true);
    }
}
