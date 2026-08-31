# Identifiers (`Auturge.Identifiers`)

Two small identifier systems:

- **Flake** — Snowflake-scheme 64-bit ids for distributed systems: a sortable
  `long` that packs a timestamp, a datacenter, a machine, and a per-millisecond
  sequence counter.
- **URN** — RFC 8141 Uniform Resource Names (`urn:<nid>:<nss>`) plus lightweight
  `Reference` / `ResourceLink` value types for "a display name pointing at a
  thing".

- **Assembly:** `Auturge.Identifiers`
- **Namespace:** `Auturge.Identifiers` (plus `Auturge.Identifiers.Instances`)
- **Target:** `net10.0`
- **Dependencies:** none (BCL only)

| Type | Kind | Purpose |
| --- | --- | --- |
| [`Flake`](#flake) | `readonly struct` | A decoded view of a 64-bit snowflake id |
| [`FlakeConfig`](#flakeconfig) | `readonly struct` | The bit layout + epoch used to encode/decode flakes |
| [`FlakeConfigs`](#flakeconfigs) | `static class` | Ready-made layouts (`Funsies`, `Twitter` / `SnowFlake`) |
| [`FlakeGenerator`](#flakegenerator) | `sealed class` | Produces monotonically increasing ids for one source |
| [`URN`](#urn) | `abstract partial class : Uri` | An `urn:<nid>:<nss>` name; derive per namespace |
| [`Reference<T>`](#referencet) | `class` | A display name paired with the resource it identifies |
| [`ResourceLink<T>`](#resourcelinkt) | `class` | A `Reference<T>` plus a resolvable `Uri` |

---

## Flake

A flake **is** a signed 64-bit `long`. The `Flake` struct is a *decoded view*:
it holds the packed `Value` and also surfaces the components extracted from it.

```
  +------+---------------------+------------+-----------+--------------+
  | sign |      timestamp      | datacenter |  machine  |   sequence   |
  |  1b  |   (ms since epoch)  |            |           |              |
  +------+---------------------+------------+-----------+--------------+
   MSB                                                             LSB
```

Field widths come from the active `FlakeConfig` (machine and datacenter may be
0 bits). The sign bit is always `0`, so flakes are non-negative and sort in
creation order.

### Layout — `FlakeConfig`

`FlakeConfig` describes how the 63 usable bits are divided and what instant the
timestamp counts from.

| Member | Meaning |
| --- | --- |
| `Epoch` | Point the timestamp counts from, as non-negative Unix ms |
| `SequenceBits` / `MachineBits` / `DatacenterBits` | Field widths (machine/datacenter may be 0) |
| `TimestampBits` | What's left: `63 - Sequence - Machine - Datacenter` |
| `MaxSequence` / `MaxMachineNum` / `MaxDatacenterNum` | Largest value each field can hold |
| `SequenceOffset` (0) / `MachineOffset` / `DatacenterOffset` / `TimestampOffset` | Bit positions |
| `RolloverDate` | Instant past which the timestamp no longer fits — `DateTime.MaxValue` if unrepresentable |

```csharp
var layout = new FlakeConfig(
    epoch: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    sequenceBits: 20, machineBits: 0, datacenterBits: 0);
```

Constraints, enforced in the constructor:

- `Epoch` must be non-negative (and, for the `DateTime` overload, in the past).
- `SequenceBits >= 1` — the generator needs at least one bit to hand out more
  than one id per millisecond.
- The fields must leave **at least 35 timestamp bits** (~1 year); fewer always
  means an accidental sub-year rollover, so it throws.

Flakes encoded under one layout **cannot** be decoded under another.

### Ready-made layouts — `FlakeConfigs`

| Layout | Epoch | Seq / Machine / DC bits | Notes |
| --- | --- | --- | --- |
| `Funsies` | 2025-01-01 UTC | 20 / 0 / 0 | Single-source; ~1,048,576 ids/ms; 43 timestamp bits (~279 years) |
| `Twitter` | 2010-11-04 01:42:54 UTC | 12 / 5 / 5 | The classic Twitter snowflake; 4,096 ids/ms |
| `SnowFlake` | — | — | Alias for `Twitter` |

### The ambient layout — `Flake.Config` / `Flake.Configure`

`Flake.NewFlake()` and the single-argument decode constructor use a
**process-wide** layout. It starts as `FlakeConfigs.Funsies`.

```csharp
// once, at start-up:
Flake.Configure(FlakeConfigs.Twitter, dataCenterId: 1, machineId: 4);

// or just the layout, keeping the current source ids:
Flake.Config = FlakeConfigs.SnowFlake;
```

- Intended to be set **once at start-up**. In a distributed deployment give each
  node a distinct datacenter/machine pair.
- Individual reads and writes are thread-safe: the config, the source ids, and a
  matching generator travel together as one immutable object swapped by a single
  volatile reference write, so a concurrent reader always sees a consistent set.
- Assigning a layout whose machine/datacenter fields are too narrow for the
  **currently configured** source ids throws `ArgumentOutOfRangeException`.

### Generating ids

```csharp
long id = Flake.NewFlake();                 // ambient layout + ambient source ids

var gen = new FlakeGenerator(FlakeConfigs.Twitter, datacenterId: 1, machineId: 4);
long next   = gen.GetNextId();               // raw long
Flake flake = gen.NewFlake();                // decoded
```

`FlakeGenerator` is one source. A single instance is thread-safe (it locks
around the counter); use **one per datacenter/machine pair**. The parameterless
constructor uses `FlakeConfigs.SnowFlake`; an explicit `TimeProvider` can be
passed (mainly so tests control the clock).

> **The generator fails loudly on a bad clock.** `GetNextId` throws
> `InvalidOperationException` when the clock has moved **backwards** since the
> last id (uniqueness can no longer be guaranteed — `_lastStamp` lives only in
> memory, so a restart behind the previous time would re-issue ids), or when it
> currently reads **outside the layout's window** (before the epoch or past the
> rollover date — packing would corrupt the id). Callers expecting a transient
> correction may catch and retry. Within a millisecond, once the sequence space
> is spent the generator spins until the next millisecond.

### Decoding and encoding directly

```csharp
var f = new Flake(id);                       // decode with the ambient Config
var g = new Flake(id, FlakeConfigs.Twitter); // decode with an explicit layout

var built = new Flake(sequence: 0, timestamp: 1_700_000_000_000,
                      dataCenterId: 1, machineId: 4);   // encode; ambient Config
```

| Component | Type | Meaning |
| --- | --- | --- |
| `Value` | `long` | The packed id. Equality and ordering are defined **solely** by this. |
| `DataCenterId` | `long` | Datacenter component |
| `MachineId` | `long` | Machine component |
| `Sequence` | `long` | Per-millisecond counter |
| `TimeStamp` | `DateTime` | UTC instant the flake was generated (ms precision) |

The encode constructor throws `ArgumentOutOfRangeException` if a component is
outside what the active `Config` can represent.

### Equality, ordering, conversion

- `==`, `!=`, `Equals`, `CompareTo(Flake)`, `CompareTo(long)` all compare
  `Value` only. `GetHashCode` hashes `Value` only — the component properties are
  derived and may be left unset when a `Flake` is built with an object
  initializer.
- `implicit operator long(Flake)` unwraps to `Value`.
- `ToString()` is `Value.ToString(CultureInfo.InvariantCulture)`.
  `ToComponentString()` renders `D:… M:… S:… T:…` for diagnostics.

---

## URN

An `abstract` class modelling `urn:<nid>:<nss>` per
[RFC 8141](https://www.rfc-editor.org/rfc/rfc8141). It extends `System.Uri`, so
a URN is usable anywhere a `Uri` is. Derive it inside your own namespace.

```csharp
public sealed partial class BookUrn : URN
{
    public BookUrn(string isbn) : base("isbn", isbn) { }
}

var u = new BookUrn("0451524934");        // urn:isbn:0451524934
```

| Member | Meaning |
| --- | --- |
| `NID` | Namespace Identifier — the category. Compared **case-insensitively** (ASCII / Ordinal). |
| `NSS` | Namespace Specific String — the name within the `NID`. Compared **case-sensitively** (Ordinal). |
| `URN(string s)` | Parse a full `urn:nid:nss` string |
| `URN(string nid, string nss)` | Compose from parts |
| `protected static TryParseParts(s, out nid, out nss)` | Non-throwing split, for a subclass `TryParse` |
| `protected const _urnRegexOptions` | Shared `RegexOptions` for subclasses that parse their own NSS |

Validation (RFC 8141): the NID is 2–32 alphanumerics with internal hyphens; the
NSS is one or more `pchar`s (`unreserved` / `sub-delims` / `:` / `@` / `/`) or
percent-encoded octets. Invalid input throws `FormatException`; `null` throws
`ArgumentNullException`.

> **Why every comparison is overridden.** `Uri` implements `IEquatable<Uri>`, so
> a `URN`-typed `Equals(other)` call would otherwise bind to `Uri`'s
> case-sensitive whole-string comparison. `Equals(URN)`, `Equals(object)`, `==`,
> `!=`, and `GetHashCode` are all routed through the same NID (case-insensitive)
> / NSS (ordinal) check so the RFC's equivalence rules hold on every path.

---

## `Reference<T>`

```csharp
public class Reference<T>(string displayName, T resource) : IEquatable<Reference<T>>
    where T : notnull
```

A human-readable label paired with the thing it points at. Both parts are
required (`ArgumentNullException` otherwise) and read-only.

```csharp
var r = new Reference<BookUrn>("Nineteen Eighty-Four", new BookUrn("0451524934"));
var copy = new Reference<BookUrn>(r);   // reprojects: display name + resource only
```

| Member | Meaning |
| --- | --- |
| `DisplayName` | The label |
| `Resource` | The referenced resource (`T`) |
| `Reference(Reference<T> original)` | Copy constructor — copies the two members, **drops** any subclass state |
| `protected virtual EqualsCore(Reference<T>)` | The type-specific check, run only after runtime types match |

Every comparison path (`==`, `Equals(object)`, `IEquatable<Reference<T>>`)
funnels through `Equals(object)` → `EqualsCore`, so a subclass that widens
equality stays consistent across all of them. Default `EqualsCore` compares
`DisplayName` and `Resource`.

## `ResourceLink<T>`

```csharp
public class ResourceLink<T>(Reference<T> reference, Uri link) : Reference<T>(reference), IEquatable<ResourceLink<T>>
    where T : notnull
```

A `Reference<T>` plus a resolvable location.

| Member | Meaning |
| --- | --- |
| `Link` | Where the resource can be retrieved (`Uri`, required) |

`EqualsCore` is overridden to also require `Link` equality, and `GetHashCode`
folds it in — so two `ResourceLink`s are equal only when display name, resource,
**and** link all match.

---

## Testing

Pinned by `Auturge.Identifiers.Tests` (NUnit): flake encode/decode round-trips,
generator monotonicity and clock-failure behavior, `FlakeConfig` validation and
derived offsets, ambient-config thread-safety, URN parsing/validation/equality
against the RFC, and `Reference` / `ResourceLink` equality including the subclass
consistency rules.
