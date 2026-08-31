# Stores (`Auturge.Stores`)

A tiny repository layer. Instead of one entity interface that forces an id,
audit columns, a concurrency token, and a soft-delete flag onto everything, it
offers **opt-in capability interfaces**, ready-made base classes that compose
them, an async `IStore` contract, and an in-memory implementation for tests and
caching.

- **Assembly:** `Auturge.Stores`
- **Namespaces:** `Auturge.Stores` (entities, interfaces, `Store<>`),
  `Auturge.Stores.Stores` (`InMemoryStore<>`)
- **Target:** `net10.0`
- **Dependencies:** `Auturge.Identifiers` (for `Flake` default ids)

| Type | Kind | Purpose |
| --- | --- | --- |
| [`IEntity` … `ISoftDeletable<TUser>`](#archetype-interfaces) | `interface` | Capability markers |
| [`StoredEntity<TKey>` / `StoredEntity`](#storedentity) | `abstract class` | Id + concurrency token |
| [`AuditEntity<TKey,TUser>` / `AuditEntity<TUser>`](#auditentity) | `abstract class` | + audit + soft-delete |
| [`HistoryEntity<…>`](#historyentity) | `abstract class` | Append-only change-log rows |
| [`IStore<TEntity>` / `IStore<TEntity, TKey>`](#istore) | `interface` | The async repository contract |
| [`Store<TEntity>` / `Store<TEntity, TKey>`](#store) | `abstract class` | Base repo that delegates to a backend |
| [`InMemoryStore<TEntity, TKey>` / `InMemoryStore<TEntity>`](#inmemorystore) | `class` | `ConcurrentDictionary`-backed store |

---

## Archetype interfaces

Compose only what an entity actually needs.

| Interface | Members | Use it when… |
| --- | --- | --- |
| `IEntity` | *(marker)* | Base marker for anything the store layer manages |
| `IStoredEntity` | *(marker)* | The entity is persisted as a record |
| `IStoredEntity<TKey>` | — (`: IStoredEntity, IIdentifiable<TKey>`) | Single primary key of type `TKey` |
| `IIdentifiable<TKey>` | `TKey Id { get; init; }` | Any single-key entity — **not** composite keys |
| `IConcurrentEntity` | `Guid ConcurrencyToken { get; set; }` | Concurrent writers must not clobber each other |
| `IAudit` | `DateTimeOffset Created`, `LastUpdated` | Track create/update time — usually **not** lookup tables |
| `IAudit<TUser>` | `+ TUser? CreatedBy`, `LastUpdatedBy` | Also track *who* — `null` ⇒ the system did it |
| `ISoftDeletable` | `bool IsDeleted`, `DateTimeOffset? DeletedAt` | Hide on delete instead of erasing — **not** history tables |
| `ISoftDeletable<TUser>` | `+ TUser? DeletedBy` | Also track who deleted it — `null` ⇒ the system |

`TKey` (and `TEntityKey` on history rows) is constrained `notnull`.

> **`null` means "the system".** On `IAudit<TUser>` / `ISoftDeletable<TUser>` a
> `null` `CreatedBy` / `LastUpdatedBy` / `DeletedBy` marks a row that isn't
> attributed to a user — seed data, migrations, imports, or a built-in
> principal.

---

## `StoredEntity`

```csharp
public abstract class StoredEntity<TKey>(TKey id)
    : IStoredEntity<TKey>, IConcurrentEntity, IEquatable<StoredEntity<TKey>>
    where TKey : notnull;

public abstract class StoredEntity(long? id = null) : StoredEntity<long>(id ?? Flake.NewFlake());
```

The common base: a primary key plus a `ConcurrencyToken` (a GUID v7, seeded in
the constructor). The non-generic `StoredEntity` keys on `long` and **mints a
`Flake`** when you don't supply an id.

```csharp
public sealed class Widget(long? id = null, string name = "") : StoredEntity(id)
{
    public string Name { get; set; } = name;
}
```

**Equality is identity equality** — two instances are equal when they have the
same `Id` (and the same runtime type). `==`, `!=`, `Equals`, and `GetHashCode`
all follow `Id` only, so an entity still equals its own pre-update self.

## `AuditEntity`

```csharp
public abstract class AuditEntity<TKey, TUser>(TKey id, TUser? creator = default)
    : StoredEntity<TKey>(id), IAudit<TUser>, ISoftDeletable<TUser>, IEquatable<AuditEntity<TKey, TUser>>
    where TKey : notnull;

public abstract class AuditEntity<TUser>(long id, TUser? creator = default) : AuditEntity<long, TUser>(id, creator);
```

`StoredEntity` plus audit (`Created` / `LastUpdated` / `CreatedBy` /
`LastUpdatedBy`) and soft-delete (`IsDeleted` / `DeletedAt` / `DeletedBy`).
`Created` / `LastUpdated` seed to `UtcNow`; `CreatedBy` / `LastUpdatedBy` seed to
`creator`. A `null` `creator` marks a **system-created** row.

```csharp
public class User : AuditEntity<long, User>
{
    public static readonly User SYSTEM = new SystemUser(0, "SYSTEM");  // creator: null
    public static readonly User ADMIN  = new SystemUser(1, "ADMIN");   // creator: SYSTEM
    // …
}
```

Equality is still `Id`-only (the audit and concurrency fields are deliberately
excluded).

## `HistoryEntity`

```csharp
public abstract class HistoryEntity<TKey, TEntityKey, TUser> : IHistoryEntity<TKey, TEntityKey, TUser>
    where TKey : notnull where TEntityKey : notnull;

public abstract class HistoryEntity<TUser> : HistoryEntity<long, long, TUser>;
```

Append-only change-log rows — one per insert/update/delete of another entity.
Deliberately **not** `IConcurrentEntity` or `ISoftDeletable` (a history row is
never updated or hidden).

| Member | Meaning |
| --- | --- |
| `Id` | The history row's own key |
| `EntityId` | Key of the entity that changed |
| `Action` | `ChangeType.Insert` / `Update` / `Delete` |
| `ChangedBy`, `TimeStamp`, `TableName` | Who / when / which table |
| `OldValuesJson`, `NewValuesJson` | Before/after snapshots (`null` for insert/delete respectively) |

Derive for table-specific columns, or use the built-in `HistoryEntry` via the
static `Create` factories (one takes an explicit timestamp, one stamps
`UtcNow`).

---

## `IStore`

```csharp
public interface IStore<TEntity> where TEntity : class, IStoredEntity;

public interface IStore<TEntity, in TKey> : IStore<TEntity>
    where TEntity : class, IStoredEntity, IIdentifiable<TKey>
    where TKey : notnull;
```

| Method | Notes |
| --- | --- |
| `IQueryable<TEntity> Query()` | Composable query over **non-deleted** rows |
| `Task<TEntity> AddAsync(entity, ct)` | `ArgumentException` on a duplicate id |
| `Task<IEnumerable<TEntity>> AddRangeAsync(entities, ct)` | Batch; a duplicate or `null` rolls the batch back |
| `Task<bool> DeleteAsync(entity / id, ct)` | `false` if absent or already deleted |
| `Task<TEntity?> FindByAsync(predicate, ct)` / `FindAllByAsync` | Non-deleted matches only |
| `Task<IEnumerable<TEntity>> GetAllAsync(ct)` | Non-deleted rows |
| `Task<bool> ContainsKeyAsync(id, ct)` *(keyed)* | `false` for a soft-deleted key |
| `Task<TEntity?> GetByIdAsync(id, ct)` *(keyed)* | `null` for a soft-deleted key |
| `Task<TEntity> UpdateAsync(entity, ct)` | `KeyNotFoundException` if absent; `InvalidOperationException` on a stale `ConcurrencyToken` |
| `Task<int> SaveChangesAsync(ct)` | Flush staged writes (no-op for the in-memory store) |

**Soft-deleted rows are hidden from every read path** — not just `Query`.

## `Store`

```csharp
public abstract class Store<TEntity> : IStore<TEntity>;
public abstract class Store<TEntity, TKey> : Store<TEntity>, IStore<TEntity, TKey> where TKey : notnull;
```

A base repository that forwards every call to an **injected** `IStore` backend
(there is no hidden default — pass one in).

```csharp
public class UserStore() : Store<User, long>(new InMemoryStore<User, long>());
// production: Store<User, long>(someRelationalBackend)
```

## `InMemoryStore`

```csharp
public class InMemoryStore<TEntity, TKey>(Func<TEntity, TKey>? idSelector = null) : IStore<TEntity, TKey>
    where TEntity : class, IStoredEntity<TKey> where TKey : notnull;

public class InMemoryStore<TEntity>() : InMemoryStore<TEntity, long>(e => e.Id)
    where TEntity : class, IStoredEntity<long>;
```

A thread-safe `ConcurrentDictionary`-backed implementation for unit/integration
tests and lightweight caching. `idSelector` defaults to `e => e.Id`.

- **Entity isolation.** A field-wise copy is taken on every write and returned
  on every read. Mutating an instance after handing it to the store — or after
  reading one back — has no effect until the next `UpdateAsync`. The copy is
  *shallow*: nested reference members (collections, linked entities) are shared.
- **Stamping.** On insert the store stamps only the archetypes the entity
  actually implements (`ConcurrencyToken` for `IConcurrentEntity`,
  `Created`/`LastUpdated` for `IAudit`), on the copy, **before** it's published —
  a concurrent reader never sees an unstamped row. `AddRangeAsync` shares one
  timestamp across the batch.
- **Concurrency.** `UpdateAsync` compares the incoming `ConcurrencyToken`
  against the stored row; a mismatch throws `InvalidOperationException`. On
  success a fresh token and `LastUpdated` are written back.
- **Soft-delete.** For an `ISoftDeletable` entity, `DeleteAsync` sets the flag
  and `DeletedAt`, leaves the key occupied (a re-add still conflicts), and is
  idempotent (a second delete returns `false`). For a non-soft-deletable entity
  it removes the row outright.

Entities that implement `IStoredEntity<TKey>` but none of the capability
interfaces (a lookup row) work fine — nothing is stamped.

---

## Localized messages

Store exception messages (duplicate id, null-in-batch, key-not-found,
concurrency violation) resolve through `RS` (`RS.cs` + the embedded `RS.resx`),
so they can be localized per `CultureInfo`.

---

## Testing

`Auturge.Stores.Tests` (NUnit, 40 tests):

| Fixture | Covers |
| --- | --- |
| `StoreAddTests` | Insert, stamping (`Created`/`LastUpdated`/`ConcurrencyToken`), duplicate id, cancellation |
| `StoreAddRangeTests` | Batch insert, per-row stamping, rollback on duplicate / `null` / cancellation |
| `StoreUpdateTests` | Persist + return, token advance, `LastUpdated` bump, stale-token violation, missing key, cancellation |
| `StoreDeleteTests` | Idempotent soft-delete, key stays occupied, every read path hides deleted rows |
| `InMemoryStoreTests` | The `long`-keyed convenience class, hard-delete, `DeleteAsync(entity)`, entity isolation, non-concurrent entities |
| `UserTests` | System-principal wiring (`SYSTEM` has no creator, `ADMIN` created by `SYSTEM`) |
