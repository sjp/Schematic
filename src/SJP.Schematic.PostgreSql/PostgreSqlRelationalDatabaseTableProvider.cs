using System;
using System.Collections.Generic;
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
    /// <summary>
    /// A database table provider for PostgreSQL.
    /// </summary>
    /// <seealso cref="IRelationalDatabaseTableProvider" />
    public class PostgreSqlRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">A database identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
        public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));

            _tableProvider = new AsyncLazy<Option<IRelationalDatabaseTableProvider>>(LoadVersionedTableProvider);
        }

        /// <summary>
        /// A database connection that is specific to a given PostgreSQL database.
        /// </summary>
        /// <value>A database connection.</value>
        protected ISchematicConnection Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets an identifier resolver that enables more relaxed matching against database object names.
        /// </summary>
        /// <value>An identifier resolver.</value>
        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// A database connection factory used to query the database.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// The dialect for the associated database.
        /// </summary>
        /// <value>A database dialect.</value>
        protected IDatabaseDialect Dialect => Connection.Dialect;

        /// <summary>
        /// Gets a database column type provider.
        /// </summary>
        /// <value>A type provider.</value>
        protected IDbTypeProvider TypeProvider => Dialect.TypeProvider;

        /// <summary>
        /// Gets all database tables.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        public async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var provider = await _tableProvider.ConfigureAwait(false);
            var tables = provider.Match(
                tp => tp.GetAllTables(cancellationToken),
                AsyncEnumerable.Empty<IRelationalDatabaseTable>
            );

            await foreach (var table in tables.WithCancellation(cancellationToken).ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return table;
        }

        /// <summary>
        /// Gets a database table.
        /// </summary>
        /// <param name="tableName">A database table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
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
                [new Version(9, 6)] = () => new PostgreSqlRelationalDatabaseTableProviderBase(Connection, IdentifierDefaults, IdentifierResolver),
                [new Version(10, 0)] = () => new Versions.V10.PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver),
                [new Version(11, 0)] = () => new Versions.V11.PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver),
                [new Version(12, 0)] = () => new Versions.V12.PostgreSqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults, IdentifierResolver)
            };
            var versionLookup = new VersionResolvingFactory<IRelationalDatabaseTableProvider>(factories);
            var result = versionLookup.GetValue(version);

            return Option<IRelationalDatabaseTableProvider>.Some(result);
        }

        private readonly AsyncLazy<Option<IRelationalDatabaseTableProvider>> _tableProvider;
    }
}
