using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests.Fakes
{
    internal class FakeRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
        }

        public virtual string ServerName { get; set; }

        public virtual string DatabaseName { get; set; }

        public virtual string DefaultSchema { get; set; }

        public virtual IEnumerable<IRelationalDatabaseTable> Tables { get; set; } = new List<IRelationalDatabaseTable>();

        public virtual IEnumerable<IRelationalDatabaseView> Views { get; set; } = new List<IRelationalDatabaseView>();

        public virtual IEnumerable<IDatabaseSequence> Sequences { get; set; } = new List<IDatabaseSequence>();

        public virtual IEnumerable<IDatabaseSynonym> Synonyms { get; set; } = new List<IDatabaseSynonym>();

        public virtual IDatabaseSequence GetSequence(Identifier sequenceName) => null;

        public virtual Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName) => Task.FromResult<IDatabaseSequence>(null);

        public virtual IDatabaseSynonym GetSynonym(Identifier synonymName) => null;

        public virtual Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName) => Task.FromResult<IDatabaseSynonym>(null);

        public virtual IRelationalDatabaseTable GetTable(Identifier tableName) => null;

        public virtual Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName) => Task.FromResult<IRelationalDatabaseTable>(null);

        public virtual IRelationalDatabaseView GetView(Identifier viewName) => null;

        public virtual Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName) => Task.FromResult<IRelationalDatabaseView>(null);

        public virtual bool SequenceExists(Identifier sequenceName) => false;

        public virtual Task<bool> SequenceExistsAsync(Identifier sequenceName) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync() => Task.FromResult(Sequences.ToAsyncEnumerable());

        public virtual bool SynonymExists(Identifier synonymName) => false;

        public virtual Task<bool> SynonymExistsAsync(Identifier synonymName) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync() => Task.FromResult(Synonyms.ToAsyncEnumerable());

        public virtual bool TableExists(Identifier tableName) => false;

        public virtual Task<bool> TableExistsAsync(Identifier tableName) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync() => Task.FromResult(Tables.ToAsyncEnumerable());

        public virtual bool ViewExists(Identifier viewName) => false;

        public virtual Task<bool> ViewExistsAsync(Identifier viewName) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync() => Task.FromResult(Views.ToAsyncEnumerable());
    }
}
