using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Threading.Tasks;
using System.Threading;

namespace SJP.Schematic.Reporting
{
    internal sealed class RelationshipFinder
    {
        public RelationshipFinder(IRelationalDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        private IRelationalDatabase Database { get; }

        public Task<IEnumerable<IRelationalDatabaseTable>> GetTablesByDegreesAsync(IRelationalDatabaseTable table, uint degrees, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return GetTablesByDegreesCore(table, degrees, cancellationToken);
        }

        private async Task<IEnumerable<IRelationalDatabaseTable>> GetTablesByDegreesCore(IRelationalDatabaseTable table, uint degrees, CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IRelationalDatabaseTable> { [table.Name] = table };

            await AddRelatedTablesAsync(result, new[] { table }, degrees, cancellationToken).ConfigureAwait(false);

            return result.Values;
        }

        private async Task AddRelatedTablesAsync(IDictionary<Identifier, IRelationalDatabaseTable> tableLookup, IEnumerable<IRelationalDatabaseTable> tables, uint degree, CancellationToken cancellationToken)
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

                    var childTable = Database.GetTable(childTableName, cancellationToken);
                    await childTable.IfSome(t => addedTables[childTableName] = t).ConfigureAwait(false);
                }

                foreach (var parentKey in parentKeys)
                {
                    var parentTableName = parentKey.ParentTable;
                    var isNewTable = !tableLookup.ContainsKey(parentTableName) && !addedTables.ContainsKey(parentTableName);
                    if (!isNewTable)
                        continue;

                    var parentTable = Database.GetTable(parentTableName, cancellationToken);
                    await parentTable.IfSome(t => addedTables[parentTableName] = t).ConfigureAwait(false);
                }
            }

            foreach (var addedTable in addedTables)
                tableLookup[addedTable.Key] = addedTable.Value;

            var newDegree = degree - 1;
            await AddRelatedTablesAsync(tableLookup, addedTables.Values, newDegree, cancellationToken).ConfigureAwait(false);
        }
    }
}
