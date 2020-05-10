using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled
{
    /// <summary>
    /// Builds a relational database in a layered approach using multiple relational databases as sources.
    /// </summary>
    public interface IRelationalDatabaseBuilder
    {
        /// <summary>
        /// Overrides any existing database objects with a provided database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>A relational database builder with the given relational database applied.</returns>
        IRelationalDatabaseBuilder OverrideWith(IRelationalDatabase database);

        /// <summary>
        /// Overrides any existing database objects with a provided database as returned by a given factory.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <returns>A relational database builder with the given relational database applied.</returns>
        IRelationalDatabaseBuilder OverrideWith(Func<IRelationalDatabase> databaseFactory);

        /// <summary>
        /// Builds a relational database from the given databases.
        /// </summary>
        /// <returns>A relational database instance.</returns>
        IRelationalDatabase Build();
    }
}
