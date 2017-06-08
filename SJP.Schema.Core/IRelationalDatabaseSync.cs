using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseSync
    {
        // make sure that this takes a dialect provider so that we can do quoting per vendor && version
        string DefaultSchema { get; }
        string DatabaseName { get; }

        bool TableExists(Identifier tableName);

        IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table { get; }

        IEnumerable<IRelationalDatabaseTable> Tables { get; }

        bool ViewExists(Identifier viewName);

        IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View { get; }

        IEnumerable<IRelationalDatabaseView> Views { get; }

        bool SequenceExists(Identifier sequenceName);

        IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence { get; }

        IEnumerable<IDatabaseSequence> Sequences { get; }

        bool SynonymExists(Identifier synonymName);

        IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym { get; }

        IEnumerable<IDatabaseSynonym> Synonyms { get; }

        bool TriggerExists(Identifier triggerName);

        IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        IEnumerable<IDatabaseTrigger> Triggers { get; }
    }
}
