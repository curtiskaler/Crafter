<!-- Generic only — language-specific rules in sub-folder files -->

# Testing Rules

## Frameworks
- NUnit (built-in assertions) for tests; Moq for mocking.

## Constraints
- Test behavior via public interfaces — never internal/private members.
- Tests must be 100% deterministic: no race conditions, unseeded randomness, unhandled async timing.
- No explanatory comments, loops, or if/else inside test bodies — assertions only.

## Naming
- Test class: `[TargetClass]Tests`.
- Test method: `[Method]_Should_[Outcome]_When_[Condition]` or `[Method]_Given_[Context]_When_[Action]_Then_[Result]` — no generic prefixes.
  - e.g. `SubmitPayment_Should_RejectPayment_When_BalanceIsInsufficient`

## Rules
- Every code change ships with a matching test file.
- Never delete a test without explicit user permission.
- Mock via constructor injection — never mock concrete classes unless necessary.
- Cover the happy path plus at least two edge/failure cases.
- Bug fixes: write the failing regression test first, then the fix.
