# Content

Carbon's product voice is plain, specific and brief. The interface is a tool; the words help someone finish a task.

## Voice

- Address the person as "you". Refer to the product by name or not at all; use "we" only when the product owns a failure ("We couldn't save your changes").
- Plain words over jargon; the person's vocabulary over internal names. One term per concept across the product.
- No emoji, exclamation marks or marketing adjectives ("seamless", "powerful") in product UI.
- Present tense, active voice: "The cluster is deleted", not "The cluster has been successfully deleted".

## Capitalization and format

- Sentence case everywhere: titles, headings, buttons, labels, tabs, menu items, tags, table headers ("Create cluster", "Last modified"). Proper nouns keep their capitals.
- Digits for numbers ("3 nodes"), with the unit. Format dates, numbers and currency for the person's culture.
- No terminal period on labels, headings, buttons or single-sentence helper text fragments; use periods in full-sentence messages and descriptions.
- No colon after a label.

## Buttons and links

- Start with a verb and name the outcome: "Create cluster", "Download report", "Delete bucket". One to three words where possible.
- Never "Click here", "OK" or a bare "Submit"; name what happens. "Cancel" dismisses without acting; "Close" closes a surface that has no pending action.
- Destructive buttons repeat the verb and object of the confirmation ("Delete bucket", not "Yes").
- Link text says where it goes ("View billing details"), not "Learn more" alone, and is never a URL.

## Labels, helper and placeholder text

- Labels: one to three words, a noun phrase ("Cluster name"). Mark the minority of required or optional fields with "(required)" or "(optional)".
- Helper text explains a format, constraint or consequence ("Use 3 to 63 lowercase letters, numbers or hyphens"). Do not repeat the label.
- Placeholders show an example ("e.g. prod-eu-01"). They disappear on input and are low contrast, so never the only label and never essential.

## Errors and warnings

- Say what is wrong and how to fix it: "Enter a number from 1 to 50", "Choose a file smaller than 50 MB". Do not blame ("You entered an invalid value") or use codes alone.
- Errors appear when the field loses focus or on submit, not while typing. Warnings (`WarnText`) describe a valid but risky choice ("Clusters with fewer than 3 nodes are not highly available").
- A form-level summary says how many fields need attention ("Fix 2 errors before you continue").

## Notifications

- Two parts: a title that states the outcome ("Upload failed", "Cluster created") and a subtitle with the reason and the next step ("The file is larger than 50 MB. Choose a smaller file.").
- Action labels in notifications are verbs ("Retry", "View cluster", "Undo").

## Empty states

- A short heading that says what is (not) here ("No clusters yet", "No results for \"prod\""), one sentence on why or what to do, one action.

## Truncation and length

- Write for the narrowest layout; expect translated text to be about 30% longer.
- Truncate with an ellipsis only where the full text stays reachable (tooltip, details page); never truncate a button label, let it wrap.
