* Get unit and integration tests working on AppVeyor once we can run the new
  `dotnet test` tooling on it. Should be able to run every DB vendor through
  AppVeyor except for Oracle (no surprises there...).

* Use Sigil for reflection? Is it worthwhile or perhaps just use Fasterflect
  if it's beneficial.

* When analyzing schema use observables to continue and collect multiple errors.

* Create dummy output for databases so that scripts can be generated (perhaps
  a dry-run option?)

* Use string resources for translation -- primarily exception messages are affected.

* Simplify type declarations and descriptions as much as possible. Want it to be
  painless and easy.

* Fix generic types for queryables and indexes? Do we want them to be dependent
  on a type such as table/view or generic?

* Think about what is common for different database vendors, e.g. Redis, Cassandra,
  SQL Server, Postgres, etc.

* Add an IsEnabled property for constraints and triggers?

* Split into multiple assemblies. This is mostly done but perhaps split further for
  migrations assemblies.

* Once types and API are settled, minimise API surface by making objects sealed or
  internal as much as possible.

* Create a plugin based assembly that can perform certain tasks.
  * Make it readline based?
  * Want to be able to do things like generate ORM POCOs (e.g. Entity Framework, Dapper, etc)
    in addition to linting schema.

* Linters should be configurable and have different severity, i.e. hard errors, warnings, info.
  additionally rules can be configurable. Want to be able to detect things like missing
  indexes on FKs and also potentially missing foreign keys (if column names match certain patterns).

* Computed columns. Not widely supported, perhaps just Oracle and SQL Server only?

* Full-text indexes? Not sure how generic they are or whether they need to be treated any
  differently in the declarations. Maybe just an attribute stating that it's a full-text index?

* Add support for Oracle, Postgres and Mysql. These are probably sufficient, and cover the most
  widely used databases, especially in .NET.

* Add or implement logging. Create providers for common implementations, e.g.
  log4net, serilog, etc. Create a common interface and set statically.
