# Callout

A persistent, non-dismissible message placed in page content to highlight information the user should know before acting (formerly StaticNotification).

Hand-written from `packages/styles/scss/components/notification/_actionable-notification.scss` and the `Callout` in `packages/react/src/components/Notification/Notification.tsx` (Carbon v11.99.0). Reuses the ActionableNotification styles. Difference: `lowContrast` defaults to true here (Carbon's default is false); pass `lowContrast={false}` for the inverse style. The warning icon's "!" is a cut-out, not a black fill.

**When to use**
- Information that must not be overlooked before the user acts, shown with the content and placed next to the input, button or data it is about.

**When not to use**
- Feedback on something that happened (success, failure): use `InlineNotification` or `ToastNotification`.
- Messages the user can dismiss or must respond to: use `InlineNotification` or `ActionableNotification`.
- Ordinary supporting text: use plain body text. Do not stack callouts or put many on one page.

**You provide:** `kind` (info default, warning; info-square, warning-alt, success and error also render, though Carbon recommends info/warning only), `title` (optional, when it helps scanning), `subtitle` (text or nodes, e.g. a sentence ending in an inline `Link`), optional `children` after the subtitle, optional `actionButtonLabel` + `onActionButtonClick` (a small ghost button at the end), `titleId`, `statusIconDescription`, `lowContrast`, `className`; other attributes go on the container.

**Example**
```jsx
const { Callout, Link } = window.Carbon;
<Callout kind="warning" title="3 persistent volumes are deleted too" titleId="del-vols"
  subtitle={<>Snapshot volumes you want to keep first. <Link href="/docs/snapshots" inline aria-describedby="del-vols">How to snapshot volumes</Link></>} />
```

**Rules**
- Low contrast: `notification-background-<kind>` fill, 3px `support-<kind>` left border, 1px border in the same color at 40% opacity, `text-primary` text, 20px status icon in `support-<kind>`. High contrast (`lowContrast={false}`) uses `background-inverse` and the `support-*-inverse` colors.
- Title in `heading-compact-01` sits before the `body-compact-01` subtitle; both wrap. Width is 18rem minimum, up to 38/46/52rem at md/lg/max breakpoints.
- No close button: it stays as long as the content it describes.
- Placement: right next to the related control or data; above an inline notification when both are present.
- Content: short title without end punctuation; one or two sentences. Links only when they add context needed for the current task. Carbon's guidance prefers placing the action button on the page near the action; use `actionButtonLabel` sparingly.

**Accessibility**
- No live-region role: it loads with the page and is read in document order, so place it before the control it warns about.
- Give links and the action button `aria-describedby` pointing at `titleId` (the action button gets it automatically) so they are announced with the context.
- The status icon is named by `statusIconDescription` (default `<kind> icon`); color is not the only cue (icon shape and title).
