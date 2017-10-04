using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseSync
    {
        string ServerName { get; }

        string DatabaseName { get; }

        string DefaultSchema { get; }

        bool TableExists(Identifier tableName);

        IRelationalDatabaseTable GetTable(Identifier tableName);

        IEnumerable<IRelationalDatabaseTable> Tables { get; }

        bool ViewExists(Identifier viewName);

        IRelationalDatabaseView GetView(Identifier viewName);

        IEnumerable<IRelationalDatabaseView> Views { get; }

        bool SequenceExists(Identifier sequenceName);

        IDatabaseSequence GetSequence(Identifier sequenceName);

        IEnumerable<IDatabaseSequence> Sequences { get; }

        bool SynonymExists(Identifier synonymName);

        IDatabaseSynonym GetSynonym(Identifier synonymName);

        IEnumerable<IDatabaseSynonym> Synonyms { get; }
    }
}
