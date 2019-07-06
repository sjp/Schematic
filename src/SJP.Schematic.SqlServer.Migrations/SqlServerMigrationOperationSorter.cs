using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.SqlServer.Migrations
{
    // TODO: have a look here for Sort() to complete
    // https://github.com/aspnet/EntityFrameworkCore/blob/master/src/EFCore.Relational/Migrations/Internal/MigrationsModelDiffer.cs
    public class SqlServerMigrationOperationSorter : IMigrationOperationSorter
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
            typeof(DropSequenceOperation),
            typeof(DropColumnOperation),
            typeof(DropTableOperation),
            typeof(RenameTableOperation),
            typeof(CreateSequenceOperation),
            typeof(CreateTableOperation),
            typeof(CreateViewOperation),
        };
    }
}
