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

### MAUI layout traps (learned the hard way)
- `HorizontalOptions=Fill` + `MaximumWidthRequest` **centers** the view once space exceeds the max. For "fill up to a
  max, start-aligned" (notifications, code snippets, file rows) wrap the content in `CdsMaxWidthBox` (Foundation).
- `HorizontalOptions=Start` shrinks a Grid to its content even with Star columns.
- Star columns in an auto-sized parent can under-measure children and truncate labels. For "equal widths sized to the
  widest child" (segmented controls, button sets) use `CdsEqualWidthRow` (Foundation).
- Views switched often and costly to build (app screens, large panes) go in a `CdsDeck` (Foundation), not a Grid of
  views toggled with `IsVisible`.
- A control sets its own `HorizontalOptions` only as a constructor default, never when it rebuilds: a rebuild runs
  again after XAML applied the app's attributes and would overwrite them. To fill the width it is offered (fluid sets,
  equally spaced steps, fluid fields), call `SetChrome(chrome, fillsWidth: true)`; the app's alignment and
  `WidthRequest` still apply.
- On Mac Catalyst a `Border` paints its background and stroke half a point inside its frame. Put a fill that must
  cover the whole box (button faces) on the layout around the Border.
- `HorizontalStackLayout` measures children unconstrained: cap a label's `MaximumWidthRequest` to get an ellipsis.
- A Label with `LineBreakMode.NoWrap` and no `MaxLines` draws one line: set `MaxLines` for multi-line monospace text.
- In a row of items with different heights, set `VerticalOptions=Start` so a short item isn't stretched to its neighbour.
- `FlexLayout` applies a child's margins twice (in the flex slot and again in the child's arrange): a child with a
  `WidthRequest` moves by half its margins (`Margin="0,0,24,24"` starts 12 left of the edge). In the gallery, wrap
  rows with `g:WrapLayout`, which hands each child the slot its arrange expects.
- `ContentView` measures its content even when the content is hidden. `CdsControl` measures to zero while its chrome
  is hidden, so hide a part by hiding the root you passed to `SetChrome`, not by sizing it.

### Lifetime and performance traps (found by `tools/relay-exercise.sh`)
- A self-drawing `GraphicsView` sets `Drawable = new CdsWeakDrawable(this)`, never `Drawable = this`. On iOS and Mac
  Catalyst a platform view in a hierarchy keeps its managed peer alive, so anything a platform view holds strongly must
  not lead back to the page: that is a cycle through native code the GC never collects, and it leaks the whole page.
- Subscribe to longer-lived sources (a view model's command, the shell, a static event) weakly
  (`CdsWeakSubscription<T>`) or only while `Loaded`. `SetCommand` and `CdsBreakpointWatcher` already do.
- Child collections are `CdsItemCollection<T>`, not `ObservableCollection<T>`: its `Clear()` raises one `Remove`
  listing the items, where `ObservableCollection` raises a `Reset` with no `OldItems` (old children would stay).
- A control that rebuilds its views on every collection change requests the rebuild through `CdsCoalescedAction`, so
  filling a shown list costs one rebuild, not one per item.
- `SetColor`/`SetBrush` remember the applied key on the target, so discarded views can be collected; `ForgetColors` is
  only needed to force colors to apply again.
- Hover tracking comes with `CreateHitTarget`/`TrackFocus`; call `TrackHover()` only for a control that reacts to hover
  without either. Every gesture recognizer costs platform setup each time its page enters or leaves the window.
- Moving a large screen into and out of the window costs MAUI hundreds of milliseconds (window and resource
  propagation over every element, native subviews and gestures), and so does hiding it with `IsVisible`: MAUI pushes the
  effective visibility to every descendant, one native update each. Keep long-lived screens attached in a `CdsDeck`
  (Foundation): only its `Active` child is laid out and shown, the others are hidden on their own native view. Code that
  asks whether something is on screen uses `CdsDeck.IsShown`/`IsHidden`, not `IsVisible` alone.
- A view removed for good (a screen leaving the back stack, a closed overlay's container) gets `DisconnectHandlers()`:
  a removed subtree keeps its native views and the Loaded and window hooks MAUI put on them, which on iOS and Mac Catalyst
  hold the managed views, and through their event handlers the page. `CdsOverlayHost` does it for overlays.
- Endless animations go through `CdsLoop`. A loop whose owner is hidden (or in an inactive deck child) parks off the
  shared clock until shown again, so a loading skeleton hidden when its data arrives, or a screen kept hidden, costs no
  frames.
- Rebuilds keep the child views already in place (`CdsContainedList` and `CdsList` sync their rows) instead of clearing
  and re-adding them: each re-add costs the child its native view and a layout pass.
- `CdsHitTarget` skips MAUI Button's native title and image layout on iOS and Mac Catalyst (`ICrossPlatformLayout`);
  keep it chrome-less (no text, no image) or that shortcut is wrong.
- Every element costs MAUI its handler, native subview, resource propagation and (on Apple) a safe-area walk up to the
  window, about 0.5 ms on Mac Catalyst; a table builds dozens per row. Parts a control rarely shows are created on first
  use, not in the constructor: `CdsFocusRing` draws its outline when first shown, `CdsFormMessage` its icon and text
  with the first message, `CdsActionFace` its label (not icon-only), badge and outline, `CdsCheckbox` its label (set
  `HideLabel` before `Label`) and message row. Follow that for new optional parts.
- Per-row hover in `CdsTable` is one pointer recognizer on the body grid that finds the row by position (one per pooled
  slot when virtualized), not one per cell. Tool cells (flush, no column name: selection, expand, row menu) go into the
  row without a cell host, and an expandable row's child row is built on its first expansion.
- `CdsDataTable` virtualizes against its nearest vertical `ScrollView` ancestor (`CdsTable.PageScroll`) when it has no
  sticky header and no `Auto` column: header and body are separate grids, the body is as tall as all rows, and the
  viewport is the scroll view's less the body's offset in its content. Rows in view are built at once; new slots for
  overscan rows wait for the next turn of the run loop. The ancestor is looked up again on each refresh, on `Loaded` and
  when the table's parent changes.

### Metrics (`CdsMetrics`, `Foundation/CdsMetrics.cs`)
- The library's `System.Diagnostics.Metrics` instruments live in `CdsMeter`; their public names are constants on
  `CdsMetrics`. Durations are histograms in seconds tagged `carbon.component` with the control type.
- The shared choke points already measure every control: `CdsWeakDrawable` (draws), `CdsControl` (`ApplyVisuals`,
  measure, arrange, activation), `CdsCoalescedAction` (rebuilds), `CdsThemeManager.SetTheme` and the `CdsLoop` clock.
  Use them and a new control is covered for free; don't add per-control instruments for the same work.
- New measured work: `var start = CdsMeter.Start(CdsMeter.X); try { ... } finally { CdsMeter.Stop(CdsMeter.X, start, this); }`.
  `Start` returns 0 when nothing listens, so keep anything costly (tag strings, `GetType().Name`) behind `Stop`.
- `tools/relay-exercise.sh` listens (`samples/Maui.Carbon.Relay/Hosting/ExerciseMetrics.cs`). Relay sets
  `MetricsSupport=true` because optimized Apple and Android builds trim metrics away.

## 8. Overlays and keyboard

- Anything that floats (menus, list boxes, popovers, tooltips, modals, toasts, calendars) goes through
  `CdsOverlayService.Open(view, new CdsOverlayOptions { Anchor = this, Placement = …, LightDismiss = …, IsModal = … })`.
  Never use MAUI popups, `DisplayAlert`, or modal pages. Keep the returned `CdsOverlay` and `Close()` it; listen
  to `Closed` to sync your `IsOpen`.
- Floating surfaces with a caret use `CdsPopoverSurface`; menus and list boxes build their own surface (`layer-01`
  fill, `CdsTokens.SetShadow`).
- Menus and list boxes use **virtual focus**: keep focus on the trigger, track a highlighted index, and pass
  `Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation` plus a `KeyHandler` in the options. Escape closes by default.
- Composite controls that move focus with arrows (tabs, radio and tile groups, content switcher, structured list,
  tree view) override `WantsNavigationKeys => true` and `OnKeyDown(CdsKeyEventArgs e)`, and keep one Tab stop with
  `CdsRovingFocus` (`RemembersFocus = false` and `PreferredIndex` = the selected index when selection follows focus).
  Secondary buttons inside them (tab scroll arrows, number steppers) set `IsTabStop = false`.
- A virtual-focus menu or list closes when its trigger loses focus, with `ReturnFocus = false` so the Tab that moved
  focus is not undone. It announces the highlighted item (label, state, "2 of 5") through `CdsAnnouncer` on open and
  on every keyboard move.
- Overlays that take no keys (toasts) are passive: key routing skips them, so they never take Escape or arrows from a
  modal or menu. Modals and dialogs without `ReturnFocusTo` return focus to the control focused before they opened.
- Decide a responsive layout while measuring (a custom layout, or `CdsContextKeys.ViewportWidth`), never by changing a
  Grid's rows or columns from `SizeChanged`: the new structure is arranged with the old measure.
- `CdsOverlayService` needs a page host (every ContentPage gets one automatically). Open overlays only when
  `IsLoaded` is true.

## 9. Accessibility checklist

- Every control has a name (`SemanticProperties.SetDescription` on the hit target / native input), a role
  (`CdsAccessibilityRole`), and its state (`IsChecked`, `IsSelected`, `IsExpanded` (false, not null, while the panel
  or menu a trigger opens is closed), `IsReadOnly`, `CurrentKind` for the current page instead of `IsSelected`).
- Name a group (radio, checkbox or tile group, form group, toolbar, tab row, list, menu or list-box surface, dialog
  surface, nav or header landmark) with `CdsSemantics.SetGroup(view, name, kind, hint)`. Never put
  `SemanticProperties.Description` or `Hint` on a layout, Border, ScrollView or ContentView with interactive
  descendants: on iOS and Mac Catalyst it becomes one VoiceOver element and hides them. `ContainerSemanticsAuditTests`
  enforces this. A Description an app sets on a Cds control is forwarded by `CdsControl` to its hit target.
- Name row-level controls by their row ("Select row, <name>"); a screen reader has no table context.
- Helper / error text → `SemanticProperties.SetHint` on the input. Error and warning messages are alerts: spoken once,
  assertively, when they appear on a loaded control (`CdsMessageAlert`; `CdsFieldChrome` and `CdsFormMessage` do it).
- Adjustable controls set `CdsHitTarget.AccessibleValue` to the formatted value (never a hint) and step on
  `Incremented`/`Decremented` as the arrow keys do.
- Headings: `SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level2)` where Carbon renders a heading.
- Live regions (counts, notifications, inline loading) → `CdsAnnouncer.Announce(text)` (polite) or
  `CdsAnnouncer.Announce(text, CdsAnnouncePriority.Assertive)` for alerts, never `SemanticScreenReader.Announce`.
  Announce only while the element is effectively shown (loaded, every ancestor visible), and coalesce related changes.
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
  control in all four themes, and (for anything that sits on surfaces) a `g:ThemeStrip Layered="True"`. Let the
  example fill its zone rather than fixing a width; the strip wraps to two columns when an example is wider than a
  quarter of the page, and `MinimumZoneWidth` keeps content that scales down (diagrams) from shrinking.
- Bindings in XAML need `x:DataType`; prefer code-behind event handlers for the demos.
- Use realistic, sentence-case Carbon content ("Create cluster", "prod-eu-de-1"), never lorem ipsum.

## 11. Tests

`tests/Maui.Carbon.Tests/Components/<Component>Tests.cs` (xunit v3):
- the token resolver for each kind × state that the README specifies (e.g. disabled primary → `ButtonDisabled`);
- every model (pure logic) thoroughly: edge cases, clamping, keyboard index movement;
- constructing the control and setting each property must not throw (controls can be created without a platform).
- announcements: `using var heard = CdsAnnouncer.Capture();`, then assert on `heard.Announcements`.
- a loaded control (for `IsLoaded`-gated behaviour and overlays): `new Window(new ContentPage { Content = view })`.
  Keep the root in a variable for the whole test; an element holds its parent weakly.
- tests that set the process-wide `CdsFocusTracker` join `[Collection(FocusTrackerCollection.Name)]`.

## 12. Build and verify

This machine hangs MSBuild unless node reuse and shared compilation are off. Always:

```bash
export MSBUILDDISABLENODEREUSE=1
tools/verify.sh --fast          # inner loop: generator check, library net11.0, tests
tools/verify.sh                 # before you finish: + maccatalyst, ios, android & windows compile checks
tools/verify.sh --samples       # + the gallery and the Relay sample for Mac Catalyst (--gallery still works)
```
Zero warnings (warnings are errors), zero test failures. If you started in a git worktree, first run
`git merge --ff-only main` (worktrees can start from an old commit).

You cannot launch the gallery app from an agent session (no window server access). Verify by building; the
orchestrator reviews screenshots of your pages after merging. Make every page render meaningfully without
interaction (open states shown via properties, not clicks) so screenshots show the component.

## 12a. Charts

The data visualization components (`Components/Charts/`) draw differently from the other controls. Read
`Components/Charts/Core/` and the exemplars (`SimpleBarChart`, `LineChart`) before adding or changing one.

- A chart derives from `CdsChart` (or `CdsCartesianChart` for axis charts) and builds an immutable `ChartScene` for
  its plot size in `BuildScene` / `BuildMarks`: shapes, `ChartText` labels and `ChartMark`s. The hooks are
  `private protected`: charts are not an extension point for apps.
- One `GraphicsView` draws the shapes; colors are `ChartPaint` references (theme keys, `CdsChartToken`s, series,
  sequential or diverging steps) resolved for the theme and layer at draw time, so a theme change only redraws.
  Never hex colors. The chart palettes are generated by `tools/carbon-charts/build-maui.mjs`.
- Text is never drawn on the canvas: every `ChartText` becomes a pooled `CdsText` label over it (IBM Plex Sans
  Condensed for ticks and legends, font auto-scaling off). Layout measures text with `CdsChartTextMetrics` (generated
  from the embedded fonts), so scenes are the same on every platform and in tests.
- Hover and the data cursor only change which mark is active and redraw; they never rebuild the scene. Marks are also
  the keyboard and screen-reader path: one adjustable hit target over the plot steps through them (arrow keys,
  VoiceOver and TalkBack adjust gestures) and shows each mark's tooltip. Add marks in reading order with a spoken
  `Description`, and keep "Show as table" complete: it is the accessible way to every value.
- Test charts without a platform through `RenderScene(width, height)` and `RenderTable()`.

## 13. Definition of done and report

- Builds clean on every target with `tools/verify.sh --gallery`; tests green.
- XML docs on every public member; every README prop mapped or listed as left out.
- Commit on your branch with a plain message (no `Co-Authored-By` or other attribution trailers).
- Report: per component, what you left out versus its README and why (MAUI limitation, missing icon…), any
  deviation from these conventions, and any foundation change you need.

## 14. The Relay sample app

`samples/Maui.Carbon.Relay` is the library used the way a product team would use it: one `CdsUIShell` frame, MVVM
with CommunityToolkit.Mvvm, DI and an in-memory backend. When a component changes in a way apps feel (a renamed
property, a new required setup step), update Relay too.

- **Core vs app.** `samples/Maui.Carbon.Relay.Core` is plain `net11.0` and holds the models, mock services, pure
  logic and view models. It never references MAUI or Maui.Carbon, so `tests/Maui.Carbon.Relay.Tests` can test it.
  The app maps domain enums to Carbon in `Presentation/CarbonMap.cs` and `Converters.cs`.
- **Adding a screen.**
  - Add `ViewModels/<Screen>ViewModel.cs` (derive `PageViewModel`, load in `OnActivatedAsync`, partial-property
    `[ObservableProperty]` only) and a route in `Abstractions/Routes`.
  - Add `Views/<Screen>View.xaml` (`x:DataType` on every XAML and template).
  - Register both, and `Navigator.Map`, in `MauiProgram.cs`. Add the side-nav item and its route mapping in
    `Pages/MainPage`, and a shot in `Hosting/RouteSnapshotRunner`.
- **Navigation.** `CdsUIShell` has none of its own. `Hosting/Navigator` swaps the shell's `Body` and activates the
  view model.
- **Logging.** `Hosting/AppLog` sets up Serilog with a text file sink: `relay-<date>.log` in `RELAY_LOG_DIR` (or
  `--log-dir`), the screenshot directory in screenshot mode, else `<AppData>/Logs/<app id>` (`~/Library/Logs/...` on Mac Catalyst). Log through an injected
  `ILogger<T>` in the app (Core stays logging-free; its failures reach the log as error toasts through
  `UserFeedback`). Unhandled and unobserved exceptions, failed screen loads and MAUI's own warnings are logged
  already. A caught exception the user never sees as a toast should be logged at Error.
- **Data-binding declarative item lists.** Lists like `CdsContainedList.Items` and `CdsStructuredList.Rows` take
  child elements rather than an `ItemsSource`; bind them with `Presentation/ItemsBinding`.
- **Verify.**
  - `tools/verify.sh --samples` builds Relay and runs its tests.
  - `APP=relay tools/snapshots.sh out/relay white,g100` signs in and captures every screen and open state, then
    prints the log's warning count and any `[ERR]`/`[FTL]` entries.
  - `tools/relay-exercise.sh out/exercise` (`Hosting/ExerciseRunner`) drives every screen, open state and interaction
    in every theme and window size and writes `exercise-report.md`: logged warnings and errors with the step and stack
    that caused them, layout overflow, low contrast, collapsed or unsettled layout, leaked element types (with what
    holds them), UI-thread hangs, and the crash stack if the app dies. Run it after changing a component Relay uses.
