# Numerics (`Auturge.Numerics`)

An exact, arbitrary-precision decimal number type plus a couple of supporting
helpers. The project exists so that costing math — purchase prices, yields,
per-unit costs — can be done without the rounding error of `double` or the
range ceiling of `decimal`.

- **Assembly:** `Auturge.Numerics`
- **Namespace:** `Auturge.Numerics`
- **Target:** `net10.0`
- **Dependencies:** none (BCL only)

| Type | Kind | Purpose |
| --- | --- | --- |
| [`Number`](#number) | `partial struct` | Exact arbitrary-precision decimal |
| [`NumberExtensions`](#numberextensions) | `static class` | `Floor`/`Truncate`/`Round`/`ConvertsTo` sugar |
| [`Fraction<T>`](#fractiont) | `class` | Approximates a value as a vulgar fraction (numerator/denominator) |
| [`Formula<TValue, T1>`](#formulatvalue-t1) | `abstract class` | Ordered pipeline of `(value, arg) => value` steps |

---

## `Number`

`Number` is a value type that stores a decimal number exactly, with no fixed
precision limit and no binary rounding error. Internally it is a
sign + arbitrary-precision significand + decimal offset:

| Member | Type | Meaning |
| --- | --- | --- |
| `RawValue` (internal) | `BigInteger` | The significand, **always a magnitude** (never negative) |
| `DecimalOffset` (internal) | `int` | Count of fractional digits (a non-negative "negative exponent") |
| `Sign` | `int` | `-1`, `0`, or `+1` |
| `IsNegative` | `bool` | Sign flag — the sign lives here, *not* in `RawValue` |
| `IsIntegral` | `bool` | True when `DecimalOffset == 0` |
| `DigitCount` | `int` | Total significant digits, excluding sign and separator |
| `SmallestType` | `Type` | Smallest BCL numeric type that can hold this value (computed lazily) |

> **Sign lives in `IsNegative`/`Sign`, not in `RawValue`.** `RawValue` is the
> absolute significand. Anything that reads `RawValue` directly (casts,
> saturating conversions, formatting) must re-apply the sign itself. Negation
> (`-x`) flips the `IsNegative` flag rather than negating the significand.

The significand is always stored **normalized**: trailing fractional zeros are
trimmed in the constructor (`1.2500` is stored as significand `125`, offset `2`).

### Construction

```csharp
var a = new Number(42L);                 // from any integral primitive — built straight from bits
var b = new Number(1.25m);               // from decimal/double/float/Half — via exact decimal string
var c = new Number(BigInteger.Pow(10, 40));
var d = new Number(123456, exponent: 3); // significand + negative exponent => 123.456
Number e = Number.Parse("3.14159", CultureInfo.InvariantCulture);
Number f = 42;                           // implicit from int
Number g = 3.5;                          // implicit from double
```

Integral primitives (`byte`…`Int128`, `char`) are constructed directly from
their bits through `BigInteger`. Types that carry a fractional part
(`decimal`, `double`, `float`, `Half`) round-trip through their culture-aware
`ToString()` so the exact decimal value is captured rather than the nearby
binary fraction.

Constants: `Number.Zero`, `Number.One`, `Number.Two`,
`Number.AdditiveIdentity`, `Number.MultiplicativeIdentity`.

### Generic math

`Number` implements the full .NET 7+ generic-math stack, so it can be used as
the `T` in any `where T : INumber<T>` / `IFloatingPoint<T>` API:

`INumberBase<Number>`, `IFloatingPoint<Number>`, `ISignedNumber<Number>`,
`IAdditionOperators`, `ISubtractionOperators`, `IMultiplyOperators`,
`IDivisionOperators`, `IModulusOperators`, `IComparisonOperators`,
`IEqualityOperators`, `IIncrementOperators`, `IDecrementOperators`,
`IUnaryNegationOperators`, `IUnaryPlusOperators`, `IComparable`,
`IComparable<Number>`, `IEquatable<Number>`, `IConvertible`,
`IParsable<Number>`, `ISpanParsable<Number>`, `IFormattable`,
`ISpanFormattable`.

`Radix` is `10`. `IsFinite`/`IsRealNumber`/`IsCanonical` are always `true`;
there is no NaN, infinity, or subnormal.

### Arithmetic

```csharp
Number sum  = new Number(0.1m) + new Number(0.2m);   // exactly 0.3
Number diff = a - b;
Number prod = a * b;
Number quot = a / b;                                  // see note on division
Number rem  = a % b;
```

- **Addition/subtraction** align both operands to the larger `DecimalOffset`,
  operate on signed `BigInteger`s, and re-wrap.
- **Multiplication** multiplies the significands and adds the offsets — exact.
- **Division is not exact.** `operator /` delegates to
  `Number.Divide(dividend, divisor, fractionalDigits)` with a default of
  **8 fractional digits** (`DefaultFractionalDigitCount`). Call
  `Number.Divide` directly to choose the precision.
- `0 / 0` returns `Number.One` (so the identity `x / x == 1` holds for all
  `x`); any other divide by zero throws `DivideByZeroException`.
- `%` is `dividend - (dividend / divisor) * divisor` computed at 0 fractional
  digits.

### Rounding

`Number.Round(value, digits, MidpointRounding mode)` supports `ToEven`,
`AwayFromZero`, `ToNegativeInfinity`, `ToPositiveInfinity`, and `ToZero`.
`digits` must be `>= 0`.

`NumberExtensions` adds the familiar spellings:

```csharp
n.Floor();               // Round(n, 0, ToNegativeInfinity)
n.Truncate();            // Round(n, 0, ToZero)
n.TruncateTo(3);         // keep 3 fractional digits, toward zero
n.Round(2, MidpointRounding.AwayFromZero);
```

### Conversions

Implicit **to** `Number`: `int`, `double`.
Implicit **from** `Number`: `decimal`, `double`, `int` (via `IConvertible`).

Explicit **from** `Number` to the unsigned integer types and `checked`
variants: `byte`, `ushort`, `uint`, `ulong`, `UInt128` (each floors first,
then casts, re-applying the sign).

`IConvertible` is implemented in full (`ToInt32`, `ToDecimal`, `ToDouble`,
`ToType(Type, IFormatProvider)`, …). There is also a generic
`ToType<T>(IFormatProvider?)` and `ToSmallest()` which converts to
`SmallestType`.

Helpers:

| Method | Returns |
| --- | --- |
| `Number.GetBestType(Number)` | Smallest BCL type that holds the value losslessly, else `typeof(Number)` |
| `Number.ConvertsTo(Number, Type)` / `n.ConvertsTo(Type)` | Whether the value fits that type without loss |
| `n.SmallestType` | Same as `GetBestType`, cached per-instance |

`GetBestType` accounts for the sign when picking an integer type (e.g. `-40000`
→ `int`, not `ushort`), and for floats checks that the type's shortest
round-trip string equals the original.

### Parsing

```csharp
Number.Parse(string, IFormatProvider?);
Number.Parse(ReadOnlySpan<char>, IFormatProvider?);
Number.Parse(string/span, NumberStyles, IFormatProvider?);
Number.TryParse(..., out Number);
```

Default style is `NumberStyles.Float | NumberStyles.AllowThousands`.
Scientific notation (`1.5e30`, `5.29E-11`) is supported — a negative exponent
is folded into the significand so `DecimalOffset` stays non-negative.
Parse failures throw `NumberParseException` (from `Number.Parse`) or return
`false` (from `TryParse`).

### Formatting

`ToString()` with no arguments is the **round-trip** form: every significant
digit, current culture's separators, no rounding.

`ToString(string? format, IFormatProvider?)` implements the .NET
[standard numeric format strings](https://learn.microsoft.com/dotnet/standard/base-types/standard-numeric-format-strings):

| Specifier | Behavior |
| --- | --- |
| `R`, `G` (no precision) | Round-trip / positional, all digits |
| `G{n}` | `n` significant digits — **stays positional**, never switches to scientific |
| `F{n}` | Fixed-point |
| `N{n}` | Fixed-point with group separators |
| `C{n}` | Currency (culture patterns/symbol) |
| `P{n}` | Percent (value × 100) |
| `E{n}` / `e{n}` | Scientific, 3-digit exponent |
| `D`, `X`, `B` | Integral only — throws `FormatException` if the value has a fractional part |

Custom (non-`letter[digits]`) format strings are **not** supported and throw
`FormatException`.

> **Design divergences from the BCL, on purpose:**
> - Rounding is **half-away-from-zero on the exact value** everywhere
>   (matching `decimal`'s long-standing fixed-precision behavior), not
>   half-to-even. Since `Number` is exact there is no representation error to
>   reason about.
> - `G{n}` never switches to scientific notation the way BCL `G` does — it
>   rounds to `n` significant digits and stays positional.

### Equality & hashing

`==`, `!=`, `<`, `<=`, `>`, `>=`, `CompareTo`. `GetHashCode` combines
`IsNegative`, `RawValue`, and `DecimalOffset` — consistent with `Equals`
because the significand is always stored normalized. Convenience overloads
compare a `Number` directly against `double` and `int`.

### `IFloatingPoint` layout members

`GetExponentByteCount`, `GetSignificandByteCount`,
`TryWriteSignificandBigEndian`/`LittleEndian`,
`TryWriteExponentBigEndian`/`LittleEndian`, etc. expose the significand
(`RawValue`) and exponent (`DecimalOffset`) as byte spans. `E`, `Pi`, `Tau`
are provided as high-precision decimal literals.

---

## `NumberExtensions`

`static` helpers over `Number`: `Floor()`, `Truncate()`, `TruncateTo(int)`,
`Round(int, MidpointRounding)`, `ConvertsTo(Type)`. All forward to the
corresponding `Number` static methods.

---

## `Fraction<T>`

```csharp
public class Fraction<T> where T : INumber<T>, IConvertible
```

Approximates a value as a vulgar fraction by walking the Stern–Brocot tree
until within `error` (default `1e-10`).

```csharp
var third = new Fraction<double>(0.3333333333);
// third.Numerator == 1, third.Denominator == 3, third.Value == 0.3333333333
```

| Member | Meaning |
| --- | --- |
| `Value` | The original approximate value (`T`) |
| `Numerator` | `BigInteger` |
| `Denominator` | `BigInteger` |

The constructor converts `T` to `decimal` (via `IConvertible`) for the search,
so the input must fit in `decimal`. A `TODO` in the source notes the intent to
re-implement this against `Number` for arbitrary precision.

---

## `Formula<TValue, T1>`

```csharp
public abstract class Formula<TValue, T1>(params Func<TValue, T1, TValue>[] operations) : IFormula
```

An ordered pipeline. `Apply(value, arg)` folds each operation over an
accumulator, left to right:

```csharp
sealed class Markup() : Formula<Number, Number>(
    (cost, pct) => cost + cost * pct / new Number(100L),
    (withMargin, _) => withMargin.Round(2, MidpointRounding.AwayFromZero));

Number price = new Markup().Apply(cost, marginPercent);
```

`IFormula` is a bare marker interface.

---

## Testing

Behavior is pinned by the `Auturge.Numerics.Tests` suite (NUnit): construction,
arithmetic, comparison, conversion, parsing, scientific notation, rounding,
formatting, `INumberBase` API surface, and `IFloatingPoint` byte layout.
