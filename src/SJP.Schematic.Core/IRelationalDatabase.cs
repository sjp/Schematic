namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase
        : IRelationalDatabaseTableProvider,
          IDatabaseViewProvider,
          IDatabaseSequenceProvider,
          IDatabaseSynonymProvider,
          IDatabaseRoutineProvider
    {
        IIdentifierDefaults IdentifierDefaults { get; }
    }
}
