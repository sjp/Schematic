using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled
{
    /// <summary>
    /// A relational database that is built from constituent relational databases.
    /// </summary>
    /// <seealso cref="IRelationalDatabase" />
    public class OrderedRelationalDatabase : IRelationalDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedRelationalDatabase"/> class.
        /// </summary>
        /// <param name="databases">An ordered set of databases, later objects are preferred over earlier objects.</param>
        /// <exception cref="ArgumentNullException"><paramref name="databases"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="databases"/> is empty.</exception>
        public OrderedRelationalDatabase(IEnumerable<IRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Empty())
                throw new ArgumentException("At least one database must be present in the collection of databases", nameof(databases));

            Databases = databases.ToList();
            BaseDatabase = Databases.Last();
        }

        /// <summary>
        /// Identifier defaults. Used to determine the default name resolution applies to the database.
        /// </summary>
        /// <value>The identifier defaults.</value>
        public IIdentifierDefaults IdentifierDefaults => BaseDatabase.IdentifierDefaults;

        /// <summary>
        /// The base database.
        /// </summary>
        /// <value>A base relational database.</value>
        protected IRelationalDatabase BaseDatabase { get; }

        /// <summary>
        /// The databases to use for resolution.
        /// </summary>
        /// <value>An ordered collection of relational databases.</value>
        protected IEnumerable<IRelationalDatabase> Databases { get; }

        /// <summary>
        /// Gets a database table, taking the first found table, if available.
        /// </summary>
        /// <param name="tableName">A database table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTable(tableName, cancellationToken);
        }

        /// <summary>
        /// Gets all database tables, while also respecting the layering preferences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database tables.</returns>
        public async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tablesTasks = Databases.Select(d => d.GetAllTables(cancellationToken).ToListAsync(cancellationToken).AsTask());
            var tableCollections = await Task.WhenAll(tablesTasks).ConfigureAwait(false);
            var tables = tableCollections
                .SelectMany(t => t)
                .DistinctBy(t => t.Name)
                .OrderBy(t => t.Name.Schema)
                .ThenBy(t => t.Name.LocalName);

            foreach (var table in tables)
                yield return table;
        }

        /// <summary>
        /// Loads the first available table from the provided database.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var tables = await Databases
                .Select(d => d.GetTable(tableName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return tables.HeadOrNone();
        }

        /// <summary>
        /// Gets a database view, taking the first found view, if available.
        /// </summary>
        /// <param name="viewName">A database view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadView(viewName, cancellationToken);
        }

        /// <summary>
        /// Gets all database views, while also respecting the layering preferences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database views.</returns>
        public async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var viewsTasks = Databases.Select(d => d.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask());
            var viewCollections = await Task.WhenAll(viewsTasks).ConfigureAwait(false);
            var views = viewCollections
                .SelectMany(v => v)
                .DistinctBy(v => v.Name)
                .OrderBy(v => v.Name.Schema)
                .ThenBy(v => v.Name.LocalName);

            foreach (var view in views)
                yield return view;
        }

        /// <summary>
        /// Loads the first available view from the provided database.
        /// </summary>
        /// <param name="viewName">A view name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A view, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var views = await Databases
                .Select(d => d.GetView(viewName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.HeadOrNone();
        }

        /// <summary>
        /// Gets a database sequence, taking the first found sequence, if available.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequence(sequenceName, cancellationToken);
        }

        /// <summary>
        /// Gets all database sequences, while also respecting the layering preferences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database sequences.</returns>
        public async IAsyncEnumerable<IDatabaseSequence> GetAllSequences([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var sequencesTasks = Databases.Select(d => d.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).AsTask());
            var sequenceCollections = await Task.WhenAll(sequencesTasks).ConfigureAwait(false);
            var sequences = sequenceCollections
                .SelectMany(s => s)
                .DistinctBy(v => v.Name)
                .OrderBy(v => v.Name.Schema)
                .ThenBy(v => v.Name.LocalName);

            foreach (var sequence in sequences)
                yield return sequence;
        }

        /// <summary>
        /// Loads the first available sequence from the provided database.
        /// </summary>
        /// <param name="sequenceName">A sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A sequence, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var sequences = await Databases
                .Select(d => d.GetSequence(sequenceName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return sequences.HeadOrNone();
        }

        /// <summary>
        /// Gets a database synonym, taking the first found synonym, if available.
        /// </summary>
        /// <param name="synonymName">A database synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database synonym in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonym(synonymName, cancellationToken);
        }

        /// <summary>
        /// Gets all database synonyms, while also respecting the layering preferences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database synonyms.</returns>
        public async IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var synonymsTasks = Databases.Select(d => d.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).AsTask());
            var synonymCollections = await Task.WhenAll(synonymsTasks).ConfigureAwait(false);
            var synonyms = synonymCollections
                .SelectMany(s => s)
                .DistinctBy(s => s.Name)
                .OrderBy(s => s.Name.Schema)
                .ThenBy(s => s.Name.LocalName);

            foreach (var synonym in synonyms)
                yield return synonym;
        }

        /// <summary>
        /// Loads the first available synonym from the provided database.
        /// </summary>
        /// <param name="synonymName">A synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A synonym, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonym(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var synonyms = await Databases
                .Select(d => d.GetSynonym(synonymName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return synonyms.HeadOrNone();
        }

        /// <summary>
        /// Gets a database routine, taking the first found routine, if available.
        /// </summary>
        /// <param name="routineName">A database routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return LoadRoutine(routineName, cancellationToken);
        }

        /// <summary>
        /// Gets all database routines, while also respecting the layering preferences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database routines.</returns>
        public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var routinesTasks = Databases.Select(d => d.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask());
            var routineCollections = await Task.WhenAll(routinesTasks).ConfigureAwait(false);
            var routines = routineCollections
                .SelectMany(s => s)
                .DistinctBy(r => r.Name)
                .OrderBy(r => r.Name.Schema)
                .ThenBy(r => r.Name.LocalName);

            foreach (var routine in routines)
                yield return routine;
        }

        /// <summary>
        /// Loads the first available routine from the provided database.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseRoutine> LoadRoutine(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return LoadRoutineAsyncCore(routineName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseRoutine>> LoadRoutineAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var routines = await Databases
                .Select(d => d.GetRoutine(routineName, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return routines.HeadOrNone();
        }
    }
}
