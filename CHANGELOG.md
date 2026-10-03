# Changelog

Notable changes per released version, newest first. Versions before 10.2.13 are not documented here —
the Git history is the source for those.

## 10.3.1

### Fixed

* **A value object built from an incomplete set of values no longer throws `MissingMethodException`.**
  `CheckAndConvertValueObjectPatches` and `ToPatches` assemble a value object from the values that
  were sent and fill every constructor parameter without one. When the domain model had no value
  object yet — or always, in `ToPatches` — they filled it with a `DefaultExpression` object instead of
  the default value, so no constructor matched and creating the instance threw
  `Constructor on type '…' not found`. A typical case: an optional address added to a domain model
  that had none, without a house number. Missing parameters are `null` for reference types and the
  type's default for value types now. Missing parameters of an existing value object are still taken
  from it, as before.

## 10.3.0

### Added

* **The database connection can be created by another provider.** `DatabaseSettings` has a
  `protected virtual CreateConnection(string connectionString)`; it returns a `SqlConnection`, as
  `GetConnection` always did, and a subclass returns another connection instead — an
  `NpgsqlConnection` for PostgreSQL. The library itself does not depend on another provider.

### Changed

* **`Eshava.Storm` and `Eshava.Storm.Linq` 1.1.0**, up from 1.0.41 and 1.0.14. They bring the PostgreSQL
  dialect, and with 1.0.42 and 1.0.15 a set of fixes that change behaviour a consumer can notice —
  among them `DateTimeOffset` properties now being written and read, a partial update with an
  incomplete composite key being refused, and an owned object that is not set writing its columns as
  `NULL`. **Read the changelog of `Eshava.Storm` before updating**; its *Changed* sections list what to
  check.
* **`DateOnlyHandler` and `TimeOnlyHandler` no longer require a `SqlParameter`.** They set
  `DbType.Date` and `DbType.Time` instead of casting the parameter to `SqlParameter`, which threw an
  `InvalidCastException` with any other provider. On SqlClient the two are the same — `DbType.Date` is
  `SqlDbType.Date`, `DbType.Time` is `SqlDbType.Time` — and the value written is unchanged.

### Fixed

* **`DateOnlyHandler` and `TimeOnlyHandler` read what Npgsql returns.** Npgsql returns a `date` column
  as `DateOnly` and a `time` column as `TimeOnly`, where SqlClient returns `DateTime` and `TimeSpan`.
  The handlers accepted only the latter and returned the default value for anything else — silently,
  so every date and time read through Npgsql came back as `0001-01-01` and midnight. They accept both.

## 10.2.13

### Changed

* **A nested object sent as `null` in a partial put document now clears the value object.** Both
  converters — `PartialPutDocumentConverterTextJson` and `PartialPutDocumentConverterNewtonsoftJson` —
  emit a `Replace` operation with the value `null` for a class-typed property whose json value is
  `null`. Before, the converters descended into the null as if it were an object, found no children,
  and produced a layer without operations: the document said "no address any more" and nothing
  happened, silently.

  The rule a partial put document is built on is that a property left out means *unchanged* and a
  property contained is a *statement*. A scalar property already followed it — `"name": null`
  produced a replace with `null` and cleared the value — so the same token meant two different things
  depending on the property's type. It means one thing now.

  **What decides whether the clearing is allowed is the domain model**, as for every other patch: an
  optional value object is set to `null`, a required one refuses the patch through its validation.
  Nothing in the converters knows the difference.

  **This is a behaviour change for existing clients.** A client that serialises an untouched nested
  object as `null` — rather than leaving the property out — used to change nothing and now clears
  the value object. Check how your clients serialise empty nested objects before updating.

  Collections are not affected: an item is still removed through its explicit remove marker, and a
  collection property sent as `null` behaves as before.
