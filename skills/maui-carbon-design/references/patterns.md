# Patterns

Carbon defines patterns as best-practice answers to a user goal: reusable combinations of components, flows and sequences. A component is a finished control; a pattern is a way to put controls together so a person can get something done, and it can be built more than one way. This file shows how to pick a pattern. The three pattern files hold Carbon's rules for each one, mapped to Cds controls.

## How to use a pattern

1. **Start from the person's goal, not the widget.** "Remove a member from a project" is a goal; "a delete button" is not. Find the goal in the table below.
2. **Read the whole section,** including the "when not to use" lines: most mistakes come from picking the wrong variant, such as a modal for a long form or a toast for a form error.
3. **Copy the structure from the gallery page,** then bind it to your data. Change the content; keep the composition.
4. **Check the states the pattern names** (empty, loading, error, read-only, disabled) and its keyboard and screen-reader rules.

Each section names the pages that implement it in the library's public repository. Read them at the tag of the installed package version:

- Gallery: `https://raw.githubusercontent.com/waring-ai/Maui.Carbon/v<version>/samples/Maui.Carbon.Gallery/Pages/Patterns/<Name>.xaml`
- Relay (the same patterns inside a full MVVM app): `…/samples/Maui.Carbon.Relay/Views/<Name>.xaml`

## Find the pattern for the goal

| The person wants to… | Pattern | Read |
|---|---|---|
| Get around the app, know where they are, switch account or product | Page anatomy and global header | below |
| Add, edit, copy, remove, delete, refresh, reset, cancel or close something | Common actions | below |
| Enter or change data (create, edit, configure, sign up) | Forms | [forms](patterns-forms.md#forms) |
| Fill in a short, focused form at an expressive moment | Fluid forms | [forms](patterns-forms.md#fluid-styles) |
| Sign in | Login | [forms](patterns-forms.md#login) |
| Change preferences | Settings | [forms](patterns-forms.md#settings) |
| Write and format text | Text toolbar | [forms](patterns-forms.md#text-toolbar) |
| Scan, sort and act on many records | Resource lists | [data](patterns-data.md#resource-lists) |
| See everything about one resource | Details pages | [data](patterns-data.md#details-pages) |
| Find something by text | Search | [data](patterns-data.md#search) |
| Narrow a set by its attributes | Filtering | [data](patterns-data.md#filtering) |
| Read text or lists that do not fit | Overflow content | [data](patterns-data.md#overflow-content) |
| Watch numbers and trends | Dashboards | [data](patterns-data.md#dashboards) |
| Answer a question, confirm a choice or do a short task without leaving the page | Dialogs | [feedback](patterns-feedback.md#dialogs) |
| Open extra controls or information from a trigger (profile menu, table settings) | Disclosures | [feedback](patterns-feedback.md#disclosures) |
| Learn what happened, or about something the system did | Notifications | [feedback](patterns-feedback.md#notifications) |
| Judge the health or priority of items at a glance | Status indicators | [feedback](patterns-feedback.md#status-indicators) |
| Wait for content or an action | Loading | [feedback](patterns-feedback.md#loading) |
| Understand an area that has nothing in it yet, no results or an error | Empty states | [feedback](patterns-feedback.md#empty-states) |
| See something they cannot change or use right now | Disabled, read-only and hidden | [feedback](patterns-feedback.md#disabled-read-only-and-hidden) |

Most screens combine several patterns. A resource list page, for example, uses page anatomy, resource lists, search, filtering, common actions, empty states, loading and dialogs for deletion.

## Page anatomy and global header

Gallery: `PatternResourceListPage` (page frame). Relay: `Pages/MainPage.xaml` (the shell) and the header panels `Views/AccountPanel`, `Views/NotificationsPanel`.

**Page**

- `CdsUIShell` provides the header and side nav for global navigation. The side nav marks the current page.
- A page header (`CdsPageHeader`) has a breadcrumb when the page sits below the top level, then the title (the page's only level-1 heading), an optional one-line description, page actions at the end (the primary one last in that row), and tabs when the page has sections.
- Content goes on the grid, with sections 48 apart. Page-level messages (`CdsInlineNotification`) go at the top of the content, under the header.
- Keep the layout stable across states: skeletons, empty states and errors take the content's place. They are not a different page.
- Drilling down opens the selected object on its own page, with a breadcrumb back up above the title.

**Choosing the shell**

- **Header only** (`CdsHeader` with `CdsHeaderNavigation` links and `CdsHeaderMenu` sub-menus) suits a few main sections with no secondary navigation. Its menus overlay the content, so do not use it when a sub-menu has to stay open.
- **Header with side nav** (`CdsSideNav`) holds more items stacked vertically and one more level of hierarchy, and its menus stay open without covering content. Use it for most products.
- Each side-nav area groups the pages of one end-to-end workflow, so a person can finish the task without leaving the area.
- Organize navigation by the person's tasks and the domain, never by the org chart, acquisitions or technical structure. "Recent" works as a secondary scheme, not the main one. Alphabetical order works only when people know the exact label.
- Never put unbounded, user-generated content (every project, every document) in the side nav. List it on a page and drill down instead.

**Header anatomy and rules**

- Menu button (`CdsHeaderMenuButton`): opens the side nav on narrow widths.
- Header name (`CdsHeaderName`): the product name, which links only to the product's home.
- Header links (`CdsHeaderNavigation`): product sections. They never open a new window or go to another domain, and they move into the side nav (`CdsHeaderSideNavItems`) at narrow widths.
- Header menus (`CdsHeaderMenu`): the label only opens the menu and never navigates itself. The menu closes on selection, on an outside click, or on the label again.
- Utilities (`CdsHeaderGlobalAction`, right-aligned in `CdsHeaderGlobalBar`): system-level tools such as notifications, help and profile. A utility opens a `CdsHeaderPanel` and never navigates directly.
- Switcher (`CdsSwitcher`): moves between products. List recently used, frequently used, or all of the account's apps if the list is short.
- When the header holds both, product links go on the left and system links on the right. On narrow widths, product links move to the side nav and system links to the right panel.
- The header is where people orient themselves: show which account, environment or mode is active there.

**State and accessibility**

- Keep or restore each area's state (filters, scroll, a half-filled form) when a person switches areas, or warn them before it is lost. The shell does not do this for you: keep that state in the view model or the route.
- `CdsSkipToContent` is the header's first focusable element, so keyboard users can skip past the navigation.
- Name each navigation region with `CdsSemantics.GroupName` and `GroupKind="Navigation"`, with a distinct name when there are several. Visual order matches focus order, and a heading's style matches its level.

## Common actions

Carbon uses each common action one way everywhere, so people recognize it. Use the word, the control and the friction given here.

| Action | Meaning | Control | Rules |
|---|---|---|---|
| Add | Put an existing object into a list, set or system | `CdsButton` with `Icon="Add"`, or the toolbar's primary "Add …" | Emphasis follows importance: one primary, the rest secondary. Before adding, say what it costs or changes: money, access, legal effect, how long it takes, single or bulk. |
| Create | Make a new object | Primary `CdsButton` "Create …" (in a table toolbar, at its end) | One per view. |
| Edit | Change values | An `Edit` icon button, a menu item, or a button | Commonly switches the target into an editable state. |
| Copy | Duplicate a value or object | `CdsCopyButton`, `CdsCodeSnippet` | Always confirm with the "Copied" feedback the control shows. |
| Cancel | Stop the current task and close it | Secondary `CdsButton` (ghost in toolbars) or a link | Never destructive itself. Warn first if stopping loses data. In dialogs, Cancel undoes changes applied in the dialog. |
| Close | Dismiss a page, panel, menu or message | The close (x) icon, top right | Never a text "Close" inside a button group. |
| Clear | Empty a field or selection | The x inside the field or tag (`CdsSearch`, `CdsDismissibleTag`, "Clear filters") | Controls with a default (radio groups) return to the default. |
| Reset | Go back to the last applied or saved values | `CdsLink` or ghost button "Reset" | Not the same as Clear. |
| Refresh | Reload data that is out of sync | A `Renew` icon button or a button | Keep the content on screen while it reloads. |
| Next / Back | Move through a sequence | "Next" primary, "Back" secondary, with `CdsProgressIndicator` | Rename Next on the last step to the final action ("Create cluster"). |
| Remove | Take an object out of a list without destroying it | Secondary or ghost button, `Subtract` icon, menu item | Rarely the primary action. Say clearly that it is not Delete. |
| Delete | Destroy an object | `Kind="Danger"` button, `TrashCan` icon, or a danger menu item | Friction follows impact, below. |

**Delete friction**

- **Low impact** (trivial to undo or recreate): delete on click and offer Undo in a toast.
- **Moderate** (cannot be undone, or deletes more than one item): a danger `CdsModal` (`IsDanger`) names the object and the result, for example "Delete bucket logs-eu?" with the button "Delete bucket".
- **High** (costly to recreate, a lot of data, or side effects on other items): the danger modal also makes the person type the resource name before the button enables.
- **After deleting,** go back to the list it came from, remove the item with a short exit animation, and confirm with a success toast.
- **If deleting fails,** say so in a notification, restore the item, and if it is possible send a second notice through another channel.

**Errors from any action** say what happened, why if it is known, and what to do next, such as try again, go back, or contact support. Field errors take at most two lines; full-page and modal errors take at most three lines of body text. Be honest and calm, and never show only an error code.
