# Maui.Carbon conventions

How to add a Carbon component to Maui.Carbon. Read this whole file before you write code. The exemplars are:
`CdsButton` (`src/Maui.Carbon/Components/Actions/Button/`), `CdsTag` (`Components/Feedback/Tag/`),
`CdsTextInput` (`Components/Forms/TextInput/`), `CdsPopover` and `CdsTooltip` (`Components/Overlays/`).
Copy their structure.

## 1. Sources of truth

- `design/carbon/components/<Name>/README.md`: what the component is, the props, the rules, accessibility, what the
  web version leaves out. **Implement every prop and rule that makes sense in MAUI.**
- `design/carbon/components/<Name>/preview.html`: the examples the gallery page should mirror.
- `design/carbon/components/index.d.ts`: the prop types of the web version.
- `design/carbon/components/bundle.css`: exact measurements (padding, heights, gaps, which token in which state).
  Search it with `grep -n 'cds--<name>' design/carbon/components/bundle.css`.
- `design/carbon/README.md`: the design language (layers, focus, states, content rules).
- Never invent token values or measurements. When the CSS says `padding-inline: 1rem`, that is `CdsSpacing.Spacing05`.

## 2. Ownership: what you may touch

Each work package owns only:

- `src/Maui.Carbon/Components/<Category>/<Component>/**` for each of its components
- `samples/Maui.Carbon.Gallery/Pages/<Category>/<Component>Page.xaml(.cs)` for each of its components
- `samples/Maui.Carbon.Gallery/Pages/<Category>/Data/<Package>*.cs` for sample data, if needed
- `tests/Maui.Carbon.Tests/Components/<Component>*Tests.cs`

**Read-only for everyone** (ask in your report instead of editing): `src/Maui.Carbon/Foundation`, `Overlays`,
`Theming`, `Typography`, `Motion`, `Icons`, `Hosting`, `Generated`, `Platforms`, `design/`, the gallery's
`Infrastructure/`, `App.*`, `MauiProgram.cs`, every `.csproj`, `Directory.*.props`, `.slnx`, CI and `tools/`.
If you genuinely need a foundation change (a new platform hook, a new token helper), stop and describe it in your
report; the orchestrator implements it between waves. Do not add NuGet packages.

Categories (folder names): `Actions`, `Forms`, `FluidForms`, `Data`, `Navigation`, `Containers`, `Overlays`,
`Feedback`, `AI`, `Layout`, `Patterns`.

## 3. Files per component

All public types live in the single namespace `Maui.Carbon` (folders are for humans).

| File | Contents |
|---|---|
| `Cds<Name>.cs` | the class, constructor (builds the chrome), event wiring, `ApplyVisuals`, layout code |
| `Cds<Name>.Properties.cs` | `partial` class with bindable properties only (optional for small controls) |
| `Cds<Name>Tokens.cs` | **pure** static resolver: `(kind, CdsInteractionState) → color keys` (public, unit-tested) |
| `<Name>Model.cs` | pure logic with no MAUI views (paging math, calendar grids, filtering) — `internal`, unit-tested |
| `Cds<Name>Enums.cs` | enums (`Cds<Name>Kind`, `Cds<Name>Size`) |
| sub-parts | `CdsAccordionItem.cs`, `CdsTableRow.cs`… in the same folder |

## 4. Base classes

- Interactive control → derive from `CdsControl` (or `CdsContainer` when it hosts consumer content in `Body`).
  - Build the visual tree in C# (no XAML in the library) and call `SetChrome(root)` once. Never set `Content`.
  - Override `ApplyVisuals()` and set every color there with `SetColor(target, prop, key)` / `SetBrush(target, brushProp, key)`.
    Call `RefreshVisuals()` from property-changed callbacks. `ApplyVisuals` runs on every state change.
  - Tappable/clickable/focusable regions: `CreateHitTarget(CdsAccessibilityRole.X)` gives a native button that
    provides focus, Tab order, Enter/Space, press state and screen-reader focus. Put it **last** (on top) in the same
    grid cell as the visuals; mark the visuals `InputTransparent = true` and
    `AutomationProperties.SetExcludedWithChildren(visuals, true)`. Override `OnActivated()` for clicks.
    Set `hit.IsChecked / IsSelected / IsExpanded` and `SemanticProperties.SetDescription(hit, name)`.
  - Native text input → `CdsNativeEntry` / `CdsNativeEditor` / `CdsNativePicker` (chrome already stripped) inside a
    `CdsFieldChrome`, with `TrackFocus(entry)`.
  - Commands: `SetCommand(Command, CommandParameter)` from the property-changed callback, `ExecuteCommand()` in
    `OnActivated`. `IsEnabled` follows `CanExecute` automatically.
- Pure layout/text → derive from the MAUI type (`CdsText : Label`, `CdsStack : StackLayout`, `Layout` + `ILayoutManager`).
- Theming/layer scopes: never set `Resources` yourself; use `CdsTheme` / `CdsLayer`.

## 5. Mapping the web API (index.d.ts) to MAUI

| Web | MAUI |
|---|---|
| `onClick` | `event EventHandler? Clicked` + `ICommand Command` + `object CommandParameter` |
| `onChange(value)` | two-way bindable property (`BindingMode.TwoWay`) + `event EventHandler<ValueChangedEventArgs<T>>`-style event named `<Thing>Changed` |
| `value` / `defaultValue` / `selectedItem` / `initialSelectedItem` | one two-way property (`Value`, `SelectedItem`, `IsChecked`…) |
| `disabled` | MAUI `IsEnabled` (do not add a `Disabled` property) |
| `invalid` / `invalidText` | `IsInvalid` / `InvalidText` |
| `warn` / `warnText` | `IsWarning` / `WarnText` |
| `readOnly` | `IsReadOnly` |
| `open` / `expanded` / `selected` / `checked` | `IsOpen` / `IsExpanded` / `IsSelected` / `IsChecked` (two-way when the user can change it) |
| `labelText` / `titleText` | `Label` |
| `hideLabel` | `HideLabel` |
| `renderIcon="add"` | `CdsIconName? Icon` (`Icon="Add"` in XAML) |
| `size="sm"` | shared `CdsSize` enum (`Xs, Sm, Md, Lg, Xl, Xxl`) unless the component has its own size names |
| `kind` / `type` | `Kind` / `Type` enum named `Cds<Name>Kind` |
| `children` | `Body` (single view) or `Items` (`IList<T>` + `ItemsSource`/`ItemTemplate` for data) |
| `aria-label` / `iconDescription` | `SemanticProperties.Description` on the hit target; an `IconDescription` property for icon-only controls |
| `className`, `id`, `as`, `translateWithId`, `style` | drop |

Item collections: prefer `IEnumerable? ItemsSource` + `DataTemplate? ItemTemplate` (or a `Func<object, string>`
`ItemToString`, mirroring Carbon's `itemToString`) for data-driven controls, and an `IList<TItem> Items` content
property of `Cds<Name>Item` children for declarative ones (`CdsTabs` → `CdsTab`). Use `ObservableCollection` +
`CollectionChanged` to rebuild.

## 6. Bindable properties

```csharp
public static readonly BindableProperty KindProperty = BindableProperty.Create(
    nameof(Kind), typeof(CdsButtonKind), typeof(CdsButton), CdsButtonKind.Primary,
    propertyChanged: static (b, _, _) => ((CdsButton)b).UpdateLayout());

public CdsButtonKind Kind
{
    get => (CdsButtonKind)GetValue(KindProperty);
    set => SetValue(KindProperty, value);
}
```
- CLR wrappers contain no logic. Clamp with `coerceValue`. `BindingMode.TwoWay` only for values the user edits.
- Every public member has an XML doc comment (`<summary>`); missing docs fail the build. Put the Carbon rule in
  the summary ("one primary per view").
- `<see cref="X"/>` must resolve; for inherited members write `<see cref="CdsContainer.Body"/>`.

## 7. Tokens and visuals

- Colors only through `SetColor` / `SetBrush` / `CdsTokens.*` with `CdsColorKeys.*`. **Only `-01` layer tokens**
  (`Layer01`, `Field01`, `BorderSubtle01`, `LayerHover01`…): `CdsLayer` remaps them. Never `-02`/`-03`.
- **No hex values, no `Color.FromArgb`, no `Colors.X` except `Colors.Transparent`, no
  `Application.Current.Resources[...]`, no `AppThemeBinding`.**
- Spacing/size/radius only from `CdsSpacing`, `CdsSizing`, `CdsRadius`, `CdsSizeExtensions.Height()`. The system is
  square: radius only for tags (pill), toggles (pill + circle), checkboxes (2px) and popovers (2px).
- Type only through `CdsText` (`TypeStyle`, `Tone`) or `CdsTypography.Apply(...)`. Never `FontAttributes.Bold`
  (use `CdsTypeStyle.HeadingCompact01` or `CdsFonts.SansSemiBold`).
- Hairlines: `BoxView` 1px high/wide with `BoxView.ColorProperty`. Outlines: `Border` + `SetBrush(border, Border.StrokeProperty, key)`.
- Shadows (menus, popovers only): `CdsTokens.SetShadow(view, CdsColorKeys.Shadow)`.
- Icons: `new CdsIcon(CdsIconName.X, 16)`; color with `SetColor(icon, CdsIcon.ColorProperty, key)`. Use only icons in
  `CdsIconName` (see the Icons gallery page). Status glyphs: `ErrorFilled`, `CheckmarkFilled`, `WarningFilled`,
  `WarningAltFilled`, `InformationFilled`.
- State colors: hover → `*-hover` token, pressed → `*-active`, selected → `*-selected`, disabled →
  `text-disabled`/`icon-disabled`/`button-disabled` (never lower `Opacity` for disabled).
- Focus: `CdsFocusRing(CdsFocusRingStyle.Inset|Offset|Button)` in the same cell above the visuals,
  `ring.IsActive = Is(CdsInteractionState.FocusVisible)` (fields: `Focused`).
- Color transitions are instant in MAUI; animate only geometry/opacity/rotation, and only through `CdsMotion`
  (`CdsMotion.AnimateAsync`, or `view.FadeToAsync/RotateToAsync/TranslateToAsync` guarded by
  `CdsMotion.IsReducedMotion`) with `CdsDuration.*` and `CdsEasing.*`.

## 8. Overlays and keyboard

- Anything that floats (menus, list boxes, popovers, tooltips, modals, toasts, calendars) goes through
  `CdsOverlayService.Open(view, new CdsOverlayOptions { Anchor = this, Placement = …, LightDismiss = …, IsModal = … })`.
  Never use MAUI popups, `DisplayAlert`, or modal pages. Keep the returned `CdsOverlay` and `Close()` it; listen
  to `Closed` to sync your `IsOpen`.
- Floating surfaces with a caret use `CdsPopoverSurface`; menus and list boxes build their own surface (`layer-01`
  fill, `CdsTokens.SetShadow`).
- Menus and list boxes use **virtual focus**: keep focus on the trigger, track a highlighted index, and pass
  `Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation` plus a `KeyHandler` in the options. Escape closes by default.
- Composite controls that move focus with arrows (tabs, radio groups, content switcher, tree view) override
  `WantsNavigationKeys => true` and `OnKeyDown(CdsKeyEventArgs e)`.
- `CdsOverlayService` needs a page host (every ContentPage gets one automatically). Open overlays only when
  `IsLoaded` is true.

## 9. Accessibility checklist

- Every control has a name (`SemanticProperties.SetDescription` on the hit target / native input), a role
  (`CdsAccessibilityRole`), and its state (`IsChecked`, `IsSelected`, `IsExpanded`).
- Helper / error text → `SemanticProperties.SetHint` on the input.
- Headings: `SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level2)` where Carbon renders a heading.
- Live regions (counts, toasts, inline loading) → `SemanticScreenReader.Announce(text)` when they change.
- Disabled controls are not focusable (`IsEnabled = false` does it); read-only controls stay focusable.
- Icon-only controls require a description; decorative icons stay out of the tree (`CdsIcon` default).
- Never use color alone: status = icon + text.

## 10. Gallery page (one per component)

`samples/Maui.Carbon.Gallery/Pages/<Category>/<Component>Page.xaml` + `.xaml.cs`, copying
`Pages/Actions/ButtonPage.xaml`:

- Root `<g:GalleryPage>` with `xmlns:cds="https://github.com/waring-ai/Maui.Carbon"` and
  `xmlns:g="clr-namespace:Maui.Carbon.Gallery.Infrastructure"`.
- Code-behind class `partial`, derives `GalleryPage`, carries
  `[GalleryPage("<Display name>", GalleryCategory.<Category>, Order = n, Spec = "<README folder>", Summary = "<README first sentence>")]`.
  The page appears in the flyout automatically — never edit `AppShell`.
- Content: a `VerticalStackLayout Spacing="48"` of `g:GalleryExample` sections that mirror `preview.html`:
  every kind and size, every state (normal, disabled, invalid, warning, read-only, selected, skeleton, as
  applicable), an **interactive** example wired to a `cds:CdsText` status label, a `g:ThemeStrip` showing the
  control in all four themes, and (for anything that sits on surfaces) a `g:ThemeStrip Layered="True"`.
- Bindings in XAML need `x:DataType`; prefer code-behind event handlers for the demos.
- Use realistic, sentence-case Carbon content ("Create cluster", "prod-eu-de-1"), never lorem ipsum.

## 11. Tests

`tests/Maui.Carbon.Tests/Components/<Component>Tests.cs` (xunit v3):
- the token resolver for each kind × state that the README specifies (e.g. disabled primary → `ButtonDisabled`);
- every model (pure logic) thoroughly: edge cases, clamping, keyboard index movement;
- constructing the control and setting each property must not throw (controls can be created without a platform).

## 12. Build and verify

This machine hangs MSBuild unless node reuse and shared compilation are off. Always:

```bash
export MSBUILDDISABLENODEREUSE=1
tools/verify.sh --fast          # inner loop: generator check, library net10.0, tests
tools/verify.sh                 # before you finish: + maccatalyst, ios, android & windows compile checks
tools/verify.sh --gallery       # + the gallery for Mac Catalyst
```
Zero warnings (warnings are errors), zero test failures. If you started in a git worktree, first run
`git merge --ff-only main` (worktrees can start from an old commit).

You cannot launch the gallery app from an agent session (no window server access). Verify by building; the
orchestrator reviews screenshots of your pages after merging. Make every page render meaningfully without
interaction (open states shown via properties, not clicks) so screenshots show the component.

## 13. Definition of done and report

- Builds clean on every target with `tools/verify.sh --gallery`; tests green.
- XML docs on every public member; every README prop mapped or listed as left out.
- Commit on your branch with a plain message (no `Co-Authored-By` or other attribution trailers).
- Report: per component, what you left out versus its README and why (MAUI limitation, missing icon…), any
  deviation from these conventions, and any foundation change you need.
