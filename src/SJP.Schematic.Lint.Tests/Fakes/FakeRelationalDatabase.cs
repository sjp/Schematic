﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
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

        public virtual Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IDatabaseSequence>(null);

        public virtual IDatabaseSynonym GetSynonym(Identifier synonymName) => null;

        public virtual Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IDatabaseSynonym>(null);

        public virtual IRelationalDatabaseTable GetTable(Identifier tableName) => null;

        public virtual Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IRelationalDatabaseTable>(null);

        public virtual IRelationalDatabaseView GetView(Identifier viewName) => null;

        public virtual Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IRelationalDatabaseView>(null);

        public virtual bool SequenceExists(Identifier sequenceName) => false;

        public virtual Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Sequences.ToAsyncEnumerable());

        public virtual bool SynonymExists(Identifier synonymName) => false;

        public virtual Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Synonyms.ToAsyncEnumerable());

        public virtual bool TableExists(Identifier tableName) => false;

        public virtual Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Tables.ToAsyncEnumerable());

        public virtual bool ViewExists(Identifier viewName) => false;

        public virtual Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(false);

        public virtual Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Views.ToAsyncEnumerable());
    }
}
