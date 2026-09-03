namespace SJP.Schematic.Core;

/// <summary>
/// Defines a container type that implements accessors for all database objects.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
/// <seealso cref="IDatabaseViewProvider" />
/// <seealso cref="IDatabaseSequenceProvider" />
/// <seealso cref="IDatabaseSynonymProvider" />
/// <seealso cref="IDatabaseRoutineProvider" />
/// <seealso cref="IDatabaseUserDefinedTypeProvider" />
public interface IRelationalDatabase
    : IRelationalDatabaseTableProvider,
      IDatabaseViewProvider,
      IDatabaseSequenceProvider,
      IDatabaseSynonymProvider,
      IDatabaseRoutineProvider,
      IDatabaseUserDefinedTypeProvider
{
    /// <summary>
    /// Identifier defaults. Used to determine the default name resolution applied to the database.
    /// </summary>
    /// <value>The identifier defaults.</value>
    IIdentifierDefaults IdentifierDefaults { get; }
}