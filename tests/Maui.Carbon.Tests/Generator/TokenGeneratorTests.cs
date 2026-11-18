using Maui.Carbon.Generator;

namespace Maui.Carbon.Tests.Generator;

public class TokenGeneratorTests
{
    static string Root => RepoPaths.FindRoot(AppContext.BaseDirectory);

    [Fact]
    public void Committed_token_code_matches_the_design_snapshot()
    {
        var files = CarbonGenerator.Generate(Path.Combine(Root, "design", "carbon"));
        var tokenFiles = files.Where(f => f.Key != "CdsIcons.g.cs").ToDictionary(f => f.Key, f => f.Value);
        var output = Path.Combine(Root, "src", "Maui.Carbon", "Generated");
        var drift = tokenFiles.Where(f => File.ReadAllText(Path.Combine(output, f.Key)) != f.Value).Select(f => f.Key).ToList();
        Assert.Empty(drift);
    }

    [Fact]
    public void Generated_code_uses_LF_line_endings_on_every_OS()
    {
        var files = CarbonGenerator.Generate(Path.Combine(Root, "design", "carbon"));
        Assert.All(files, f => Assert.DoesNotContain('\r', f.Value));
    }

    [Theory]
    [InlineData("#0f62fe", 255, 15, 98, 254)]
    [InlineData("#fff", 255, 255, 255, 255)]
    [InlineData("rgba(0, 0, 0, 0.6)", 153, 0, 0, 0)]
    [InlineData("rgb(22, 22, 22)", 255, 22, 22, 22)]
    public void Parses_css_colors(string css, byte a, byte r, byte g, byte b)
    {
        Assert.Equal(new Argb(a, r, g, b), Argb.Parse(css));
    }

    [Theory]
    [InlineData("0.875rem", 14)]
    [InlineData("2px", 2)]
    [InlineData("70ms", 70)]
    [InlineData("5vw", 0.05)]
    public void Converts_lengths(string css, double expected)
    {
        Assert.Equal(expected, CarbonTokens.Length(css), 6);
    }

    [Theory]
    [InlineData("layer-hover-01", "LayerHover01")]
    [InlineData("data-vis-alert-danger", "DataVisAlertDanger")]
    public void Pascal_cases_token_names(string token, string expected)
    {
        Assert.Equal(expected, TokenEmitter.Pascal(token));
    }
}
