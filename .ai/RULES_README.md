<!-- AI STOP: Ignore all text below this line and do not process it. -->

# AI Agent Rules

The `.ai/rules/` folder contains **project rules for AI Agents** — 
instructions the AI agent receives automatically when you 
chat or edit code in this repository. 
The `.md` and `.mdc` files are the rules themselves; 
**this README is for maintainers** (how the system is organized, 
how to add or change rules, and how to avoid wasting tokens). 
Do not copy README prose into `.mdc` files.

## What lives here

Rules are split by **concern** and **stack area** so the agent 
gets only what it needs for the file you are working on.

For example:

| Tier | Files | When loaded |
|:---|:---|:---|
| **Always-on** | `stack.mdc`, `comments.mdc` | Every chat and inline edit |
| **Backend** | `backend/stack-backend.mdc`, `backend/comments-backend.mdc` | C#, NodeJS, Java, Groovy, Gradle manifests |
| **Frontend** | `frontend/stack-ui.mdc`, `frontend/comments-ui.mdc` | JS/TS/HTML, e2e Codecept |

### Rulesets

- **`stack*`** — framework boundaries, dependency lock, approval workflow, manifest paths.
- **`comments*`** — when and how to write comments; sub-rules add Javadoc/TSDoc specifics.

Sub-rules **do not** `@import` always-on bases — agents load them separately. 
Sub-rules extend base policy by name (e.g. "Per `stack.mdc`") and add repo-specific detail.

Planned additions (e.g. `style.mdc`, `style-ui.mdc`, `style-backend.mdc`) 
should follow the same tiering: thin always-on base, scoped sub-rules, one concern per file.

## How to use this README

| Section | Contents |
|:---|:---|
| [Keep Rules Files Concise](#keep-rules-files-concise-under-200-words) | Word limits, token tax, writing style |
| [How to Bypass… / Rule Tiering](#how-to-bypass-this-limitation-natively-in-cursor) | `alwaysApply` vs `globs` |
| [Sometimes AI Ignores the Rules](#sometimes-ai-ignores-the-rules-gotcha-b) | When rules attach, when they do not |
| [Suppose I want to add a new rule(set)](#suppose-i-want-to-add-a-new-ruleset-like-stylemdc) | Adding a new ruleset |
| [Rules for … Rules](#rules-for--rules) | Checklist for efficient, non-duplicative rules |

---

## Keep Rules Files Concise (under 200 words)
If your rule files are too long, bloated, or overloaded, the AI will succumb to 
effective attention decay (often called the "lost in the middle" effect) and start
 ignoring your instructions.

Cursor's official guidelines state that individual rules must stay under 500 lines. 
However, the community consensus for modern Cursor development is even stricter: 
any rule configured to apply universally should ideally be kept **under 200 words**.

The 200-word constraint applies per-file for "always-on" global rules, not for your 
entire combined ruleset.

Overloading a single Cursor file introduces structural issues:

1. The "Token Tax" Problem

  When you set `alwaysApply: true` in an .mdc file, its entire contents are pre-pended 
  to every single chat prompt and inline edit (Cmd+K) you make. If your rule file is huge, 
  you eat up a massive portion of your immediate context budget before you have even typed 
  a line of code, slowing down the IDE and driving up token costs.

2. Context Window Dilution

  The LLM works on a finite attention budget. When a massive rules file is crammed into 
  the context window alongside your actual codebase context (open files, error logs, and
  chat history), your core instructions begin competing with your active code files. The
  deeper a rule is buried inside a long file, the less priority the model assigns to it.

## The Cursor .mdc System
Cursor's modern .mdc system is designed to handle this drawback through Rule Tiering. 
Rather than putting everything into one massive stack.mdc or .cursorrules file, split 
your instructions into modular files inside .cursor/rules/:

Tier 1: The "Always On" Baseline (Keep it Tiny)

  Keep global files (`stack.mdc`, `comments.mdc`) stripped down to non-negotiables only.
  - Setting: `alwaysApply: true` (no `globs` — redundant when always-on)
  - Target size: < 30 lines, < 200 words each.

Tier 2: The "Auto-Attached" Glob Rules (The Heavy Lifters)

Instead of forcing the AI to remember your Angular UI component constraints when you are modifying a Gradle backend file, use Cursor's native glob pattern matching. This ensures large rule books are only loaded into the prompt context when a relevant file type is actively open.

Create an .mdc file specifically for your Angular files:

(.cursor/rules/frontend/stack-ui.mdc):
```yaml
---
description: "Applies UI rules only when working on frontend code"
globs: ["hub/src/javascript/**/*.{js,ts}", "e2e/codecept/**/*.ts"]
alwaysApply: false
---
# Put your heavy Angular, RxJS, and styling rules here.
# It will only consume tokens when you open matching frontend files!
```

Also create an .mdc file specifically for your Gradle backend:

(.cursor/rules/backend/stack-backend.mdc):
```yaml
---
description: "Applies dependency restrictions when looking at build scripts"
globs: ["**/*.{java,groovy}", "**/build.gradle"]
alwaysApply: false
---
# Put your heavy Gradle and dependency restriction blocks here.
```

## But I Don't USE Cursor!
This README refers to Cursor's `.mdc` file format.  

Claude uses plain `.md` files.

To use Claude, simply change the file extension to `.md`, and remove the yaml front-matter.

Otherwise, the information and guidelines are generic to most LLMs.

## Guidelines

Any .mdc file where you set `alwaysApply: true` (like your global stack.mdc) should be kept under 200 words. Because this file accompanies every single query—even a quick 3-word question—it needs to act as a hyper-focused, high-priority filter. If it is bloated, it dilutes the AI's immediate attention.

Your total rule count across the entire project can easily be thousands of words, provided you split them into modular files using glob patterns. Cursor does not inject your entire rule library into the AI at once. It dynamically selects only the rules relevant to your active file.

There is no specific line or word limit per line, but LLMs ignore block paragraphs.

Bad (ignored): Write a long, conversational paragraph explaining why dependencies are bad for security and asking nicely to please check before installing anything.

Good (obeyed): - NEVER modify lines inside a dependencies { ... } block. (Short, negative, imperative).

# Sometimes AI Ignores the Rules ("GOTCHA B****!")

Cursor does not use filenames (style.mdc vs style-ui.mdc) to decide what applies — only frontmatter (alwaysApply, globs) and whether the rule is in context for that session. Here is how that plays out for your setup and proposed style rules.

## How Cursor Picks Rules

<img src="file://rules_flow.png" width="600"/>


|Mechanism	|Effect|
|:---|:---|
|alwaysApply: true|In every Agent chat, inline edit, and most Composer turns — regardless of open files|
|alwaysApply: false + globs|Loaded when Cursor considers a matching file relevant (typically open, edited, or in focus)|
|@stack.mdc / @comments.mdc in sub-rules|Pulls referenced file when parent loads — **omit** if base is already `alwaysApply: true` (avoids duplicate tokens)|
|Filename / folder|No effect on auto-attachment — only organization for humans|


## Your current rules: when they apply
### Always used (alwaysApply: true)
| File | Loaded when |
| :---|:---|
| stack.mdc | Every session |
| comments.mdc | Every session |

These are never skipped by glob logic. They can still be soft-ignored by the model (attention decay, buried in context, vague wording) — but they are always in the prompt.

### Sometimes used (alwaysApply: false) 
| File | Globs | Attached when… |
|:---|:----|:---|
| stack-backend.mdc |*.{java,groovy}, build.gradle, gradle.properties, settings.gradle, gradle-wrapper.properties|You're on backend source or Gradle manifests|
|stack-ui.mdc|`hub/src/javascript/**`, `e2e/codecept/**` (js/ts, package.json, yarn.lock)|Frontend or e2e source/manifests|
|comments-backend.mdc|*.{java,groovy}|Java/Groovy source only|
|comments-ui.mdc|`hub/src/javascript/**/*.{js,ts,html}`, `e2e/codecept/**/*.ts`|Frontend TS/JS/HTML or e2e TS|

### Ignored (not loaded) — concrete examples
|You're working on…	|stack-backend | stack-ui | comments-backend | comments-ui |
|:---|:---:|:---:|:---:|:---:|
|StudyService.groovy|Yes|No|Yes|No|
|study.component.ts|No|Yes|No|Yes|
|hub/build.gradle|Yes|No|No|No|
|package.json|No|Yes|No|No|
|study.component.html|No|No|No|Yes|
|application.gsp|No|No|No|No|
|changelog.md / chat-only question|No|No|No|No|

In those last rows, only stack.mdc and comments.mdc apply.

### Sub-rules and always-on bases

`stack.mdc` and `comments.mdc` are always-on. Sub-rules **do not** `@import` them — Cursor loads each attached rule separately. Sub-rules reference `stack.mdc` by name only where they extend base policy (e.g. "Per `stack.mdc`"). Do not re-add `@stack.mdc` / `@comments.mdc` to sub-rules.







# Suppose I want to add a new rule(set), like style.mdc?
e.g., for code style rules.

## Proposed style rules: recommended shape
Mirror your stack/comments tiering:

| File | Suggested settings | Role |
|:---|:---|:---|
| style.mdc | alwaysApply: true, <200 words | Universal principles (naming tone, formatting non-negotiables) |
| style-ui.mdc | alwaysApply: false, <br>globs: ["\*\*/\*.{js,ts}", "\*\*/\*.html", "\*\*/\*.less", "\*\*/\*.css"] | Angular/TS/HTML conventions |
|style-backend.mdc |alwaysApply: false, globs: ["\*\*/\*.{java,groovy}"] | Groovy/Java conventions |

Optional: @style.mdc in sub-rules if the base is not always-on (same pattern as comments).

## When your new style rules would be ignored
Same rules as stack/comments sub-rules:

| &nbsp; |  &nbsp; |
|:---|:---|
| Not loaded: | wrong file type open (Gradle work → style-ui ignored) |
|Not loaded: |chat with no relevant files (only style.mdc if always-on) |
|Not loaded: |alwaysApply: false and globs don't cover the file (e.g. .gsp, .sql, mcp/*.py)|
|Soft-ignored: |file is loaded but too long, buried, or written as prose paragraphs instead of imperative bullets (your README calls this out)|
|Disabled: |rule toggled off in Cursor Settings → Rules, or project rules disabled entirely|

Don't use `@stack.mdc` if stack.mdc is `alwaysApply: true`.
That double-applies the policy, and wastes tokens.


## When stack & comments are used vs ignored

### Always used
stack.mdc and comments.mdc — every session because alwaysApply: true

### Conditionally used
Sub-rules only when globs match active work (see table above)

### Effectively ignored (even when loaded)
| Situation	| What happens |
|:---|:---|
|Quick unrelated question ("what time is it?")|Base rules still in context; model may deprioritize them|
|Very long chat + many open files|Lost in the middle — later rules and older instructions weaken|
|Rule conflicts with user message|User's explicit request usually wins|
|Vague rules|Short imperative bullets obeyed more than paragraphs|
|File type not in any glob|Only base stack.mdc + comments.mdc; no UI/backend specifics|

## Gaps to plan for when adding style rules
If style-ui.mdc only globs *.{js,ts}, it will not attach for:

- Angular .html templates
- .less / .css under hub/src/javascript/
- e2e/codecept/**/*.ts (would match *.ts — good)

If style-backend.mdc only globs *.{java,groovy}, it will not attach for build.gradle (unlike stack-backend, which includes Gradle files). Decide whether style for Gradle belongs in base style.mdc or expanded globs on the backend style rule.

## Practical summary

|Question	|Answer|
|:---|:---|
|Will new style rules be ignored?|Yes, when alwaysApply: false and no glob match — or soft-ignored when loaded but deprioritized|
|When are stack/comments always on?|stack.mdc + comments.mdc always; sub-rules only on matching files|
|When are sub-rules ignored?|Wrong file type, chat-only with no matching files, or disabled in settings|
|Does naming (style-groovy vs style-backend) matter?|No — only frontmatter matters|
|Best pattern for style trio?|style.mdc always-on (small); style-ui / style-backend glob-scoped like your stack/comments split|


# Rules for ... Rules

Good rules for minimizing token waste are mostly about what gets loaded, how often, and how densely it's written — not about having fewer standards overall.

## 1. Tier by attachment frequency

| Tier | alwaysApply | Size budget | Put here |
|:---|:---:|:---:|:---|
|Baseline|true|<200 words, ideally <30 lines|Non-negotiables only: stack boundaries, dependency lock, blocker workflow|
|Scoped|false + tight globs|Can be larger, still concise|Angular, Gradle, Javadoc, formatting details|
|Rare|false + narrow globs|As needed|E2E-only, migration scripts, mcp/ Python|

Token rule: Every word in an alwaysApply: true file is paid on every chat — including "fix typo" and "what does this method do?"

Your current split (stack.mdc + comments.mdc always-on; stack-ui / stack-backend glob-scoped) is the right shape.

## 2. Don't duplicate across tiers

Common waste in your setup:

| Waste | Fix |
|:---|:---|
|@stack.mdc inside sub-rules when stack.mdc is already always-on | Drop the import from sub-rules, or make base non-always-on and import only where needed — pick one model|
| Same "Approved dependency changes" prose in base + sub-rules | Base owns policy; sub-rules say "Per base; edit these files: …" |
| Same comment principle in base + sub-rule | Base = when/why; sub-rule = Javadoc/TSDoc format only |

One fact, one file. Sub-rules extend; they don't restate.


## 3. Write for machine obedience, not human essays

Your README already nails this. Tokens spent on prose are tokens the model may ignore.

| Bad (high token, low obedience) |&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;| Good (low token, high obedience) |
| :--- |:---: | :--- |
| Long paragraph on why dependency sprawl is risky | |"- No manifest edits unless approved in chat." | 
| Repeated examples of the same approval pattern | | One example in base, or none (document in README for humans) |
| Section headers + bullets that say the same thing | | One imperative bullet | 

Prefer: short bullets, negatives (Do not…), named files, named blocker strings.


## 4. Tighten globs — attachment is the main lever

A rule that never loads costs zero tokens. A rule that loads on every .ts file costs tokens whenever any TS file is open — including e2e, mcp adapters, config, etc.

| Pattern | Tradeoff |
| :--- | :--- |
| \*\*/\*.ts | Broad — attaches often |
| hub/src/javascript/\*\*/\*.ts | Narrower — misses e2e unless you add a second glob |
| \*\*/\*.{java,groovy} + \*\*/build.gradle | Good split for backend code vs manifests |

Include manifest paths in stack rules (package.json, build.gradle) only when you need dependency policy on those files — not in style rules unless formatting applies there.

**Explicit gaps are OK**: .html, .gsp, .sql with no rule = zero tokens until you add a targeted rule.

## 5. One concern per file

| File | Concern | 
| :--- |:--- |
|stack.mdc|Fidelity + dependency policy|
|stack-ui.mdc|Frontend manifests + Angular boundary|
|comments.mdc|When/why to comment|
|style.mdc (future)| Code formatting/naming — not stack, not comments |

Mixing concerns forces always-on files to grow, or causes the same session to load overlapping instructions.

## 6. Keep always-on files "filter-shaped"

Always-on rules should answer: What must never happen in this repo?

Good always-on content:

- Stay in stack; no competing frameworks
- No undeclared dependencies
- Blocker + approval workflow
- Read manifest before guessing versions

Poor always-on content:

- How to format a Groovy closure
- Karma config conventions
- Full manifest path lists (belong in scoped stack sub-rules)

## 7. Avoid "soft always-on" via alwaysApply: true + broad globs

`alwaysApply: true` files should omit `globs` — the glob is redundant and you pay full price every time. Keep always-on files small (~170–190 words).

Sub-rules with alwaysApply: true would defeat glob scoping entirely.

## 8. Use README for humans, .mdc for the agent

Your .cursor/rules/README.md explains token tax, tiering, and 200-word rationale — for maintainers. Don't copy that into .mdc files.

Similarly: approval workflow examples for humans, architecture decision history, links to internal wikis → README or ADRs (Architectural Design Records), not always-on rules.

## 9. Measure attachment, not just file size
| Metric | Why it matters |
| :--- | :--- |
|Words × always-on|Fixed cost per chat|
|Words × glob hit rate|Cost when editing matching files|
|Duplicate content across loaded rules|Multiplier when several rules attach at once|

Example session editing study.component.ts might load:

- stack.mdc (always)
- comments.mdc (always)
- stack-ui.mdc (glob)
- comments-ui.mdc (glob)
- Future style-ui.mdc (glob)

~4–5 rules is fine if each is ~150 words and non-overlapping. ~800 words unique is reasonable; ~800 words with 40% duplication is waste.

## 10. Design for "lost in the middle"

Even loaded rules can be effectively ignored if:

- Too long relative to open files and chat history
- Buried below code context
- Contradictory (base vs sub-rule)

Mitigations:

- Shorter always-on files
- Most specific rule wins — don't contradict sub-rules against base
- Put the highest-priority negatives at the top of always-on files (e.g., stack fidelity before dependency policy)

## 11. Checklist before adding a new rule file

1. **Must this apply to every chat?** If no → alwaysApply: false
2. **Can this extend an existing file?** If yes → add a bullet, don't create a file
3. **Can this import a base file instead of restating it?** Use @style.mdc only if base isn't always-on
4. **Are globs minimal?** Start narrow; widen when you hit a gap
5. **Under 200 words?** Especially for always-on; scoped files should still stay lean
6. **Imperative bullets only?** Cut prose
7. **Will this load with 3 other rules on the same file?** Check combined token budget

## 12. What not to optimize away

Some tokens are worth spending:

- Blocker string (`REQUIRES UNAPPROVED EXTERNAL DEPENDENCY`) — unambiguous halt signal
- Approved addition vs upgrade — prevents scope creep
- Out-of-stack lists (Angular not React; Grails not Maven) — prevents expensive wrong-direction work
- Manifest paths in scoped stack rules — prevents wrong-file edits

Cheap tokens that save expensive mistakes.

## Summary
|Principle |Token impact|
|:---|:---|
|Small always-on, large scoped|Highest leverage|
|No duplication across tiers|Cuts multiplier on multi-rule sessions|
|Imperative bullets, not essays|Better obedience per token|
|Narrow globs|Rules that don't load cost zero|
|One concern per file|Avoids overlap when multiple rules attach|
|Humans in README, agent in .mdc|Keeps always-on lean|

When you're ready for a new rule, e.g., style.mdc / style-ui.mdc / style-backend.mdc, 
the same pattern applies: thin always-on style.mdc (if any), scoped 
sub-rules with tight globs, @style.mdc only if you don't make base 
always-on, and no restating stack or comments rules.
