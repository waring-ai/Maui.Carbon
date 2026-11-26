using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Foundations;

[GalleryPage("Typography", GalleryCategory.Foundations, Order = 20,
    Summary = "The IBM Plex type scale. Productive styles for UI, body styles for paragraphs, headings for page structure.")]
public sealed class TypographyPage : GalleryPage
{
    public TypographyPage()
    {
        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing06 };
        foreach (var style in Enum.GetValues<CdsTypeStyle>())
        {
            var row = new VerticalStackLayout { Spacing = CdsSpacing.Spacing02 };
            row.Add(new SpecLabel(style));
            row.Add(new CdsText("The quick brown fox deploys a cluster", style));
            stack.Add(row);
        }
        Body = stack;
    }

    /// <summary>
    /// The token name, size/line height and weight of a style. Fluid styles step up at the page breakpoint, so their
    /// label follows the same breakpoint as the sample below it and says which breakpoint the size is for.
    /// </summary>
    sealed class SpecLabel : CdsText
    {
        static readonly BindableProperty BreakpointProperty = BindableProperty.Create(
            "Breakpoint", typeof(CdsBreakpoint?), typeof(SpecLabel), null,
            propertyChanged: static (b, _, _) => ((SpecLabel)b).Update());

        readonly CdsTypeStyle _style;

        public SpecLabel(CdsTypeStyle style) : base("", CdsTypeStyle.Code01, CdsTextTone.Secondary)
        {
            _style = style;
            if (CdsFluidType.IsFluid(style))
                SetDynamicResource(BreakpointProperty, CdsContextKeys.Breakpoint);
            Update();
        }

        void Update()
        {
            if (!CdsFluidType.IsFluid(_style))
            {
                var spec = CdsTypeSpec.Of(_style);
                Text = $"{spec.TokenName} · {spec.FontSize}/{spec.LineBoxHeight} · {spec.FontWeight}";
                return;
            }
            var breakpoint = (CdsBreakpoint?)GetValue(BreakpointProperty) ?? CdsBreakpoint.Sm;
            var fluid = CdsTypeSpec.Of(_style, breakpoint);
            Text = $"{fluid.TokenName} · {fluid.FontSize}/{fluid.LineBoxHeight} · {fluid.FontWeight} · at {breakpoint.ToString().ToLowerInvariant()}";
        }
    }
}
