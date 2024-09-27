using System;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// A set of rules for determining the class and property names for a database mapping object.
/// </summary>
public class SnakeCaseNameTranslator : NameTranslator
{
    /// <summary>
    /// Return a namespace name for a schema qualified object name.
    /// </summary>
    /// <param name="objectName">An optionally qualified object name.</param>
    /// <returns><c>null</c> if <paramref name="objectName"/> does not contain a schema name or should not be used.</returns>
    public override string? SchemaToNamespace(Identifier objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        if (objectName.Schema == null)
            return null;

        var schemaIdentifier = CreateValidIdentifier(objectName.Schema);
        return schemaIdentifier?.Underscore();
    }

    /// <summary>
    /// Return a name for a table.
    /// </summary>
    /// <param name="tableName">An optionally qualified table name.</param>
    /// <returns>A class name.</returns>
    public override string TableToClassName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var tableIdentifier = CreateValidIdentifier(tableName.LocalName);
        return tableIdentifier.Underscore();
    }

    /// <summary>
    /// Return a name for a view.
    /// </summary>
    /// <param name="viewName">An optionally qualified view name.</param>
    /// <returns>A class name.</returns>
    public override string ViewToClassName(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var viewIdentifier = CreateValidIdentifier(viewName.LocalName);
        return viewIdentifier.Underscore();
    }

    /// <summary>
    /// Return a property name for a column.
    /// </summary>
    /// <param name="className">The name of the class the column is a member of.</param>
    /// <param name="columnName">A column name.</param>
    /// <returns>A property name.</returns>
    public override string ColumnToPropertyName(string className, string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(className);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var isValid = IsValidIdentifier(columnName);
        var columnIdentifier = isValid
            ? columnName
            : CreateValidIdentifier(className, columnName);

        var result = columnIdentifier.Underscore();
        return string.Equals(result, className, StringComparison.Ordinal)
            ? result + "_"
            : result;
    }
}