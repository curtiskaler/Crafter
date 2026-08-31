# Crafter Wiki

A .NET (`net10.0`) solution for tracking recipes, ingredients, suppliers,
purchase prices, and calculated costs — so a game or a business can know what a
product costs to make and price it accordingly. It targets video-game crafting
systems, restaurant / food-service, and pharmaceutical formulation design.

## Features

- [Numerics](features/numerics.md) — `Auturge.Numerics`: the exact
  arbitrary-precision `Number` type, `Fraction<T>`, and `Formula`.
- [Quantity](features/quantity.md) — `Auturge.Quantity` /
  `Auturge.Quantity.Numerics`: dimensional quantities, the unit library, and
  the unit-conversion engine.
- [Identifiers](features/identifiers.md) — `Auturge.Identifiers`: Snowflake IDs
  (`Flake`) and URN-based names & references.
- [Stores](features/stores.md) — `Auturge.Stores`: composable entity
  archetypes, the `IStore` repository contract, and `InMemoryStore`.

## Development

- [Licenses](_dev/licenses.md)
