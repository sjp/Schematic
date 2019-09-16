using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public PostgreSqlRelationalDatabaseTableProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Dialect = new PostgreSqlDialect(connection);

            _tableProvider = new AsyncLazy<Option<IRelationalDatabaseTableProvider>>(LoadVersionedTableProvider);
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDatabaseDialect Dialect { get; }

        public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default)
        {
            var provider = await _tableProvider.Task.ConfigureAwait(false);
            var tablesTask = provider.Match(
                tp => tp.GetAllTables(cancellationToken),
                () => Empty.Tables
            );

            return await tablesTask.ConfigureAwait(false);
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
            var version = await Dialect.GetDatabaseVersionAsync(CancellationToken.None).ConfigureAwait(false);

            var factories = new Dictionary<Version, Func<IRelationalDatabaseTableProvider>>
            {
                [new Version(9, 4)] = () => new Versions.V9_4.PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver, TypeProvider),
                [new Version(11, 0)] = () => new Versions.V11.PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver, TypeProvider)
            };
            var versionLookup = new VersionResolvingFactory<IRelationalDatabaseTableProvider>(factories);
            var result = versionLookup.GetValue(version);

            return Option<IRelationalDatabaseTableProvider>.Some(result);
        }

        private readonly AsyncLazy<Option<IRelationalDatabaseTableProvider>> _tableProvider;
    }
}
