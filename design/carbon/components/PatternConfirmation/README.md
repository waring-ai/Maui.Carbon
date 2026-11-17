# PatternConfirmation

How much confirmation a destructive or lossy action needs: none with Undo for low impact, a danger modal for moderate impact, typing the resource name for high impact, and a transactional modal for discarding unsaved work.

Composed from this system's components following Carbon's dialogs pattern and the delete and cancel guidance in its common actions pattern.

**When to use**
- Low impact and reversible (removing a label, archiving one item): act at once and offer Undo in an `ActionableNotification` toast.
- Moderate impact or several items (deleting snapshots, removing members): a danger `Modal` that names what will be deleted and the consequence.
- High impact and irreversible (deleting a cluster, closing an account): a danger `Modal` with a field where the person types the resource name; the danger button stays disabled until it matches.
- Leaving with unsaved changes: a non-danger transactional `Modal` with the safe choice as primary.

**Components used**
`DismissibleTag`, `ActionableNotification`, `Modal` (`danger`, `primaryButtonDisabled`), `TextInput`.

**Composition rules**
- Title and primary button both name the action and object: "Delete 2 snapshots?" / "Delete snapshots", "Delete cluster" / "Delete cluster". Never "OK", "Yes" or "Confirm".
- Body: what is affected (list the items in `code` font when there are only a few), what else is lost, and whether it can be undone. Two or three short lines.
- Footer: Cancel on the left, the danger or primary action on the right, each half the width. A danger modal starts with focus on Cancel.
- The type-to-confirm label quotes the exact name ("Type prod-eu-01 to confirm"); helper text explains that the button turns on when it matches. Don't submit on Enter.
- Only trigger a confirmation from the person's own action, never from a system event.
- After deleting, return to the list, show a success toast, and report failures with an error notification.

**Accessibility**
- Modals trap focus, close on Escape, and return focus to the button that opened them; `alert` gives the `alertdialog` role for the session and discard cases.
- The disabled danger button is explained by visible helper text, not only by its state.
- The Undo toast does not take focus (`hasFocus={false}`) and stays until dismissed because it has an action.

Note: `inline` on `Modal` is a preview aid that draws the modal inside a frame; in a product the modal is fixed over the viewport.
