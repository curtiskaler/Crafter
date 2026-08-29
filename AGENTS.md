# AGENTS.md - Core C#/.NET Project Context

## Project Overview
<!-- One paragraph: what this project is, what the core user flow is. -->
A .NET (net10.0) C# solution for tracking recipes, ingredients, 
suppliers, purchase prices, calculated costs, etc., so a game 
or business can know what a product costs to make and price it
accordingly.

The project supports 
- video game crafting systems
- restaurant/food-service
- pharmaceutical formulation design

## Critical Build & Test Commands
- Restore Packages: `dotnet restore`
- Build Solution: `dotnet build --no-restore`

## Security & Boundaries
<!-- 
Invariants that must never be broken (secrets, API contracts, auth
boundaries). These are the rules an agent should never "helpfully"
work around.
-->
- Never include secrets like credentials in code 

## Project Guardrails & Anti-Patterns
- NEVER add external dependencies without explicit user approval.
- NEVER add external dependencies without confirming they map safely into a .NET 9 Standard lifecycle.

## C# Code Style & Architecture Guardrails
Do not write legacy framework code. Follow these modern C# patterns strictly:
- Language Features: Utilize C# 12+ idioms including primary constructors, collection expressions (`[]`), and required properties.
- Performance: Prefer `ReadOnlySpan<T>` and `Memory<T>` for heavy parsing tasks instead of allocations. Use `ValueTask` for high-frequency async pathways.
- Dependencies: Enforce explicit Constructor Dependency Injection. Do not inject `IServiceProvider` or use the Service Locator anti-pattern.
- Explicit Types: Avoid `var` when the underlying type is not explicitly visible from the right-hand assignment (e.g., use explicit types for method invocation returns).

## Working Agreement
- For larger/multi-step changes: use Plan Mode first, show the plan
  before implementing.
- After a session where you learned something non-trivial, ask to
  update memory files.
