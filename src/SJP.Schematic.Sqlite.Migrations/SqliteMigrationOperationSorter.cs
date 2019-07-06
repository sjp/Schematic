using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.Sqlite.Migrations
{
    public class SqliteMigrationOperationSorter : IMigrationOperationSorter
    {
        public IEnumerable<IMigrationOperation> Sort(IEnumerable<IMigrationOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            return operations
                .OrderBy(op => Array.IndexOf(OrderedOperationTypes, op.GetType()))
                .ToList();
        }

        private readonly static Type[] OrderedOperationTypes = new[]
        {
            typeof(DropTriggerOperation),
            typeof(DropIndexOperation),
            typeof(DropForeignKeyOperation),
            typeof(DropUniqueKeyOperation),
            typeof(DropPrimaryKeyOperation),
            typeof(DropCheckOperation),
            typeof(DropColumnOperation),
            typeof(DropTableOperation),
            typeof(RenameTableOperation),
            typeof(CreateTableOperation),
            typeof(CreateViewOperation),
        };
    }
}
