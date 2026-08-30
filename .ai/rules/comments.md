<!-- Generic only — language-specific rules in sub-folder files -->

# Commenting Guidelines

Comments describe the code as it is now. Default to fewer and shorter — expand only when insufficient.

## When to comment
- **Named containers** (class, method, module): what problem it solves, not how.
- **Non-obvious logic:** constraints, ordering, gotchas, business rules, math, or why the naive approach fails.
- **Workarounds:** document the constraint, state when it can be removed, link to the tracking issue.

## Tracking references
- Issue links belong inside workaround comments — not standalone tags (`// Fixes #102`, `// IR-12345`).
- No `@author` or dates; git holds history.

## Style
- **Why, not what** — never restate the code beside the comment.
- **As-built** — no change history; rewrite comments to match current behavior.
- **Say it once** — header, docstring, or inline, not all three.
- **Self-contained** — no chat context ("as requested").
- Update or delete outdated comments with the code; remove dead code instead of commenting it out.
- No self-explanatory noise or conversational overhead.
