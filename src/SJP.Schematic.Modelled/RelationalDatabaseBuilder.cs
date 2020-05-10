using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled
{
    /// <summary>
    /// A relational database builder.
    /// </summary>
    /// <seealso cref="IRelationalDatabaseBuilder" />
    public class RelationalDatabaseBuilder : IRelationalDatabaseBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalDatabaseBuilder"/> class.
        /// </summary>
        /// <param name="database">A base relational database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>null</c>.</exception>
        public RelationalDatabaseBuilder(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            _databases = new List<IRelationalDatabase> { database };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalDatabaseBuilder"/> class.
        /// </summary>
        /// <param name="databaseFactory">A base database factory.</param>
        /// <exception cref="ArgumentNullException"><paramref name="databaseFactory"/> is <c>null</c>.</exception>
        public RelationalDatabaseBuilder(Func<IRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            _databases = new List<IRelationalDatabase> { result };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalDatabaseBuilder"/> class.
        /// </summary>
        /// <param name="databases">An ordered set of relational databases to build with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="databases"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="databases"/> is empty.</exception>
        protected RelationalDatabaseBuilder(IEnumerable<IRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Empty())
                throw new ArgumentException("At least one database must be provided.", nameof(databases));

            _databases = databases.ToList();
        }

        /// <summary>
        /// Overrides any existing database objects with a provided database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>A relational database builder with the given relational database applied.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="database"/> is <c>null</c>.</exception>
        public IRelationalDatabaseBuilder OverrideWith(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var appendedDatabases = new List<IRelationalDatabase>(_databases) { database };
            return new RelationalDatabaseBuilder(appendedDatabases);
        }

        /// <summary>
        /// Overrides any existing database objects with a provided database as returned by a given factory.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <returns>A relational database builder with the given relational database applied.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="databaseFactory"/> is <c>null</c>.</exception>
        public IRelationalDatabaseBuilder OverrideWith(Func<IRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            var appendedDatabases = new List<IRelationalDatabase>(_databases) { result };
            return new RelationalDatabaseBuilder(appendedDatabases);
        }

        /// <summary>
        /// Builds a relational database from the given databases.
        /// </summary>
        /// <returns>A relational database instance.</returns>
        public IRelationalDatabase Build() => new OrderedRelationalDatabase(_databases.Reverse());

        private readonly IList<IRelationalDatabase> _databases;
    }
}
