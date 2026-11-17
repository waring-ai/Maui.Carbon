# Maui.Carbon

An unofficial .NET MAUI implementation of IBM's [Carbon Design System](https://carbondesignsystem.com) (v11):
the four themes (White, Gray 10, Gray 90, Gray 100) with nestable theme zones, the layering model, the IBM Plex type
scale, Carbon icons and the Carbon component set, for Android, iOS, Mac Catalyst and Windows.

> Not affiliated with or endorsed by IBM. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Getting started

```bash
dotnet add package Maui.Carbon
```

`MauiProgram.cs`:

```csharp
builder
    .UseMauiApp<App>()
    .UseCarbon();          // registers IBM Plex, the Carbon handlers and the page overlay host
```

`App.xaml`:

```xml
<Application xmlns:cds="https://github.com/waring-ai/Maui.Carbon" ...>
    <Application.Resources>
        <cds:CdsResources Theme="White" />
    </Application.Resources>
</Application>
```

A page:

```xml
<ContentPage xmlns:cds="https://github.com/waring-ai/Maui.Carbon" ...>
    <VerticalStackLayout Padding="32" Spacing="24">
        <cds:CdsText Text="Create cluster" TypeStyle="Heading05" />
        <cds:CdsTextInput Label="Cluster name" Placeholder="e.g. prod-eu-de-1" />
        <cds:CdsButton Text="Create cluster" Icon="Add" Clicked="OnCreate" />
    </VerticalStackLayout>
</ContentPage>
```

## Themes and layers

- Switch the app theme at runtime: `CdsThemeManager.SetTheme(CdsThemeName.G100)` (or `FollowSystem()`).
- Re-theme a region: `<cds:CdsTheme Theme="G90">…</cds:CdsTheme>`. Zones nest and reset layering.
- Step a surface up a layer: `<cds:CdsLayer>…</cds:CdsLayer>`. Components always use the `-01` tokens; the nearest
  layer decides whether they resolve to `-02` or `-03`.
- Use tokens, never hex values: `BackgroundColor="{DynamicResource CdsLayer01}"`,
  `Padding="{x:Static cds:CdsSpacing.Spacing05}"`, `<cds:CdsText TypeStyle="BodyCompact01" />`.

## Repository

| Path | What |
|---|---|
| `src/Maui.Carbon` | the library (NuGet package) |
| `samples/Maui.Carbon.Gallery` | the gallery app: every component in every theme, plus Carbon's patterns |
| `tests/Maui.Carbon.Tests` | unit tests (tokens, theming, layers, component logic) |
| `tools/Maui.Carbon.Generator` | regenerates `src/Maui.Carbon/Generated` from `design/carbon` |
| `design/carbon` | the Carbon design-system snapshot this port is built from |
| `CONVENTIONS.md` | how components are built |

Build and test: `tools/verify.sh` (add `--gallery` to build the gallery; `--fast` for the inner loop).
Run the gallery: `dotnet build samples/Maui.Carbon.Gallery -t:Run -f net10.0-maccatalyst -r maccatalyst-arm64`.

After updating `design/carbon/tokens.json` or the icons, regenerate with
`dotnet run --project tools/Maui.Carbon.Generator`.
