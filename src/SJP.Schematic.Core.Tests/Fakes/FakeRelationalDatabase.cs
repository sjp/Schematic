using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Tests.Fakes
{
    internal class FakeRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
        }

        public virtual IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; set; } = new List<IRelationalDatabaseTable>();

        public virtual IReadOnlyCollection<IDatabaseView> Views { get; set; } = new List<IDatabaseView>();

        public virtual IReadOnlyCollection<IDatabaseSequence> Sequences { get; set; } = new List<IDatabaseSequence>();

        public virtual IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; set; } = new List<IDatabaseSynonym>();

        public virtual IReadOnlyCollection<IDatabaseRoutine> Routines { get; set; } = new List<IDatabaseRoutine>();

        public virtual OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            return Sequences.Find(s => s.Name == sequenceName).ToAsync();
        }

        public virtual OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            return Synonyms.Find(s => s.Name == synonymName).ToAsync();
        }

        public virtual OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            return Tables.Find(t => t.Name == tableName).ToAsync();
        }

        public virtual OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            return Views.Find(v => v.Name == viewName).ToAsync();
        }

        public virtual OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            return Routines.Find(r => r.Name == routineName).ToAsync();
        }

        public virtual IAsyncEnumerable<IDatabaseSequence> GetAllSequences(CancellationToken cancellationToken = default) => Sequences.ToAsyncEnumerable();

        public virtual IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default) => Synonyms.ToAsyncEnumerable();

        public virtual IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default) => Tables.ToAsyncEnumerable();

        public virtual Task<IReadOnlyCollection<IDatabaseView>> GetAllViews(CancellationToken cancellationToken = default) => Task.FromResult(Views);

        public virtual Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default) => Task.FromResult(Routines);
    }
}