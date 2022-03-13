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
    /// A database sequence provider for PostgreSQL databases.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceProvider" />
    public class PostgreSqlDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseSequenceProvider"/> class.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlDatabaseSequenceProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));

            _sequenceProvider = new AsyncLazy<Option<IDatabaseSequenceProvider>>(LoadVersionedSequenceProvider);
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
        /// Gets all database sequences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database sequences.</returns>
        public async IAsyncEnumerable<IDatabaseSequence> GetAllSequences([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var provider = await _sequenceProvider.ConfigureAwait(false);
            var sequences = provider.Match(
                sp => sp.GetAllSequences(cancellationToken),
                AsyncEnumerable.Empty<IDatabaseSequence>
            );

            await foreach (var sequence in sequences.ConfigureAwait(false).WithCancellation(cancellationToken))
                yield return sequence;
        }

        /// <summary>
        /// Gets a database sequence.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
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
                [new Version(10, 0)] = () => new PostgreSqlDatabaseSequenceProviderBase(DbConnection, IdentifierDefaults, IdentifierResolver)
            };
            var versionLookup = new VersionResolvingFactory<IDatabaseSequenceProvider>(factories);
            var result = versionLookup.GetValue(version);

            return Option<IDatabaseSequenceProvider>.Some(result);
        }

        private readonly AsyncLazy<Option<IDatabaseSequenceProvider>> _sequenceProvider;
    }
}
