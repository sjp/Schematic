using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    /// <summary>
    /// Defines comment information related to <see cref="IRelationalDatabaseTable"/> instances.
    /// </summary>
    public interface IRelationalDatabaseTableComments
    {
        /// <summary>
        /// The table this object refers to.
        /// </summary>
        /// <value>The name of the table.</value>
        Identifier TableName { get; }

        /// <summary>
        /// A comment for the table, if available.
        /// </summary>
        /// <value>A comment, if available.</value>
        Option<string> Comment { get; }

        /// <summary>
        /// A comment for the primary key, if available.
        /// </summary>
        /// <value>The primary key comment, if available.</value>
        Option<string> PrimaryKeyComment { get; }

        /// <summary>
        /// Comments defined for columns in the table.
        /// </summary>
        /// <value>The column comments. If no comment exists for the column, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }

        /// <summary>
        /// Comments defined for check constraints in the table.
        /// </summary>
        /// <value>The check constraint comments. If no comment exists for the check constraint, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> CheckComments { get; }

        /// <summary>
        /// Comments defined for unique constraints in the table.
        /// </summary>
        /// <value>The unique constraint comments. If no comment exists for the unique constraint, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> UniqueKeyComments { get; }

        /// <summary>
        /// Comments defined for foreign keys in the table.
        /// </summary>
        /// <value>The foreign key comments. If no comment exists for the foreign key constraint, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> ForeignKeyComments { get; }

        /// <summary>
        /// Comments defined for indexes on the table.
        /// </summary>
        /// <value>The index comments. If no comment exists for the index, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> IndexComments { get; }

        /// <summary>
        /// Comments defined for triggers on the table.
        /// </summary>
        /// <value>The trigger comments. If no comment exists for the trigger, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> TriggerComments { get; }
    }
}
