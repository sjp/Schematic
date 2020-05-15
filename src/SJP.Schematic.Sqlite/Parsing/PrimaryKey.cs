using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    /// <summary>
    /// The parsed definition of a primary key in a SQLite <c>CREATE TABLE</c> definition.
    /// </summary>
    public class PrimaryKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKey"/> class.
        /// </summary>
        /// <param name="constraintName">A name for the primary key, if available.</param>
        /// <param name="columnName">A column name that represents the column comprising the primary key.</param>
        public PrimaryKey(Option<string> constraintName, string columnName)
        {
            Name = constraintName;
            Columns = new IndexedColumn(columnName).ToEnumerable();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryKey"/> class.
        /// </summary>
        /// <param name="constraintName">A name for the primary key, if available.</param>
        /// <param name="columns">A collection of columns that comprise the primary key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <c>null</c> or empty.</exception>
        public PrimaryKey(Option<string> constraintName, IEnumerable<IndexedColumn> columns)
        {
            if (columns == null || columns.Empty())
                throw new ArgumentNullException(nameof(columns));

            Name = constraintName;
            Columns = columns;
        }

        /// <summary>
        /// The name of the primary key constraint, if available.
        /// </summary>
        /// <value>A constraint name, if available.</value>
        public Option<string> Name { get; }

        /// <summary>
        /// The columns comprising the primary key.
        /// </summary>
        /// <value>A collection of columns.</value>
        public IEnumerable<IndexedColumn> Columns { get; }
    }
}
