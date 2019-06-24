﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class AddPrimaryKeyOperation : MigrationOperation
    {
        public AddPrimaryKeyOperation(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            PrimaryKey = primaryKey ?? throw new ArgumentNullException(nameof(primaryKey));
        }

        public IRelationalDatabaseTable Table { get; }

        public IDatabaseKey PrimaryKey { get; }
    }
}
