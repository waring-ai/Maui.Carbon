namespace Maui.Carbon.Tests.Components;

public class HeadingTests
{
    [Theory]
    [InlineData(1, CdsTypeStyle.Heading05)]
    [InlineData(2, CdsTypeStyle.Heading04)]
    [InlineData(3, CdsTypeStyle.Heading03)]
    [InlineData(4, CdsTypeStyle.Heading02)]
    [InlineData(5, CdsTypeStyle.Heading01)]
    [InlineData(6, CdsTypeStyle.HeadingCompact01)]
    [InlineData(0, CdsTypeStyle.Heading05)]
    [InlineData(9, CdsTypeStyle.HeadingCompact01)]
    public void Level_picks_the_productive_heading_style(int level, CdsTypeStyle expected) =>
        Assert.Equal(expected, CdsHeadingStyles.ForLevel(level));

    [Fact]
    public void Semantic_levels_are_clamped()
    {
        Assert.Equal(SemanticHeadingLevel.Level1, CdsHeadingStyles.ToSemantic(-1));
        Assert.Equal(SemanticHeadingLevel.Level3, CdsHeadingStyles.ToSemantic(3));
        Assert.Equal(SemanticHeadingLevel.Level6, CdsHeadingStyles.ToSemantic(12));
    }

    [Fact]
    public void Section_adds_one_level_up_to_six()
    {
        Assert.Equal(2, CdsSectionModel.LevelFor(1, null));
        Assert.Equal(3, CdsSectionModel.LevelFor(2, null));
        Assert.Equal(6, CdsSectionModel.LevelFor(6, null));
    }

    [Fact]
    public void Forced_section_level_replaces_parent_plus_one()
    {
        Assert.Equal(4, CdsSectionModel.LevelFor(1, 4));
        Assert.Equal(1, CdsSectionModel.LevelFor(5, 1));
        Assert.Equal(6, CdsSectionModel.LevelFor(1, 20));
        Assert.Equal(1, CdsSectionModel.LevelFor(1, -2));
    }

    [Fact]
    public void Heading_outside_a_section_is_level_one()
    {
        var heading = new CdsHeading("prod-eu-1");
        Assert.Equal(1, heading.Level);
        Assert.Equal(CdsTypeStyle.Heading05, heading.TypeStyle);
        Assert.Equal(SemanticHeadingLevel.Level1, SemanticProperties.GetHeadingLevel(heading));
    }

    [Fact]
    public void Nested_sections_raise_the_level_of_their_headings()
    {
        var heading = new CdsHeading("Worker pools");
        var inner = new CdsSection { Body = heading };
        var outer = new CdsSection { Body = inner };
        Assert.Equal(2, outer.EffectiveLevel);
        Assert.Equal(3, inner.EffectiveLevel);
        Assert.Equal(3, heading.Level);
        Assert.Equal(CdsTypeStyle.Heading03, heading.TypeStyle);
        Assert.Equal(SemanticHeadingLevel.Level3, SemanticProperties.GetHeadingLevel(heading));
    }

    [Fact]
    public void Forced_section_level_applies_to_the_subtree()
    {
        var heading = new CdsHeading("Details");
        _ = new CdsSection { Level = 4, Body = heading };
        Assert.Equal(4, heading.Level);
        Assert.Equal(CdsTypeStyle.Heading02, heading.TypeStyle);
    }

    [Fact]
    public void An_explicit_type_style_survives_level_changes()
    {
        var heading = new CdsHeading("Node pool") { TypeStyle = CdsTypeStyle.HeadingCompact01 };
        _ = new CdsSection { Body = heading };
        Assert.Equal(2, heading.Level);
        Assert.Equal(CdsTypeStyle.HeadingCompact01, heading.TypeStyle);
    }
}
