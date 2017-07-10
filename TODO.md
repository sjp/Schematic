# TODO

* Simplify type declarations and descriptions as much as possible. Want it to be
  painless and easy.

* Fix generic types for queryables and indexes? Do we want them to be dependent
  on a type such as table/view or generic?

* Split into multiple assemblies. This is mostly done but perhaps split further for
  migrations assemblies.

* Once types and API are settled, minimise API surface by making objects sealed or
  internal as much as possible.

* Computed columns. Not widely supported, perhaps just Oracle and SQL Server only?

* Full-text indexes? Not sure how generic they are or whether they need to be treated any
  differently in the declarations. Maybe just an attribute stating that it's a full-text index?

* Improve caching on all dependent objects. Needs to be able to handle tables, views, etc.
  Probably need to add an interface to know whether results are cached and that we can get
  better extension methods for it.

  This is in progress and caching can now be performed using IdentifierKeyedCache<T>.
  What is remaining is a nice wrapper for some improved caching. For example to avoid
  any duplication of queries within a table for example.

  IDEA: This can be achieved by querying the database for all related information.
  For example, a primary key needs to know about columns, so it could get cached
  column information by getting it from the table's database property. Something like
  `Database.GetTable(...).Column`. This would only work for the caching behaviour
  and would otherwise cause another couple of queries to be run. Not ideal. Leave it
  for now.

* Think about dependencies/dependents. Seems clumsy doing this in the user interface.
  Most of the time we can determine this anyway, for example we can order based on
  foreign keys for table dependencies.

* IDatabaseKey and IDatabaseIndex are insufficient at the moment for being able to
  handle indexes on things like functions, as found in Oracle for example.
  This means that instead of Columns we really should be doing something like
  expressions as the enumerable object, which should be an identity expression
  most of the time.

* For Oracle, we will have to do a test for whether the name must be quoted. Just a
  query that attempts to do it unquoted should be fine. If we can get the name
  unquoted then the name can be used in the uppercase form and should be translated.

  Do not allow name clashes when case-insensitivity is present. There is no good
  reason for this to be used so do not support it. Additionally this behaviour would
  not be portable across different vendors (SQLite for example is case-insensitive
  everywhere).

  In short, allow querying of information case-sensitively, but do not allow for the
  creation of a database where case-insensitiviy would break it.
