using System;
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
    public class PostgreSqlRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));

            _tableProvider = new AsyncLazy<Option<IRelationalDatabaseTableProvider>>(LoadVersionedTableProvider);
        }

        protected ISchematicConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IDatabaseDialect Dialect => Connection.Dialect;

        protected IDbTypeProvider TypeProvider => Dialect.TypeProvider;

        public async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var provider = await _tableProvider.ConfigureAwait(false);
            var tables = provider.Match(
                tp => tp.GetAllTables(cancellationToken),
                AsyncEnumerable.Empty<IRelationalDatabaseTable>
            );

            await foreach (var table in tables.WithCancellation(cancellationToken).ConfigureAwait(false))
                yield return table;
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return _tableProvider.Task
                .ToAsync()
                .Bind(tp => tp.GetTable(tableName, cancellationToken));
        }

        private async Task<Option<IRelationalDatabaseTableProvider>> LoadVersionedTableProvider()
        {
            var version = await Dialect.GetDatabaseVersionAsync(Connection, CancellationToken.None).ConfigureAwait(false);

            var factories = new Dictionary<Version, Func<IRelationalDatabaseTableProvider>>
            {
                [new Version(9, 5)] = () => new PostgreSqlRelationalDatabaseTableProviderBase(Connection, IdentifierDefaults, IdentifierResolver),
                [new Version(11, 0)] = () => new Versions.V11.PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver)
            };
            var versionLookup = new VersionResolvingFactory<IRelationalDatabaseTableProvider>(factories);
            var result = versionLookup.GetValue(version);

            return Option<IRelationalDatabaseTableProvider>.Some(result);
        }

        private readonly AsyncLazy<Option<IRelationalDatabaseTableProvider>> _tableProvider;
    }
}
