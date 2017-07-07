# TODO

* Simplify type declarations and descriptions as much as possible. Want it to be
  painless and easy.

* Fix generic types for queryables and indexes? Do we want them to be dependent
  on a type such as table/view or generic?

* Add an IsEnabled property for constraints and triggers?

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

* Think about dependencies/dependents. Seems clumsy doing this in the user interface.
  Most of the time we can determine this anyway, for example we can order based on
  foreign keys for table dependencies.

* Interfaces for `IRelationalDatabaseTable` and such need to be changed.

  These need to be able to use `Identifier` objects. The main reason for this is because
  we need to re-use the comparer objects throughout the entire database, so they need
  to be injected.

  For example, currently when a column lookup is loaded, its interface looks like
  `IReadOnlyDictionary<string, IDatabaseTableColumn>`. If we were expecting to query
  it in a case insensitive manner, then a column named `TESTCOLUMN` will not be found 
  if we search for it with `testcolumn` or `TestColumn`. This defeats the purpose of using
  a lookup in the first place if we have to enumerate it anyway.

  The consequence of this decision is that `IEqualityComparer<Identifier>` objects need
  to be passed into each and every layer, not just for caching.
