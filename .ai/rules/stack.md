<!-- Generic only — stack details in sub-folder rules -->

# Stack & Dependency Policy

## Fidelity
- Stay in this project's stack — no competing frameworks or architectures.
- Translate external examples into this project's language and conventions before outputting code.
- Read the manifest for versions; do not guess.

## Allowed
- Import and use manifest-declared packages; run build, test, and lint with existing deps.

## Forbidden
- No manifest edits, install/add, or code assuming a future package.
- No import or implement undeclared packages; proposing while halted is OK — do not add or code against them until approved.

## When blocked
- Output `⚠️ BLOCKER: REQUIRES UNAPPROVED EXTERNAL DEPENDENCY`, name the package and why, and halt.

## Approved dependency changes
User must explicitly approve manifest changes in chat before editing manifests, running install/add, or importing new packages.

**Additions** — net-new package; approval names package and purpose (version if known).

**Upgrades** — bump existing package or toolchain; approval names target and version if known.

Within approved scope only. Each approval covers only what was named unless the user says otherwise.
