# PatternLoading

Which loading feedback to use and where: skeletons for the first page load, inline loading in place of the button that started an action, a progress bar for measurable work, and a blocking overlay for an action that must finish.

Composed from this system's components following Carbon's loading pattern.

**When to use**
- Skeleton (`SkeletonText`, `SkeletonPlaceholder`, `DataTableSkeleton`): the first few seconds of a page or container load, for containers and data (tiles, tables, lists). Not for buttons, inputs, toasts, menus, dropdown items or modals themselves.
- `InlineLoading`: one component or action is processing (creating a cluster, sending an invite).
- `ProgressBar`: the work takes longer than a moment and its size is known (uploads, imports); use a ProgressIndicator for multi-step flows.
- `Loading` with overlay: the whole page is busy and must not be used until done (restoring a snapshot). Add a notification if it may take minutes.

**Components used**
`SkeletonText`, `SkeletonPlaceholder`, `DataTableSkeleton`, `Layer`, `ProgressBar`, `DataTable`, `Button`, `InlineLoading`, `Loading`.

**Composition rules**
- Skeletons copy the real layout: same heading, stat and table sizes, so content replaces them in place without a jump. Remove them as soon as data arrives; don't stack a spinner on top.
- InlineLoading takes the primary button's slot (same width, 16px inset like the button label) and moves through `active` ("Creating cluster...") to `finished` ("Cluster created") or `error` ("Creation failed"). Disable Cancel while it is active if the action can't be stopped.
- ProgressBar label names the item; helper text gives the amount done and time left, then the outcome. Failed uploads say how to retry.
- Overlay `Loading` covers only the region that is blocked, with a description ("Restoring snapshot").
- Status text is sentence case and ends with an ellipsis while running.

**Accessibility**
- Mark a loading region `aria-busy="true"` and remove it when done.
- InlineLoading and ProgressBar announce status changes through live regions; keep the description text meaningful because it is what screen readers read.
- Move focus only when the page changes; a finished inline action keeps focus near the button.
