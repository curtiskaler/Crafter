# Quantity (`Auturge.Quantity`)

Dimensional quantities — an amount paired with a unit — plus dimensional
analysis, a unit library, and a unit-conversion engine. This is what lets the
costing model say "2 cups of flour" and "$4.50 per kg" and reconcile the two.

- **Assemblies:** `Auturge.Quantity` (generic core), `Auturge.Quantity.Numerics` (bindings to [`Number`](numerics.md))
- **Namespace:** `Auturge.Quantity`
- **Target:** `net10.0`
- **Dependencies:** `Auturge.Identifiers` (for `Flake` snowflake IDs); `Auturge.Quantity.Numerics` also references `Auturge.Numerics`

## The two assemblies

`Auturge.Quantity` is generic over the numeric type: `Quantity<T>`,
`UnitConversion<T>`, `UnitConversions<T>`, `Conversion<T>` — all constrained
roughly to `where T : IEquatable<T>, INumber<T>, IConvertible`.

`Auturge.Quantity.Numerics` closes those generics over
[`Number`](numerics.md) and adds the non-generic spellings you normally use:

| Generic (core) | Closed over `Number` (`.Numerics`) |
| --- | --- |
| `Quantity<Number>` | `Quantity` |
| `UnitConversions<Number>` | `UnitConversions` |
| `ConvertedQuantity<Number>` | `ConvertedQuantity` (internal) |

```csharp
using Auturge.Quantity;

var flour  = new Quantity(2, Units.Kilograms);
var inGrams = flour.ConvertTo(Units.Grams);   // 2000 g
```

---

## Core model

### `Quantity<T>` / `Quantity`

```csharp
public class Quantity<T>(T amount, Unit unit) : IQuantity<T>, IEquatable<Quantity<T>>
    where T : IEquatable<T>, INumber<T>, IConvertible
```

| Member | Meaning |
| --- | --- |
| `Amount` | The scalar (`T`) |
| `Unit` | The [`Unit`](#unit) |
| `ConvertTo(Unit target)` | Returns a new quantity in `target` (a `ConvertedQuantity<T>`); throws `ArgumentException` if the target is a different [`Dimension`](#dimension--dimensionvector) |
| `ToString()` | `"{Amount} {Unit.Symbol}"` |

**Arithmetic operators:**

| Operator | Rule |
| --- | --- |
| `q + q`, `q - q` | Operands must share both `Dimension` **and** `Unit`, else `IncompatibleUnitTypeException` / `IncompatibleUnitException`. No implicit conversion. |
| `q + T`, `q - T` | Adds/subtracts a bare scalar, keeping the unit |
| `q * q`, `q / q` | Multiplies/divides amounts **and** units (`m * m` → area, `m / s` → velocity) |
| `q * T`, `q / T` | Scales the amount, keeps the unit |

`Equals` / `==` compare `Amount` and `Unit` (and require the exact same runtime
type). `GetHashCode` combines both.

The non-generic `Quantity` (in `.Numerics`) additionally offers
`implicit operator Quantity(int n)` → `new Quantity(n, Units.Each)`, so a bare
count like `5` becomes "5 each".

`ConvertedQuantity<T>` (internal) is what `ConvertTo` returns. It remembers the
quantity it was converted **from**, forming a linked list of conversions.
Converting back to a unit already in that chain short-circuits to the stored
value (iteratively, not recursively) instead of recomputing.

### `Unit`

```csharp
public sealed class Unit : IEquatable<Unit>, IHaveNameAndSymbol, IHaveSynonyms<Unit>
```

| Member | Meaning |
| --- | --- |
| `Id` | `Flake` snowflake ID |
| `DisplayName`, `Symbol` | e.g. `"kilograms"`, `"kg"` — `DisplayName` may be an i18n key |
| `Dimension` | The [`Dimension`](#dimension--dimensionvector) this unit measures |
| `Base` | The unit this one is defined against (`null` ⇒ this is a base unit) |
| `Factor`, `Divisor` | Exactly one deviates from `1`; `ToBase` (internal) = `Factor / Divisor`, this unit's exact [`Rational`](#rational) ratio to `Base` |
| `Definition` | [`UnitDefinition`](#unitdefinition) — base-unit exponents (e.g. Newton = `kg·m·s⁻²`) |
| `Synonyms` | Alternate name/symbol pairs |
| `IsBase` | `Base == null` |
| `Unit.One` | The dimensionless unit |

**Operators** build derived units and consult the [`Units`](#the-units-library)
cache before allocating:

```csharp
Unit perSecond = 1.0 / Units.Seconds;         // reciprocal (Hz-shaped)
Unit area      = Units.Meters * Units.Meters;
Unit velocity  = Units.Meters / Units.Seconds;
Unit kilo      = SIPrefixes.Kilo * Units.Grams; // SIPrefix<Rational> * Unit
```

`Reciprocal()`, `Reciprocal(Unit)` are also exposed directly.

> **Equality is structural, and `GetHashCode` deliberately hashes only
> `Dimension`.** Two units are equal when their `Id` matches *or* their
> `Dimension` **and** `Definition` match. The hash can't include `Definition`
> because a `Unit` is itself the key type of `UnitDefinition` and that
> dictionary is built up by mutation *after* construction (base units add
> themselves; `*` and `/` call `IncludeBaseUnits`) — a `Definition`-derived
> hash would change after the unit was used as a dictionary key.
> `Dimension`'s exponent vector is init-only and stable, so that is all that
> is folded in.

### `Dimension` / `DimensionVector`

`DimensionVector` is the seven SI base exponents:

| Property | Symbol | Base quantity |
| --- | --- | --- |
| `Time` | T | time |
| `Length` | L | length |
| `Mass` | M | mass |
| `ElectricCurrent` | I | electric current |
| `AbsoluteTemperature` | Θ | thermodynamic temperature |
| `AmountOfSubstance` | N | amount of substance |
| `LuminousIntensity` | J | luminous intensity |

`Analysis` renders the vector as a string like `"L M T^-2"`. `*`, `/`, and
`Reciprocal()` add/subtract/negate the exponents. Equality and hashing are
purely by the seven exponents. `DimensionVector.One` is all-zero
(dimensionless).

`Dimension : DimensionVector` adds `Id`, `DisplayName`, `Symbol`, and
`Synonyms`. Its `*` / `/` operators reduce through the
[`Dimensions`](#the-dimensions-library) cache — if a dimension with the
resulting vector already exists it is returned, otherwise a new one is wrapped
around the vector. `Dimension` equality is `Id` match **or** vector match; the
hash is the vector's hash (so `Equal` dimensions share a bucket).

### `Rational`

```csharp
public readonly struct Rational : IEquatable<Rational>,
    IMultiplyOperators<…>, IDivisionOperators<…>, IMultiplicativeIdentity<…>
```

An exact `BigInteger / BigInteger`, always stored fully reduced with a positive
denominator. Used where arbitrary precision is wanted but a generic
`INumber<T>` parameter isn't available — notably the `Factor`/`Divisor` fields
on the shared, non-generic `Unit`. Unlike `decimal` there is no range ceiling;
unlike `double` there is no rounding error.

| Member | Meaning |
| --- | --- |
| `Numerator`, `Denominator` | `BigInteger`, reduced |
| `Rational.Zero`, `Rational.One`, `MultiplicativeIdentity` | Constants |
| `+ - * /`, unary `-`, `< <= > >= == !=` | Exact arithmetic and comparison |
| `Reciprocal()` | `denominator / numerator` |
| `To<T>()` where `T : INumberBase<T>` | Bridge to any generic-math type via `CreateChecked` on numerator and denominator |
| `Rational.FromDecimal(decimal)` | Exact, via `decimal.GetBits` |
| implicit from `int`/`long`/`BigInteger`/`double` | `double` is captured from its **shortest round-trip decimal string**, not its binary bits — so a literal like `0.45359237` becomes exactly that rational |
| explicit to `double` | Narrowing, lossy — display/debug/interop only |
| `new Rational(n, 0)` | throws `DivideByZeroException` |

---

## Libraries (static caches)

### `MemberCache<TSelf, TElement>`

Base class for the three libraries below. `Items` is every `public static`
member of `TSelf` of type `TElement` (found by reflection) **plus** anything
added at runtime via `Add(params TElement[])` (deduplicated by `Equals`).
`TryFind(Func<TElement,bool>, out TElement)` is the lookup primitive.

> **Init-order guard.** The static members are merged in lazily on first access
> and **re-scanned until the reflected member count settles two scans in a
> row** — not merged once in a static constructor. A `MemberCache` type
> initializer can be triggered while `TSelf`'s own initializer is still running
> (a `Unit`/`Dimension` operator in a field initializer calls back into
> `MemberCache.Items`), which would otherwise freeze the cache around a
> half-built snapshot and silently drop every member declared lower in the
> file.

### The `Dimensions` library

`Dimensions : MemberCache<Dimensions, Dimension>`. Base SI dimensions (`Time`,
`Length`, `Mass`, `ElectricCurrent`, `Temperature`, `Amount`,
`LuminousIntensity`) plus ~60 derived ones (`Area`, `Volume`, `Velocity`,
`Acceleration`, `Force`, `Pressure`, `Energy`, `Power`, and a large electrical
/ magnetic set). Sentinels: `Any` (id `-1`), `None` (id `0`), `One` (id `1`,
dimensionless).

Lookup: `Dimensions.Find(DimensionVector)` (throws `KeyNotFoundException`),
`TryFind(…)`, `FindOrAdd(DimensionVector)`.

### The `Units` library

`Units : MemberCache<Units, Unit>`. Hundreds of units:

- **Time:** seconds (+ every SI prefix), minutes, hours, days, weeks,
  fortnights, months, years, decades, centuries, kilo/mega/giga/…-years,
  jiffies.
- **Mass:** grams (+ SI prefixes), tonnes, avoirdupois pounds, ounces, grains,
  short/long tons.
- **Volume:** liters (+ SI prefixes), imperial / US / US-food-labeling fluid
  ounces.
- **Length (SI):** meters (+ SI prefixes), astronomical units, light-years,
  parsecs (+ kilo/mega/giga), ångström, Bohr radii.
- **Length (imperial):** inches, feet, yards, miles, leagues.
- **Derived physical:** m/s, ft/s, ft/min, mph, m/s², gees, Newtons,
  pound-force.

Convenience one- or two-letter property aliases exist for common units
(`Units.kg`, `Units.mL`, `Units.km`, `Units.N`, …). `Units.One` and
`Units.Each` are the dimensionless counters; `Units.Any` is the wildcard.

Lookup: `Units.FindAll(DimensionVector)`,
`Units.TryFind(DimensionVector, UnitDefinition, out Unit)`,
`Units.TryFind(Func<Unit,bool>, out Unit)`.

### `SIPrefix<T>` / `SIPrefixes`

```csharp
public class SIPrefix<T> : IHaveNameAndSymbol
    where T : IMultiplyOperators<T,T,T>, IDivisionOperators<T,T,T>, IMultiplicativeIdentity<T,T>
```

`SIPrefixes` holds the full set from `quecto` (10⁻³⁰) to `quetta` (10³⁰) as
`SIPrefix<Rational>`. `SIPrefix<Rational> * Unit` produces the prefixed unit
(and checks the `Units` cache first). Sub-unit prefixes set `Divisor`;
super-unit prefixes set `Factor` — the two-overload constructor avoids a
`default(T)` divisor for interface-only `T`.

### `UnitDefinition`

```csharp
public class UnitDefinition : Dictionary<Unit, short>, IEquatable<IDictionary<Unit, short>>
```

A map of base `Unit` → exponent. `ToString()` renders `"kg m s^-2"`.
`*` / `/` add / subtract exponents; `Reciprocal()` negates them.
`IncludeBaseUnits(params Unit[])` folds a base unit into its own definition
(base units otherwise have an empty definition).

> **Equality matches keys by base-unit `Id`, not by `Unit.Equals`.** `Unit`
> equality defers back to a `Definition` comparison, and a base unit's
> definition contains itself — a structural key match would recurse with no
> base case. `GetHashCode` is order-independent over `(Id, exponent)` pairs.

---

## Conversion engine

### `Conversion` / `Conversion<T>` and `Bijection`

A `Bijection` (from `Operation`) is an invertible pair of function lists — a
forward chain and an inverse chain, each guaranteed non-empty. `Conversion`
adds `Execute(object)` (fold the forward functions) and `Invert()`.
`Conversion.Identity` is `x => x`.

```csharp
var c2f = new Conversion<double>(c => c * 9 / 5 + 32, f => (f - 32) * 5 / 9);
c2f.Execute(100);                       // 212
((IBijection<double>)c2f).Invert().Execute(212);   // 100
```

Composition:

| Operator | Meaning |
| --- | --- |
| `a * b` | do `a`, then `b` (inverse is `b⁻¹` then `a⁻¹`) |
| `a / b` | do `a`, then `b⁻¹` |

`FunctionExtensions.Box<T>()` / `Unbox<T>()` move delegates between the typed
`Func<T,T>` world and the boxed `Func<object,object>` world that the
non-generic `Conversion` stores.

### `UnitConversion` / `UnitConversion<T>`

A `SourceUnit → TargetUnit` pair plus the `Conversion` between them.

```csharp
var inPerFoot = UnitConversion<Number>.Create(Units.Feet, Units.Inches, 12);
var custom    = UnitConversion<Number>.Create(src, tgt, forward, inverse);
var fromFactor = UnitConversion<Number>.Create<double>(src, tgt, 2.54); // factor of any numeric type
```

- `Convert(T)` applies the conversion; `Invert()` swaps direction.
- `CanHandle(a, b)` is `true` if `{a,b} == {SourceUnit, TargetUnit}` in either
  order.
- `a * b` / `a / b` compose two conversions; a `List<UnitConversion<T>>`
  converts implicitly to a single chained `UnitConversion<T>`.
- Equality is **source + target only** — the conversion functions are opaque
  lambdas, and there should never be two different conversions between the same
  pair.

### `UnitConversions<T>` / `UnitConversions`

`MemberCache<UnitConversions<T>, UnitConversion<T>>` — the library of known
conversions (SI-prefix ladders for meters/seconds/years, imperial length
chains, time, astronomical units, …).

```csharp
UnitConversion<Number> conv = UnitConversions.Find(Units.Feet, Units.Meters);
bool ok = UnitConversions<Number>.TryFind(source, target, out var conv);
```

`Find` throws `ConverterNotFoundException` when no path exists.

Resolution order in `TryFind`:

1. `source == target` → identity.
2. A cached conversion whose `CanHandle(source, target)` is `true`.
3. Otherwise **build one**: construct a `UnitConversionGraph<T>` from all known
   conversions and search for a path.

### `UnitConversionGraph<T>` / `DirectedGraph<T>`

`DirectedGraph<T>` is an undirected adjacency-list graph (edges added both
ways) with BFS shortest-path (`TryFindBFS`, `FindShortedPathBFS`) and DFS.

`UnitConversionGraph<T>` builds a graph from a set of `UnitConversion<T>`
(adding edges to each unit's `Base` too), then `TryFindPath(start, target)`:

1. Splits `start` and `target` into numerator and denominator units
   (`GetNumeratorUnits` / `GetDenominatorUnits` — exponent > 0 / < 0, or
   `Unit.One`).
2. Finds a conversion for the numerator and one for the denominator,
   pairing units by matching `Dimension` (there must be exactly one target
   unit per dimension).
3. Combines them into a fraction converter — the numerator conversion is
   applied to an exact `Rational` numerator, the denominator conversion to the
   denominator, via `Rational.FromDecimal`.

This is how `m/s → mph` is derived without a hand-written converter for the
compound unit. Parts of the graph search are marked in-source as naive /
work-in-progress.

---

## Interfaces & supporting types

| Type | Role |
| --- | --- |
| `IQuantity` / `IQuantity<out T>` | `Unit` / `Unit` + `Amount` |
| `IHaveNameAndSymbol` | `DisplayName`, `Symbol` |
| `IHaveSynonyms<out T>` (internal) | `Synonyms` + `AddSynonym(…)` fluent overloads |
| `Synonym` | A name/symbol alias |
| `IUnitConversion` | `SourceUnit` / `TargetUnit` / `CanHandle` abstraction over `UnitConversion` and `UnitConversion<T>` |
| `INumericConversion<T>` | Marker constraint (`IMultiplyOperators` + `IDivisionOperators`) for conversion-capable numerics |
| `IBijection` / `IBijection<T>` | Invertible operation |
| `Operation` | Base for `Bijection` — holds the forward `Functions` list |
| `Converter<TIn,TOut>` / `Converter<T>` / `Converter` | Plain forward/inverse delegate pair |
| `DoubleExtensions` | `ApproxEqual(rhs, epsilon)` and the internal `ExpectOne()` guard used by the `1.0 / unit` reciprocal operator |
| `Reflection` | Internal — powers `MemberCache`'s static-member scan |

## Exceptions

All derive from `UnitException` (except `ConverterException`, which derives
from `Exception`):

| Exception | Thrown when |
| --- | --- |
| `IncompatibleUnitTypeException` | `+` / `-` on quantities of different **dimensions** |
| `IncompatibleUnitException` | `+` / `-` on quantities of the same dimension but different **units** |
| `ConverterException` | base for conversion failures |
| `ConverterNotFoundException` | `UnitConversions.Find` / `ConvertTo` can't find or build a path (carries `SourceUnit`, `TargetUnit`) |

Messages come from the `RS` resource file (`RS.resx`).

---

## Worked examples

```csharp
using Auturge.Quantity;

// Simple SI ladder
new Quantity(42, Units.Kilometers).ConvertTo(Units.Meters).Amount;   // 42000

// Imperial chain: ft -> in -> cm -> m
new Quantity(1, Units.Feet).ConvertTo(Units.Meters).Amount;          // 0.3048

// Generic over an arbitrary numeric type
new Quantity<double>(2.54, Units.Centimeters).ConvertTo(Units.Inches).Amount; // 1

// Dimensional arithmetic
Quantity speed = new Quantity(100, Units.Meters) / new Quantity(9, Units.Seconds);
// speed.Unit.Dimension == Dimensions.Velocity

// Round-trip is free: converting back reuses the stored original
var m  = new Quantity(5, Units.Meters);
var ft = m.ConvertTo(Units.Feet);
var back = ft.ConvertTo(Units.Meters);   // returns the original 5 m, no recompute
```

## Testing

`Auturge.Quantity.Tests` (NUnit) covers dimensions and dimension vectors, unit
operations and equality, unit definitions, `Rational` (including edge cases),
SI prefixes, synonyms, the directed graph, and end-to-end conversions for
length, generic `T`, and `Number`.
