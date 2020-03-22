﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        public PostgreSqlDatabaseSequenceProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));

            _sequenceProvider = new AsyncLazy<Option<IDatabaseSequenceProvider>>(LoadVersionedSequenceProvider);
        }

        protected ISchematicConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        public async IAsyncEnumerable<IDatabaseSequence> GetAllSequences([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var provider = await _sequenceProvider.ConfigureAwait(false);
            var sequences = provider.Match(
                sp => sp.GetAllSequences(cancellationToken),
                AsyncEnumerable.Empty<IDatabaseSequence>
            );

            await foreach (var sequence in sequences.ConfigureAwait(false))
                yield return sequence;
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _sequenceProvider.Task
                .ToAsync()
                .Bind(sp => sp.GetSequence(sequenceName, cancellationToken));
        }

        private async Task<Option<IDatabaseSequenceProvider>> LoadVersionedSequenceProvider()
        {
            var version = await Dialect.GetDatabaseVersionAsync(Connection, CancellationToken.None).ConfigureAwait(false);

            var factories = new Dictionary<Version, Func<IDatabaseSequenceProvider>>
            {
                [new Version(9, 4)] = () => new Versions.V9_4.PostgreSqlDatabaseSequenceProvider(DbConnection, IdentifierDefaults, IdentifierResolver),
                [new Version(10, 0)] = () => new Versions.V10.PostgreSqlDatabaseSequenceProvider(DbConnection, IdentifierDefaults, IdentifierResolver)
            };
            var versionLookup = new VersionResolvingFactory<IDatabaseSequenceProvider>(factories);
            var result = versionLookup.GetValue(version);

            return Option<IDatabaseSequenceProvider>.Some(result);
        }

        private readonly AsyncLazy<Option<IDatabaseSequenceProvider>> _sequenceProvider;
    }
}
