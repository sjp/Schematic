# TODO

* Simplify type declarations and descriptions as much as possible. Want it to be
  painless and easy.

* Once types and API are settled, minimise API surface by making objects sealed or
  internal as much as possible.

* Think about dependencies/dependents. Seems clumsy doing this in the user interface.
  Most of the time we can determine this anyway, for example we can order based on
  foreign keys for table dependencies.

* IDatabaseKey and IDatabaseIndex are insufficient at the moment for being able to
  handle indexes on things like functions, as found in Oracle for example.
  This means that instead of Columns we really should be doing something like
  expressions as the enumerable object, which should be an identity expression
  most of the time.