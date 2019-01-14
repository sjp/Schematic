namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase
        : IRelationalDatabaseTableProvider,
          IDatabaseViewProvider,
          IDatabaseSequenceProvider,
          IDatabaseSynonymProvider,
          IDatabaseRoutineProvider
    {
        IDatabaseDialect Dialect { get; }

        IIdentifierDefaults IdentifierDefaults { get; }
    }
}
