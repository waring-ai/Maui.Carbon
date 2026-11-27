# Choosing a local file with CdsFileUploader

For a form field that picks a file to read where it is (an import, a certificate) rather than upload it, use `CdsFileUploader` bound to the view model; never a text field plus a raw MAUI `FilePicker` button.

```xml
<cds:CdsFileUploader LabelTitle="Domain list"
                     LabelDescription="A .zip export. It is read in place, never unpacked or copied."
                     ButtonLabel="Choose file" ChosenButtonLabel="Change file" ButtonKind="Tertiary" Size="Md"
                     Accept=".zip" AccessMode="Path"
                     FilePath="{Binding ArchivePath}"
                     IsInvalid="{Binding IsArchiveInvalid}"
                     ErrorSubject="{Binding ArchiveError}"
                     ErrorBody="{Binding ArchiveErrorHelp}" />
```

- `FilePath` (`string?`) is two-way: a pick or drop sets it, removing the row sets `null`, and the view model sets it to pre-fill a default (`null` or `""` shows no file). One file (`Multiple` false, the button default): a new pick replaces the row.
- Validate in the view model when the path changes and set `ErrorSubject` (and `ErrorBody`) before `IsInvalid`, so the alert is spoken once. Once used, the three describe whichever file is chosen. `FileStatus` (`CdsFileStatus?`) sets the row's spinner or check; `null` leaves it.
- `AccessMode="Path"`: on iOS and Mac Catalyst the uploader opens the document picker in open-in-place mode and holds the file's security-scoped access until the file leaves `Files`, so `File.OpenRead(path)` or `ZipFile.OpenRead(path)` reads the original (no copy, any size). Open it while it is chosen; access ends with the process (persist it with your own bookmark). The default `Stream` mode suits uploads: read through `CdsUploadedFile.OpenReadAsync()`, because a sandboxed Apple app may not open the bare path. Android's picker may hand over a cached copy.
- Several files: set `Multiple="True"` and bind `FilePaths` (`IReadOnlyList<string>?`, two-way; the control publishes a new array on every change and never edits yours; assign a new list to change the files; `null` changes nothing). Bind `FilePath` or `FilePaths`, not both. Per-file errors go on each `CdsUploadedFile` in `Files`.
- `FilesAddedCommand` gets the added paths (`IReadOnlyList<string>`) and `FileRemovedCommand` the removed path (`string?`), so a view model without MAUI references can react. `CdsUploadedFile` has `Path`, `FromPath(path)` and `OpenReadAsync()`.
- `Accept` takes extensions and MIME types (`.zip, .tar.gz, image/*, application/x-pem-file`); on Apple platforms each maps to a uniform type at runtime, so any extension filters the picker.
