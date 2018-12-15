namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase
        : IRelationalDatabaseTableProvider,
          IRelationalDatabaseViewProvider,
          IDatabaseSequenceProvider,
          IDatabaseSynonymProvider
    {
        IDatabaseDialect Dialect { get; }

        string DatabaseVersion { get; }

        string ServerName { get; }

        string DatabaseName { get; }

        string DefaultSchema { get; }
    }
}
