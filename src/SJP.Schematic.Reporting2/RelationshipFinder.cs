using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting;

internal sealed class RelationshipFinder
{
    public RelationshipFinder(IEnumerable<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var tableLookup = new Dictionary<Identifier, IRelationalDatabaseTable>();
        foreach (var table in tables)
            tableLookup[table.Name] = table;

        Tables = tableLookup;
    }

    private IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Tables { get; }

    public IEnumerable<IRelationalDatabaseTable> GetTablesByDegrees(IRelationalDatabaseTable table, uint degrees)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new Dictionary<Identifier, IRelationalDatabaseTable> { [table.Name] = table };
        AddRelatedTables(result, [table], degrees);

        return result.Values;
    }

    private void AddRelatedTables(IDictionary<Identifier, IRelationalDatabaseTable> tableLookup, IEnumerable<IRelationalDatabaseTable> tables, uint degree)
    {
        if (tables.Empty() || degree == 0)
            return;

        var addedTables = new Dictionary<Identifier, IRelationalDatabaseTable>();

        foreach (var table in tables)
        {
            var childKeys = table.ChildKeys;
            var parentKeys = table.ParentKeys;

            foreach (var childKey in childKeys)
            {
                var childTableName = childKey.ChildTable;
                var isNewTable = !tableLookup.ContainsKey(childTableName) && !addedTables.ContainsKey(childTableName);
                if (!isNewTable)
                    continue;

                if (Tables.TryGetValue(childTableName, out var childTable))
                    addedTables[childTableName] = childTable;
            }

            foreach (var parentKey in parentKeys)
            {
                var parentTableName = parentKey.ParentTable;
                var isNewTable = !tableLookup.ContainsKey(parentTableName) && !addedTables.ContainsKey(parentTableName);
                if (!isNewTable)
                    continue;

                if (Tables.TryGetValue(parentTableName, out var parentTable))
                    addedTables[parentTableName] = parentTable;
            }
        }

        foreach (var addedTable in addedTables)
            tableLookup[addedTable.Key] = addedTable.Value;

        var newDegree = degree - 1;
        AddRelatedTables(tableLookup, addedTables.Values, newDegree);
    }
}