using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting;

internal static class RelationalDatabaseTableExtensions
{
    /// <summary>
    /// Collects the names of the columns in the table's primary key, unique keys and foreign keys.
    /// Names are compared ordinally.
    /// </summary>
    public static TableKeyColumns GetKeyColumns(this IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var primaryKeyColumns = new HashSet<string>(StringComparer.Ordinal);
        table.PrimaryKey.IfSome(primaryKey => AddColumnNames(primaryKeyColumns, primaryKey));

        var uniqueKeyColumns = new HashSet<string>(StringComparer.Ordinal);
        foreach (var uniqueKey in table.UniqueKeys)
            AddColumnNames(uniqueKeyColumns, uniqueKey);

        var foreignKeyColumns = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parentKey in table.ParentKeys)
            AddColumnNames(foreignKeyColumns, parentKey.ChildKey);

        return new TableKeyColumns(primaryKeyColumns, uniqueKeyColumns, foreignKeyColumns);
    }

    private static void AddColumnNames(HashSet<string> columnNames, IDatabaseKey key)
    {
        foreach (var column in key.Columns)
            columnNames.Add(column.Name.LocalName);
    }
}
