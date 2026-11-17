# FileUploader

Lets users pick files from their device and shows each file with its upload status, through a button (`variant="button"`) or a dashed drag-and-drop container (`variant="drop"`).

Hand-written from `packages/styles/scss/components/file-uploader` (Carbon v11.99.0). It also exports its parts, `FileUploaderButton`, `FileUploaderDropContainer` and `FileUploaderItem`, plus `FileUploaderSkeleton`. Left out: the legacy class API (`filenameStatus` per call, `onClick` reset, `clearFiles` ref), tooltips on truncated names (the full name is in the `title`), the loading "success" animation, and the network upload itself (you drive each file's `status`).

**When to use**
- Attaching or importing files: logs to a support case, a configuration or CSV import, a certificate. The drop variant suits several files or a page focused on uploading.

**When not to use**
- Pasting short text (a key, a token): use a TextArea or PasswordInput.
- Many files inside a Modal: the list grows tall; prefer a full page or side panel.

**You provide:**
- `FileUploader`: `labelTitle` (heading, default copy "Upload files"), `labelDescription` (size and type limits), `buttonLabel` (default "Add file"; Carbon suggests "Add files") or `dropLabel` for the drop variant, `accept` (array like `['.csv', 'image/*']`; other files are ignored), `multiple` (drop allows several unless `multiple={false}`), `buttonKind` (any Button kind, default `primary`), `size` `sm` | `md` | `lg` (button height; rows use sm / md, default row 48px), `disabled`, `name`, `iconDescription`, `className`.
- Files: `files` (controlled) or `defaultFiles` as `{ uuid, name, status, invalid, errorSubject, errorBody }`. `status` is `uploading` (spinner), `edit` (remove button) or `complete` (check). Added files get `filenameStatus` (default `edit`). `onChange({ files, addedFiles })` and `onDelete(event, { uuid, file })` report changes.
- `FileUploaderButton`: `labelText` (shows the chosen file name, or "n files", after a pick unless `disableLabelChanges`), `buttonKind`, `size`, `accept`, `multiple`, `disabled`, `name`, `id`, `onAddFiles(event, { addedFiles })`, `onChange(event)`.
- `FileUploaderDropContainer`: `labelText` (default "Drag and drop files here or click to upload"), `accept`, `multiple`, `disabled`, `name`, `id`, `onAddFiles(event, { addedFiles })`.
- `FileUploaderItem`: `name`, `uuid`, `status`, `invalid`, `errorSubject`, `errorBody`, `size` `sm` | `md` (default lg 48px), `iconDescription`, `onDelete(event, { uuid })`.
- `FileUploaderSkeleton`: `className`.

**Example**
```jsx
const { FileUploaderDropContainer, FileUploaderItem } = window.Carbon;
<FileUploaderDropContainer multiple accept={['.log', '.txt', '.png']}
  onAddFiles={(e, { addedFiles }) => upload(addedFiles)} />
{files.map((f) => (
  <FileUploaderItem key={f.uuid} {...f} size="md" onDelete={() => remove(f.uuid)} />
))}
```

**Rules**
- File rows sit on `layer-01`, max 320px wide, with the name left (ellipsis when long) and the state icon right: spinner in `icon-primary`, remove button in `icon-primary` (24px target), complete check in `interactive`. Rows and the button share the height of the other form fields.
- An invalid row has a 2px `support-error` outline, the error icon beside the x, and a `border-subtle-01` divider above an error title in `text-error` and supporting text in `text-primary` (one line with only `errorSubject`, two with `errorBody`).
- The drop container is 96px tall with a 1px dashed `border-strong-01` border and `link-primary` text; dragging over it draws a 2px `focus` outline. Disabled uses `text-disabled`.
- Copy: state size and type limits in the description; button text is a short verb phrase ("Add files"); error titles say what went wrong and the body says how to fix it. Reject a bad file with an invalid row rather than dropping it silently.

**Accessibility**
- The button and the drop container are real `<button>`s that open the hidden `<input type="file">` on click, Enter or Space; the input has a visually hidden label. Drag and drop is never the only way in.
- Each remove button is named "Remove file - name" (from `iconDescription`) and reachable with Tab; Enter or Space removes it. The spinner is `role="img"` with a title.
- An invalid row's message has `role="alert"` and is linked to its remove button with `aria-describedby`; always give `errorSubject` with `invalid`.
