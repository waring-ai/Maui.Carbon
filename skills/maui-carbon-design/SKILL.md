---
name: maui-carbon-design
description: Design .NET MAUI screens that follow the Carbon Design System's principles with Maui.Carbon (Waring.Maui.Carbon). Use when planning or laying out a page, dialog, form, dashboard or flow; deciding visual hierarchy, layering, color, type, spacing, grid or motion; choosing between similar components for a job; writing UI copy, labels, errors and notifications; designing empty, loading, error, read-only and disabled states; making a screen accessible; or reviewing an existing screen for Carbon conformance. For a control's API, properties and bindings, use the maui-carbon skill.
---

# Designing with Carbon in Maui.Carbon

Carbon is a productive, enterprise design language: the interface is a tool, so it is flat, square, gray with one interactive blue, dense but calm, and written in plain words. This skill covers what a screen should look like and say; the **maui-carbon** skill covers each control's properties, bindings and setup, so load it as well when you write the XAML or C#.

## Workflow for a new screen

1. **Name the job.** What does the person come here to do, and what is the one main action? Find the matching pattern in [patterns](references/patterns.md) and copy its structure; it names a gallery page in the library's repository to read.
2. **Frame it.** `CdsUIShell` (header, side nav) for global navigation; each page starts with its title (`CdsPageHeader`, or a level-1 heading) and a breadcrumb when it is deep in a hierarchy. Lay out on the 2x grid (`CdsGrid`/`CdsColumn`) or a bounded single column; stack content with `CdsStack` and spacing-scale gaps.
3. **Set the hierarchy.** Heading levels follow the outline, not the size you want (`CdsHeading` inside `CdsSection` keeps them right). Secondary detail goes behind disclosure (`CdsAccordion`, `CdsExpandableTile`, "Show more"), extra actions into a `CdsOverflowMenu`.
4. **Pick components by need** with [choosing components](references/choosing-components.md). Never restyle a control to look like another one.
5. **Build surfaces with layers, not shadows.** The page is `CdsBackground` and a container on it `CdsLayer01`; wrap what sits on that container in `CdsLayer` so its fields and borders take the next level. `CdsTheme` makes a region dark or light. See [foundations](references/foundations.md).
6. **Write the words** with [content](references/content.md).
7. **Design every state,** not only the happy path: first use (empty), loading, no results, error, partial failure, read-only, disabled with a reason, long text, many items, narrow width, light and dark themes.
8. **Make it accessible** from the start with [accessibility](references/accessibility.md), then review against the checklist below.

For charts and dashboards, also read [data visualization](references/data-visualization.md).

## The rules that matter most

- **Tokens only.** Colors are `{DynamicResource Cds…}` keys (`CdsColorKeys`), never hex. Spacing comes from `CdsSpacing` (2, 4, 8, 12, 16, 24, 32, 40, 48, 64, 80, 96, 160), control heights from `CdsSizing`/`CdsSize` (24, 32, 40, 48, 64, 80), text from `CdsText` with a `TypeStyle` and `Tone`, motion from `CdsDuration`/`CdsEasing`, icons from `CdsIcons`. An off-scale value is a bug.
- **Blue means interactive.** Everything else is gray. Status colors (error, success, warning, info) always come with an icon and a word. Tag colors categorize; they never mean status.
- **Square and flat.** No corner radius on buttons, inputs, tiles, notifications or modals; no gradients or decorative shadows; no decorative illustration in task UI. Separate content with 1px `CdsBorderSubtle01` rules or a layer step.
- **One primary action per view.** Pair it with secondary or ghost buttons; put three or more related actions in a `CdsButtonSet` plus a menu. Danger actions always have text.
- **Labels stay visible.** Placeholders are examples, not labels. `HideLabel` only where context names the field (a table row) and the label still reaches screen readers.
- **Validate on blur or submit,** never on every keystroke; after a failed submit put a summary `CdsInlineNotification` above the form and keep the field errors.
- **Read-only is not disabled.** Read-only values stay focusable, full contrast and copyable; disabled controls say why nearby. Hide what a person can never do.
- **Friction matches impact.** Low impact: act and offer Undo in a toast. Moderate: a danger `CdsModal` (`IsDanger`) naming the object. High: make the person type the resource name.
- **Motion is productive and short.** 70 to 240 ms for most changes, entrance easing in, exit easing out, nothing that autoplays or loops; the library honors reduced motion, so custom animation goes through `CdsMotion.AnimateAsync`.

## Screen skeleton

A resource list page; bind the names to your view model. It shows the frame, the hierarchy and the layering, not every property (see the maui-carbon skill and the gallery for those).

```xml
<cds:CdsStack Gap="0">
    <cds:CdsPageHeader>
        <cds:CdsPageHeaderBreadcrumbBar>
            <cds:CdsBreadcrumb Size="Sm">
                <cds:CdsBreadcrumbItem Text="Kubernetes" />
            </cds:CdsBreadcrumb>
        </cds:CdsPageHeaderBreadcrumbBar>
        <cds:CdsPageHeaderContent Title="Clusters">
            <cds:CdsPageHeaderContentText Text="Managed Kubernetes clusters in this account." />
        </cds:CdsPageHeaderContent>
    </cds:CdsPageHeader>
    <cds:CdsGrid>
        <cds:CdsColumn>
            <cds:CdsStack Gap="7" Padding="0,24,0,48">
                <cds:CdsInlineNotification Kind="Warning" IsLowContrast="True"
                                           Title="Quota nearly reached"
                                           Subtitle="9 of 10 clusters used. Delete a cluster or request more quota."
                                           IsVisible="{Binding IsNearQuota}" />
                <cds:CdsDataTable Title="Clusters" ItemsSource="{Binding Clusters}"
                                  IsSearchable="True" IsSortable="True" Selection="Multi"
                                  StickyHeader="True" HeightRequest="560">
                    <cds:CdsDataTableHeader Key="Name" Header="Name" Width="*" />
                    <cds:CdsDataTableHeader Key="Status" Header="Status" Width="160" />
                    <cds:CdsDataTableHeader Key="Region" Header="Region" Width="160" />
                </cds:CdsDataTable>
            </cds:CdsStack>
        </cds:CdsColumn>
    </cds:CdsGrid>
</cds:CdsStack>
```

The table's toolbar holds search and the one primary action ("Create cluster"); selection shows batch actions; each row has an overflow menu; status cells use `CdsIconIndicator` with a word; an empty table explains why and offers the create action (`EmptyState`/`EmptyContent`). Compare the gallery's `PatternResourceListPage.xaml` ([patterns](references/patterns.md) says where to read it).

## Review checklist

Look at the screen in White and G100 at a narrow and a wide width, and with the keyboard only.

- [ ] One primary action; button order follows the pattern (in-page forms: primary first, on the left; dialogs and side panels: primary last, on the right).
- [ ] Every color, gap, size, type style and duration is a token; no hex, no off-scale margins, no custom font sizes, no corner radius on square components.
- [ ] Surfaces step through layers (`CdsLayer`), fields on a layer use that layer's field token, and nothing floats with a decorative shadow.
- [ ] Headings form an outline (one level 1, no skipped levels); body text uses `Body01`/`BodyCompact01`, not headings for emphasis.
- [ ] Copy is sentence case, buttons start with a verb, labels have no colons, errors name the fix, no exclamation marks or emoji.
- [ ] Empty, loading, error, no-results, read-only and disabled states exist and keep the layout from jumping.
- [ ] Status always pairs color with an icon and a word; placeholder and disabled tones never carry essential information.
- [ ] Every field has a label, every icon-only control a name, every group of controls a group name (`CdsSemantics.GroupName`, never `SemanticProperties.Description` on a container with controls inside), and changes are announced.
- [ ] Focus order follows reading order; overlays return focus to their trigger; nothing traps the keyboard.
- [ ] Long text truncates only where the full text stays reachable; labels on buttons wrap rather than truncate.

## References

- [Foundations](references/foundations.md): themes, layers and color tokens, type, spacing and size, grid and breakpoints, shape, focus and states, motion, icons, AI presence, with their Maui.Carbon names.
- [Choosing components](references/choosing-components.md): which Cds control answers which need.
- [Patterns](references/patterns.md): composition rules for recurring screens, with gallery pages.
- [Content](references/content.md): voice, capitalization, labels, buttons, errors, notifications.
- [Accessibility](references/accessibility.md): contrast, keyboard, names, announcements, and the MAUI-specific traps.
- [Data visualization](references/data-visualization.md): choosing a chart, color, anatomy, axes, legends, dashboards.

These distill Carbon v11 guidance and are self-contained. Where a rule here and the library's behavior disagree, the C# implementation is the authority for what a control does; raise the difference rather than working around it with custom styling.
