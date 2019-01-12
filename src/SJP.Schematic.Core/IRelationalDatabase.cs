namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase
        : IRelationalDatabaseTableProvider,
          IDatabaseViewProvider,
          IDatabaseSequenceProvider,
          IDatabaseSynonymProvider
    {
        IDatabaseDialect Dialect { get; }

        IIdentifierDefaults IdentifierDefaults { get; }
    }
}
