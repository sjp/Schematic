using System;
using System.Linq;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Threading.Tasks;

namespace SJP.Schematic.SchemaSpy
{
    internal class RelationshipFinder
    {
        public IEnumerable<IRelationalDatabaseTable> GetTablesByDegrees(IRelationalDatabaseTable table, uint degrees)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new Dictionary<Identifier, IRelationalDatabaseTable> { [table.Name] = table };

            AddRelatedTables(result, new[] { table }, degrees);

            return result.Values;
        }

        public Task<IEnumerable<IRelationalDatabaseTable>> GetTablesByDegreesAsync(IRelationalDatabaseTable table, uint degrees)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetTablesByDegreesCore(table, degrees);
        }

        private async Task<IEnumerable<IRelationalDatabaseTable>> GetTablesByDegreesCore(IRelationalDatabaseTable table, uint degrees)
        {
            var result = new Dictionary<Identifier, IRelationalDatabaseTable> { [table.Name] = table };

            await AddRelatedTablesAsync(result, new[] { table }, degrees).ConfigureAwait(false);

            return result.Values;
        }

        private static void AddRelatedTables(IDictionary<Identifier, IRelationalDatabaseTable> tableLookup, IEnumerable<IRelationalDatabaseTable> tables, uint degree)
        {
            if (tableLookup == null)
                throw new ArgumentNullException(nameof(tableLookup));
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            if (tables.Empty() || degree == 0)
                return;

            var addedTables = new Dictionary<Identifier, IRelationalDatabaseTable>();

            foreach (var table in tables)
            {
                var childKeys = table.ChildKeys.ToList();
                var parentKeys = table.ParentKeys.ToList();

                foreach (var childKey in childKeys)
                {
                    var childTableName = childKey.ChildKey.Table.Name;
                    var isNewTable = !tableLookup.ContainsKey(childTableName) && !addedTables.ContainsKey(childTableName);
                    if (isNewTable)
                        addedTables[childTableName] = childKey.ChildKey.Table;
                }

                foreach (var parentKey in parentKeys)
                {
                    var parentTableName = parentKey.ParentKey.Table.Name;
                    var isNewTable = !tableLookup.ContainsKey(parentTableName) && !addedTables.ContainsKey(parentTableName);
                    if (isNewTable)
                        addedTables[parentTableName] = parentKey.ParentKey.Table;
                }
            }

            foreach (var addedTable in addedTables)
                tableLookup[addedTable.Key] = addedTable.Value;

            var newDegree = degree - 1;
            AddRelatedTables(tableLookup, addedTables.Values, newDegree);
        }

        private static async Task AddRelatedTablesAsync(IDictionary<Identifier, IRelationalDatabaseTable> tableLookup, IEnumerable<IRelationalDatabaseTable> tables, uint degree)
        {
            if (tables.Empty() || degree == 0)
                return;

            var addedTables = new Dictionary<Identifier, IRelationalDatabaseTable>();

            foreach (var table in tables)
            {
                var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
                var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);

                foreach (var childKey in childKeys)
                {
                    var childTableName = childKey.ChildKey.Table.Name;
                    var isNewTable = !tableLookup.ContainsKey(childTableName) && !addedTables.ContainsKey(childTableName);
                    if (isNewTable)
                        addedTables[childTableName] = childKey.ChildKey.Table;
                }

                foreach (var parentKey in parentKeys)
                {
                    var parentTableName = parentKey.ParentKey.Table.Name;
                    var isNewTable = !tableLookup.ContainsKey(parentTableName) && !addedTables.ContainsKey(parentTableName);
                    if (isNewTable)
                        addedTables[parentTableName] = parentKey.ParentKey.Table;
                }
            }

            foreach (var addedTable in addedTables)
                tableLookup[addedTable.Key] = addedTable.Value;

            var newDegree = degree - 1;
            await AddRelatedTablesAsync(tableLookup, addedTables.Values, newDegree).ConfigureAwait(false);
        }
    }
}
