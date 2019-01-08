using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    /// <summary>
    /// A set of rules for determining namespace, class and property names for a database mapping object.
    /// </summary>
    public interface INameTranslator
    {
        /// <summary>
        /// Return a namespace name for a schema qualified object name.
        /// </summary>
        /// <param name="objectName">An optionally qualified object name.</param>
        /// <returns><c>null</c> if <paramref name="objectName"/> does not contain a schema name or should not be used.</returns>
        string SchemaToNamespace(Identifier objectName);

        /// <summary>
        /// Return a name for a table.
        /// </summary>
        /// <param name="tableName">An optionally qualified table name.</param>
        /// <returns>A class name.</returns>
        string TableToClassName(Identifier tableName);

        /// <summary>
        /// Return a name for a view.
        /// </summary>
        /// <param name="viewName">An optionally qualified view name.</param>
        /// <returns>A class name.</returns>
        string ViewToClassName(Identifier viewName);

        /// <summary>
        /// Return a property name for a column.
        /// </summary>
        /// <param name="className">The name of the class that the property belongs to.</param>
        /// <param name="columnName">A column name.</param>
        /// <returns>A property name.</returns>
        string ColumnToPropertyName(string className, string columnName);
    }
}
