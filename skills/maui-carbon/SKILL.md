---
name: maui-carbon
description: Build .NET MAUI interfaces with Maui.Carbon (Waring.Maui.Carbon), the Carbon Design System component library. Use when finding or using Cds controls, writing Carbon XAML or C# UI, setting up the package, configuring themes and layers, or binding forms, file pickers, tables, charts, navigation and overlays. For design decisions (layout, hierarchy, which component fits a need, copy, states, accessibility, conformance review), use the maui-carbon-design skill.
---

# Maui.Carbon

This skill covers what the library offers and how to use it; the **maui-carbon-design** skill covers how to design a Carbon screen. Load both when designing and building a new screen. All public controls use the `Cds` prefix and the `Maui.Carbon` namespace. The NuGet package ID is `Waring.Maui.Carbon`.

## Find the right API

The skill is written for an app that references the NuGet package, so look things up in this order:

1. Read [the component catalog](references/components.md) to select a component and its companion types.
2. Search the XML documentation that ships in the package for exact member names and what they do. It matches the installed version:

   ```sh
   # <version> is the one the app references; any lib/<tfm> folder will do
   grep -A8 'name="[PE]:Maui.Carbon.CdsDataTable\.' ~/.nuget/packages/waring.maui.carbon/<version>/lib/net11.0/Maui.Carbon.xml
   ```

   `P:` members are properties, `E:` events, `M:` methods, `T:` the type itself. The file is over 1 MB, so search it rather than read it. Inherited members are documented on the base type. Do not infer names from Carbon React props.
3. For composition (items, templates, events, state), read the component's gallery page in the public repository at the tag of the installed version: `https://raw.githubusercontent.com/waring-ai/Maui.Carbon/v<version>/samples/Maui.Carbon.Gallery/Pages/<Category>/<Component>Page.xaml` (and `.xaml.cs`). `GalleryExample` and `ThemeStrip` in those pages are sample wrappers, not package types.
4. For whole screens, read `samples/Maui.Carbon.Gallery/Pages/Patterns/` and the MVVM app in `samples/Maui.Carbon.Relay/` in the same repository.
5. Only when a default value or binding mode matters and the docs do not say, read `src/Maui.Carbon/Components/<Category>/<Family>/*.Properties.cs` at that tag.

Use the tag, not `main`: newer source can have members the installed package lacks.

## Configure an app

The package needs .NET 11 with the MAUI workload (Microsoft.Maui.Controls 11.0.10 or later).

```sh
dotnet add package Waring.Maui.Carbon
```

Register fonts, handlers and lifecycle support in `MauiProgram.cs`:

```csharp
using Maui.Carbon;

builder.UseMauiApp<App>().UseCarbon();
```

Merge the resources into `App.xaml`:

```xml
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:cds="https://github.com/waring-ai/Maui.Carbon"
             x:Class="MyApp.App">
    <Application.Resources>
        <cds:CdsResources Theme="White" />
    </Application.Resources>
</Application>
```

`UseCarbon` defaults `InjectPageOverlayHost`, `ApplyImplicitStyles` and `RespectReducedMotion` to true. Dropdowns, menus, tooltips, modals and toasts need an overlay host: if you disable injection, put `CdsOverlayHost` at each page's root, and check custom page styles if overlays stop appearing.

## Compose a form

A page-content fragment; declare the `cds` namespace on the page and bind to a view model with these properties and commands.

```xml
<cds:CdsForm>
    <cds:CdsText Text="Create cluster" TypeStyle="Heading03" />
    <cds:CdsTextInput Label="Cluster name"
                      Placeholder="e.g. prod-eu-de-1"
                      Text="{Binding ClusterName, Mode=TwoWay}"
                      IsInvalid="{Binding IsNameInvalid}"
                      InvalidText="{Binding NameError}" />
    <cds:CdsSelect Label="Region" SelectedItem="{Binding Region, Mode=TwoWay}">
        <cds:CdsSelectItem Value="eu-de" Text="Frankfurt" />
        <cds:CdsSelectItem Value="us-south" Text="Dallas" />
    </cds:CdsSelect>
    <HorizontalStackLayout Spacing="16">
        <cds:CdsButton Text="Cancel" Kind="Secondary" Command="{Binding CancelCommand}" />
        <cds:CdsButton Text="Create cluster" Icon="Add" Command="{Binding CreateCommand}" />
    </HorizontalStackLayout>
</cds:CdsForm>
```

- Validate on blur or submit; bind the invalid state and an actionable message. Keep labels meaningful even when visually hidden.
- `CdsFormGroup` with `Legend` groups related fields; `CdsFormItem` and `CdsFormLabel` align custom inputs.
- Fluid inputs and `CdsFluidForm` put the label inside the field and show no helper text. There is no fluid password input.
- `CdsSelect` for simple choices, `CdsComboBox` for searchable single selection, `CdsMultiSelect` for several. Read each one's item and selection contract before binding domain objects.
- For a field that picks a local file (an import, a certificate), bind `CdsFileUploader`'s `FilePath` with `AccessMode="Path"`; never a text field plus a raw MAUI `FilePicker` button. Read [file-uploader](references/file-uploader.md) first: it covers validation, multiple files and platform file access.

## Apply themes and tokens

Use `White`, `G10`, `G90` or `G100`. Switch the app with `CdsThemeManager.SetTheme(CdsThemeName.G100)` or `CdsThemeManager.FollowSystem()`.

```xml
<cds:CdsTheme Theme="G90">
    <cds:CdsLayer>
        <VerticalStackLayout BackgroundColor="{DynamicResource CdsLayer01}"
                             Padding="{x:Static cds:CdsSpacing.Spacing05}">
            <cds:CdsText Text="Environment" TypeStyle="Heading03" />
            <cds:CdsTextInput Label="Name" />
        </VerticalStackLayout>
    </cds:CdsLayer>
</cds:CdsTheme>
```

Theme zones nest and reset layering; `CdsLayer` advances the surface level. Reference the `-01` token keys and let the nearest layer resolve them. Use dynamic color resources, spacing/sizing constants, `CdsText` type styles and Carbon icons rather than fixed hex colors, custom font sizes or Unicode substitutes. Confirm icon names in `Generated/CdsIcons.g.cs`.

## Bind a table

```xml
<cds:CdsDataTable Title="Clusters" ItemsSource="{Binding Clusters}"
                   IsSortable="True" IsSearchable="True" Selection="Multi">
    <cds:CdsDataTableHeader Key="Name" Header="Name" Width="*" />
    <cds:CdsDataTableHeader Key="Region" Header="Region" Width="140" />
</cds:CdsDataTable>
```

- Match header keys to the row's properties; see the value selectors and templates for complex cells. Use fixed or star column widths.
- Large data: `StickyHeader="True"` gives the table its own virtualized scrolling, so give it a bounded height. Inside a vertical `ScrollView` it virtualizes against the page's scrolling instead (`VirtualizeInScrollView`, default true; virtualized rows are one line high).
- Views are reused when virtualized, so bind every value in cell and expanded-row templates.
- A table outside a scroll view, with an `Auto` column, or with `VirtualizeInScrollView="False"` builds every row: page it with `CdsPagination`. The pager does not fetch or slice data; wire it to the rows you show.
- `CdsTable` and its parts are the manual, static alternative.

## Plot a chart

Charts are native controls (not a WebView) with a built-in title, legend, toolbar, tooltips, accessible data cursor and loading skeleton. Most take `Data` as a list of `CdsChartPoint(group, key | date | x, value)` plus `CdsChartAxis` settings; hierarchy, flow, network, bullet, gauge, meter and map charts have their own shapes. Read [charts](references/charts.md) before writing one: it has an example and the data shape, axis rules and options for each chart type. To choose a chart, use the maui-carbon-design skill's `references/data-visualization.md`.

## Navigation and overlays

- App frame: `CdsUIShell` with header and side navigation.
- `CdsTabs` with `CdsTab` for related panels; `CdsContentSwitcher` with `CdsSwitch` for views of the same content. See the gallery for selection and close-request handling.
- `CdsModal` for standard dialogs (`IsOpen` binds two-way; handle submit and close requests as the gallery does); `CdsComposedModal` with header/body/footer parts for custom structure. Modal scrims cover their page, including a Carbon shell, but not native MAUI Shell chrome.
- Inline notifications for contextual messages, actionable ones when a response is needed, and a toast for a transient message:

```csharp
CdsToast.Show(this, new CdsToastOptions
{
    Kind = CdsNotificationKind.Success,
    Title = "Cluster created",
    Timeout = 5000,
});
```

Do not auto-dismiss errors. Give icon-only actions an accessible name through the component's own API.

## Verify

- Build the app for each target platform. Check light and dark themes, nested layers, narrow and wide layouts, keyboard focus, and the invalid, disabled, read-only and loading states that apply.
- The library's visual review is done on Mac Catalyst; look at the other platforms you ship yourself.
- On iOS/macOS 27 the app must adopt the UIScene lifecycle; copy the gallery's `Platforms/*/Info.plist` entries and `SceneDelegate.cs` from the repository.
- A slow or over-busy screen: listen to the `Maui.Carbon` meter (`dotnet-counters monitor --counters Maui.Carbon`, a `MeterListener` or OpenTelemetry; instrument names on `CdsMetrics`). Optimized iOS, Mac Catalyst and Android builds need `<MetricsSupport>true</MetricsSupport>`.
