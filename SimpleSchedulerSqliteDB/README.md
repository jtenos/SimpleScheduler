# SimpleSchedulerSqliteDB

SQLite schema (tables + views) and an auto-initializer for running SimpleScheduler against a local
SQLite file instead of SQL Server. Select the provider with the `Database:Provider` config key
(`SqlServer` (default) or `Sqlite`); the SQLite file location is `Database:SqlitePath`.

The `.sql` files under `Schema/` are embedded resources. `SqliteSchemaInitializer.EnsureSchemaAsync`
runs them (all `CREATE ... IF NOT EXISTS`, so idempotent) on first connection, so the database file
and tables are created automatically on startup.

## Notes on the SQLite port (issue #63)

SQLite has no stored procedures, so the procedure logic from `SimpleSchedulerSqlServerDB` lives in
the `SimpleSchedulerAppServices.Implementations.Sqlite` managers as SQL scripts. Key differences:

- **Dapper is still used everywhere.** The one place SQL Server's approach doesn't translate is
  **table-valued parameters** (`[app].[BigIntArray]`, used by `Schedules_SelectMany`,
  `Workers_SelectMany`, and `Jobs_RunNow`). SQLite has no TVPs, so the SQLite managers pass a
  `long[]` and rely on Dapper's `WHERE ID IN @IDs` list expansion (`DapperFluentExtensions.AddIdListParam`).

- **Dates/times are stored as ISO-8601 TEXT.** `SimpleSchedulerData.SqliteTypeHandlers` registers
  Dapper type handlers so `DateTime` round-trips as `yyyy-MM-ddTHH:mm:ss.fffffffZ` (UTC) and
  `TimeSpan` (time-of-day) round-trips as `HH:mm:ss`. This keeps the values usable by SQLite's
  `datetime()`/`strftime()` functions.

- **Dapper + SQLite type quirks (handled by `SqliteTypeHandlers`).** SQLite returns every integer as
  `Int64`, and Dapper cannot feed an `Int64` into a `bool` or `int` **constructor** parameter (the
  entity records use `bool`/`int`). Handlers for `bool` and `int` perform that conversion. (Result
  DTOs that come back as `0/1` use settable-property classes for the same reason.)

- **Guids.** Microsoft.Data.Sqlite binds `Guid` parameters as **uppercase** canonical TEXT, so the
  `Jobs.AcknowledgementCode` default generates an uppercase GUID string (SQLite has no `NEWID()`).
  Login validation codes are generated in C# and stored the same way.

- **T-SQL constructs replaced:** `OUTPUT … INTO` → a temp table + `UPDATE`/`SELECT` (Jobs_Dequeue);
  `SCOPE_IDENTITY()` → `last_insert_rowid()`; the circular-reference `WHILE` loop → a `WITH RECURSIVE`
  CTE (Workers_Update); `FORMAT(SYSUTCDATETIME(), …)` → `strftime(...)`; `sp_executesql` dynamic SQL →
  conditional clauses built in C#; `OFFSET/FETCH` → `LIMIT/OFFSET`.

- **Concurrency.** Every SQLite connection is opened with `PRAGMA journal_mode=WAL` and
  `PRAGMA busy_timeout=5000` (plus `foreign_keys=ON`) to support multiple active connections.

- **Minor behavioral difference:** acknowledging an error that is missing or already acknowledged
  throws the same `ApplicationException` messages as SQL Server, but as a managed exception rather
  than a `RAISERROR` from the database.
